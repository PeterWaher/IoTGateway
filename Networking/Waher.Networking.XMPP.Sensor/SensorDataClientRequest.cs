using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Delegate for events triggered when a readout changes state.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="NewState">New State.</param>
	public delegate Task SensorDataReadoutStateChangedEventHandler(object Sender, SensorDataReadoutState NewState);

	/// <summary>
	/// Delegate for events triggered when readout errors have been received.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="NewErrors">New errors received. For a list of all errors received, see <see cref="SensorDataClientRequest.Errors"/>.</param>
	public delegate Task SensorDataReadoutErrorsReportedEventHandler(object Sender, IEnumerable<ThingError> NewErrors);

	/// <summary>
	/// Delegate for events triggered when readout fields have been received.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="NewFields">New fields received. For a list of all fields received, see <see cref="SensorDataClientRequest.ReadFields"/>.</param>
	public delegate Task SensorDataReadoutFieldsReportedEventHandler(object Sender, IEnumerable<Field> NewFields);

	/// <summary>
	/// Manages a sensor data client request.
	/// </summary>
	public class SensorDataClientRequest : SensorDataRequest
	{
		/// <summary>
		/// Reference to sensor client object.
		/// </summary>
		protected SensorClient sensorClient;

		private List<Field> readFields = null;
		private List<ThingError> errors = null;
		private SensorDataReadoutState state = SensorDataReadoutState.Requested;
		private readonly object synchObject = new object();
		private bool queued;

		/// <summary>
		/// Manages a sensor data client request.
		/// </summary>
		/// <param name="Id">Request identity.</param>
		/// <param name="SensorClient">Sensor client object.</param>
		/// <param name="RemoteJID">JID of the other side of the conversation in the sensor data readout.</param>
		/// <param name="Actor">Actor causing the request to be made.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="FieldNames">Names of fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="When">When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.</param>
		/// <param name="ServiceToken">Optional service token.</param>
		/// <param name="DeviceToken">Optional device token.</param>
		/// <param name="UserToken">Optional user token.</param>
		internal SensorDataClientRequest(string Id, SensorClient SensorClient, string RemoteJID, string Actor, IThingReference[] Nodes, FieldType Types,
			string[] FieldNames, DateTime From, DateTime To, DateTime When, string ServiceToken, string DeviceToken, string UserToken)
			: base(Id, RemoteJID, Actor, Nodes, Types, FieldNames, From, To, When, ServiceToken, DeviceToken, UserToken)
		{
			this.sensorClient = SensorClient;
		}

		/// <summary>
		/// Sensor Data Client.
		/// </summary>
		public SensorClient SensorClient { get { return this.sensorClient; } }

		/// <summary>
		/// Current state of readout.
		/// </summary>
		public SensorDataReadoutState State
		{
			get { return this.state; }
		}

		internal async Task SetState(SensorDataReadoutState NewState)
		{
			if (this.state != NewState)
			{
				this.state = NewState;

				SensorDataReadoutStateChangedEventHandler h = this.OnStateChanged;
				if (!(h is null))
				{
					try
					{
						await h(this, NewState);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		/// <summary>
		/// Event raised whenever the state of the sensor data readout changes.
		/// </summary>
		public event SensorDataReadoutStateChangedEventHandler OnStateChanged = null;

		/// <summary>
		/// Event raised whenever readout errors have been received. The event will report newest errors received.
		/// For a list of all errors received, see <see cref="Errors"/>.
		/// </summary>
		public event SensorDataReadoutErrorsReportedEventHandler OnErrorsReceived = null;

		/// <summary>
		/// Event raised whenever fields have been received. The event will report newest fields received.
		/// For a list of all fields received, see <see cref="ReadFields"/>.
		/// </summary>
		public event SensorDataReadoutFieldsReportedEventHandler OnFieldsReceived = null;

		internal async Task Fail(string Reason)
		{
			await this.LogErrors(new ThingError[] { new ThingError(string.Empty, string.Empty, string.Empty, DateTime.Now, Reason) });
			await this.SetState(SensorDataReadoutState.Failure);
		}

		internal virtual async Task LogErrors(IEnumerable<ThingError> Errors)
		{
			lock (this.synchObject)
			{
				if (this.errors is null)
					this.errors = new List<ThingError>();

				this.errors.AddRange(Errors);
			}

			SensorDataReadoutErrorsReportedEventHandler h = this.OnErrorsReceived;
			if (!(h is null))
			{
				try
				{
					await h(this, Errors);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		internal virtual async Task LogFields(IEnumerable<Field> Fields)
		{
			lock (this.synchObject)
			{
				if (this.readFields is null)
					this.readFields = new List<Field>();

				foreach (Field Field in Fields)
				{
					if (this.IsIncluded(Field.Name, Field.Timestamp, Field.Type))
						this.readFields.Add(Field);
				}
			}

			SensorDataReadoutFieldsReportedEventHandler h = this.OnFieldsReceived;
			if (!(h is null))
			{
				try
				{
					await h(this, Fields);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		internal void Clear()
		{
			lock (this.synchObject)
			{
				this.readFields?.Clear();
				this.errors?.Clear();
			}
		}

		internal Task Accept(bool Queued)
		{
			this.queued = Queued;
			return this.SetState(SensorDataReadoutState.Accepted);
		}

		/// <summary>
		/// Errors logged during the readout. If an error reference lacks a reference to a node (i.e its Node ID is the empty string),
		/// the error is an error relating to the readout itself, not a particular node.
		/// </summary>
		public ThingError[] Errors
		{
			get
			{
				lock (this.synchObject)
				{
					if (this.errors is null)
						return new ThingError[0];
					else
						return this.errors.ToArray();
				}
			}
		}

		/// <summary>
		/// Fields received during the readout.
		/// </summary>
		public Field[] ReadFields
		{
			get
			{
				lock (this.synchObject)
				{
					if (this.readFields is null)
						return new Field[0];
					else
						return this.readFields.ToArray();
				}
			}
		}

		/// <summary>
		/// If the request has been queued on the server side.
		/// </summary>
		public bool Queued { get { return this.queued; } }

		internal Task Started()
		{
			if (this.state == SensorDataReadoutState.Done || this.state == SensorDataReadoutState.Failure)
				this.Clear();

			return this.SetState(SensorDataReadoutState.Started);
		}

		/// <summary>
		/// Cancels the readout.
		/// </summary>
		public virtual Task Cancel()
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<cancel xmlns='");
			Xml.Append(SensorClient.NamespaceSensorData);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(this.Id));
			Xml.Append("'/>");

			this.sensorClient?.Client?.SendIqGet(this.RemoteJID, Xml.ToString(), this.CancelResponse, null);

			return Task.CompletedTask;
		}

		private Task CancelResponse(object Sender, IqResultEventArgs e)
		{
			if (e.Ok)
				return this.Cancelled();
			else
				return this.Fail(e.ErrorText);
		}

		internal Task Cancelled()
		{
			return this.SetState(SensorDataReadoutState.Cancelled);
		}

		internal Task Done()
		{
			return this.SetState(this.errors is null ? SensorDataReadoutState.Done : SensorDataReadoutState.Failure);
		}

	}
}
