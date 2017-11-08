using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Manages a sensor data client request.
	/// </summary>
	public class CustomSensorDataClientRequest : SensorDataClientRequest 
	{
		/// <summary>
		/// Manages a sensor data client request.
		/// </summary>
		/// <param name="Id">Request identity.</param>
		/// <param name="RemoteJID">JID of the other side of the conversation in the sensor data readout.</param>
		/// <param name="Actor">Actor causing the request to be made.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="FieldNames">Names of fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="When">When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.</param>
		/// <param name="ServiceToken">Optional service token, as defined in XEP-0324.</param>
		/// <param name="DeviceToken">Optional device token, as defined in XEP-0324.</param>
		/// <param name="UserToken">Optional user token, as defined in XEP-0324.</param>
		public CustomSensorDataClientRequest(string Id, string RemoteJID, string Actor, ThingReference[] Nodes, FieldType Types,
			string[] FieldNames, DateTime From, DateTime To, DateTime When, string ServiceToken, string DeviceToken, string UserToken)
			: base(Id, null, RemoteJID, Actor, Nodes, Types, FieldNames, From, To, When, ServiceToken, DeviceToken, UserToken)
		{
		}

		/// <summary>
		/// Readout failed.
		/// </summary>
		/// <param name="Reason">Reason for failure.</param>
		public new void Fail(string Reason)
		{
			base.Fail(Reason);
		}

		/// <summary>
		/// Errors logged.
		/// </summary>
		/// <param name="Errors">Errors</param>
		public new void LogErrors(IEnumerable<ThingError> Errors)
		{
			base.LogErrors(Errors);
		}

		/// <summary>
		/// Fields logged.
		/// </summary>
		/// <param name="Fields"></param>
		public new void LogFields(IEnumerable<Field> Fields)
		{
			base.LogFields(Fields);
		}

		/// <summary>
		/// Readout accepted.
		/// </summary>
		/// <param name="Queued">If it has been queued.</param>
		public new void Accept(bool Queued)
		{
			base.Accept(Queued);
		}

		/// <summary>
		/// Readout started.
		/// </summary>
		public new void Started()
		{
			base.Started();
		}

		/// <summary>
		/// Cancels the readout.
		/// </summary>
		public override void Cancel()
		{
			try
			{
				this.OnCancel?.Invoke(this, new EventArgs());
				this.State = SensorDataReadoutState.Cancelled;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when readout is cancelled.
		/// </summary>
		public EventHandler OnCancel = null;

		/// <summary>
		/// Readout complete.
		/// </summary>
		public new void Done()
		{
			base.Done();
		}

	}
}
