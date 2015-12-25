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
	/// Base class for sensor data requests.
	/// </summary>
	public abstract class SensorDataRequest
	{
		private int seqNr;
		private string remoteJid;
		private ThingReference[] nodes;
		private FieldType types;
		private string[] fields;
		private DateTime from;
		private DateTime to;
		private DateTime when;
		private string serviceToken;
		private string deviceToken;
		private string userToken;
		private object tag = null;

		/// <summary>
		/// Base class for sensor data requests.
		/// </summary>
		/// <param name="SeqNr">Sequence number assigned to the request.</param>
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
		internal SensorDataRequest(int SeqNr, string RemoteJID, ThingReference[] Nodes, FieldType Types, string[] Fields, DateTime From, DateTime To, 
			DateTime When, string ServiceToken, string DeviceToken, string UserToken)
		{
			this.seqNr = SeqNr;
			this.remoteJid = RemoteJID;
			this.nodes = Nodes;
			this.types = Types;
			this.fields = Fields;
			this.from = From;
			this.to = To;
			this.when = When;
			this.serviceToken = ServiceToken;
			this.deviceToken = DeviceToken;
			this.userToken = UserToken;
		}

		/// <summary>
		/// Sequence number assigned to the request.
		/// </summary>
		public int SeqNr { get { return this.seqNr; } }

		/// <summary>
		/// JID of the other side of the conversation in the sensor data readout.
		/// </summary>
		public string RemoteJID { get { return this.remoteJid; } }

		/// <summary>
		/// Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.
		/// </summary>
		public ThingReference[] Nodes { get { return this.nodes; } }

		/// <summary>
		/// Field Types to read.
		/// </summary>
		public FieldType Types { get { return this.types; } }

		/// <summary>
		/// Fields to read.
		/// </summary>
		public string[] Fields { get { return this.fields; } }

		/// <summary>
		/// From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.
		/// </summary>
		public DateTime From { get { return this.from; } }

		/// <summary>
		/// To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.
		/// </summary>
		public DateTime To { get { return this.to; } }

		/// <summary>
		/// When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.
		/// </summary>
		public DateTime When 
		{
			get { return this.when; }
			internal set { this.when = value; } 
		}

		/// <summary>
		/// Optional service token, as defined in XEP-0324.
		/// </summary>
		public string ServiceToken { get { return this.serviceToken; } }

		/// <summary>
		/// Optional device token, as defined in XEP-0324.
		/// </summary>
		public string DeviceToken { get { return this.deviceToken; } }

		/// <summary>
		/// Optional user token, as defined in XEP-0324.
		/// </summary>
		public string UserToken { get { return this.userToken; } }

		/// <summary>
		/// Tags the request object with another object.
		/// </summary>
		public object Tag
		{
			get { return this.tag; }
			set { this.tag = value; }
		}

	}
}
