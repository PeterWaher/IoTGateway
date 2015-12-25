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
	/// Manages a sensor data server request.
	/// </summary>
	public class SensorDataServerRequest : SensorDataRequest
	{
		private SensorServer sensorServer;
		private bool started = false;

		/// <summary>
		/// Manages a sensor data server request.
		/// </summary>
		/// <param name="SeqNr">Sequence number assigned to the request.</param>
		/// <param name="SensorServer">Sensor server object.</param>
		/// <param name="RemoteJID">JID of the other side of the conversation in the sensor data readout.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="When">When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.</param>
		/// <param name="ServiceToken">Optional service token, as defined in XEP-0324.</param>
		/// <param name="DeviceToken">Optional device token, as defined in XEP-0324.</param>
		/// <param name="UserToken">Optional user token, as defined in XEP-0324.</param>
		internal SensorDataServerRequest(int SeqNr, SensorServer SensorServer, string RemoteJID, ThingReference[] Nodes, FieldType Types, string[] Fields,
			DateTime From, DateTime To, DateTime When, string ServiceToken, string DeviceToken, string UserToken)
			: base(SeqNr, RemoteJID, Nodes, Types, Fields, From, To, When, ServiceToken, DeviceToken, UserToken)
		{
			this.sensorServer = SensorServer;
		}

		/// <summary>
		/// Sensor Data Server.
		/// </summary>
		public SensorServer SensorServer { get { return this.sensorServer; } }

		internal string Key
		{
			get { return this.RemoteJID + " " + this.SeqNr.ToString(); }
		}

		/// <summary>
		/// If the readout process is started or not.
		/// </summary>
		public bool Started
		{
			get { return this.started; }
			internal set { this.started = value; }
		}

		/// <summary>
		/// Report read fields to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Fields that have been read.</param>
		public void ReportFields(bool Done, params Field[] Fields)
		{
			this.ReportFields(Done, (IEnumerable<Field>)Fields);
		}

		/// <summary>
		/// Report error states to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Errors that have been detected.</param>
		public void ReportErrors(bool Done, params ThingError[] Errors)
		{
			this.ReportErrors(Done, (IEnumerable<ThingError>)Errors);
		}

		/// <summary>
		/// Report read fields to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Fields that have been read.</param>
		public void ReportFields(bool Done, IEnumerable<Field> Fields)
		{
			// TODO
		}

		/// <summary>
		/// Report error states to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Errors that have been detected.</param>
		public void ReportErrors(bool Done, IEnumerable<ThingError> Errors)
		{
			// TODO
		}
	}
}
