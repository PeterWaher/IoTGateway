using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Events;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Manages a chat sensor data readout request.
	/// </summary>
	public class InternalReadoutRequest : SensorDataServerRequest
	{
		private InternalReadoutFieldsEventHandler onFieldsReported;
		private InternalReadoutErrorsEventHandler onErrorsReported;
		private object state;

		/// <summary>
		/// Manages a sensor data server request.
		/// </summary>
		/// <param name="Actor">Actor causing the request to be made.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="OnFieldsReported">Callback method when fields are reported.</param>
		/// <param name="OnErrorsReported">Callback method when errors are reported.</param>
		/// <param name="State">State object passed on to callback methods.</param>
		public InternalReadoutRequest(string Actor, ThingReference[] Nodes, FieldType Types, string[] Fields, DateTime From, DateTime To,
			InternalReadoutFieldsEventHandler OnFieldsReported, InternalReadoutErrorsEventHandler OnErrorsReported, object State)
			: base(0, null, string.Empty, Actor, Nodes, Types, Fields, From, To, DateTime.MinValue, string.Empty, string.Empty, string.Empty)
		{
			this.onFieldsReported = OnFieldsReported;
			this.onErrorsReported = OnErrorsReported;
			this.state = State;
		}
	
		/// <summary>
		/// Report read fields to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Fields that have been read.</param>
		public override void ReportFields(bool Done, IEnumerable<Field> Fields)
		{
			try
			{
				List<Field> Filtered = new List<Field>();

				foreach (Field F in Fields)
				{
					if (this.IsIncluded(F.Name, F.Timestamp, F.Type))
						Filtered.Add(F);
				}

				this.onFieldsReported(this, new InternalReadoutFieldsEventArgs(Done, Filtered, this.state));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Report error states to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Errors that have been detected.</param>
		public override void ReportErrors(bool Done, IEnumerable<ThingError> Errors)
		{
			try
			{
				this.onErrorsReported(this, new InternalReadoutErrorsEventArgs(Done, Errors, this.state));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}
	}
}
