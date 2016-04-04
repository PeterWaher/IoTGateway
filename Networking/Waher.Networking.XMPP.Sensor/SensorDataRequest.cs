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
		private Dictionary<string, bool> fieldsByName = null;
		private int seqNr;
		private string remoteJid;
		private string actor;
		private ThingReference[] nodes;
		private FieldType types;
		private string[] fieldsNames;
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
		internal SensorDataRequest(int SeqNr, string RemoteJID, string Actor, ThingReference[] Nodes, FieldType Types, string[] FieldNames, 
			DateTime From, DateTime To, DateTime When, string ServiceToken, string DeviceToken, string UserToken)
		{
			this.seqNr = SeqNr;
			this.remoteJid = RemoteJID;
			this.actor = Actor;
			this.nodes = Nodes;
			this.types = Types;
			this.fieldsNames = FieldNames;
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
		/// Actor causing the request to be made.
		/// </summary>
		public string Actor { get { return this.actor; } }

		/// <summary>
		/// Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.
		/// </summary>
		public ThingReference[] Nodes { get { return this.nodes; } }

		/// <summary>
		/// Field Types to read.
		/// </summary>
		public FieldType Types { get { return this.types; } }

		/// <summary>
		/// Names of fields to read.
		/// </summary>
		public string[] FieldNames { get { return this.fieldsNames; } }

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

		/// <summary>
		/// Checks if a field with the given parameters is included in the readout.
		/// </summary>
		/// <param name="FieldName">Unlocalized name of field.</param>
		/// <returns>If the corresponding field is included.</returns>
		public bool IsIncluded(string FieldName)
		{
			return this.IsIncluded(FieldName, DateTime.MinValue, (FieldType)0);
		}
		
		/// <summary>
		/// Checks if a field with the given parameters is included in the readout.
		/// </summary>
		/// <param name="Timestamp">Timestamp of field.</param>
		/// <returns>If the corresponding field is included.</returns>
		public bool IsIncluded(DateTime Timestamp)
		{
			return this.IsIncluded(null, Timestamp, (FieldType)0);
		}
		
		/// <summary>
		/// Checks if a field with the given parameters is included in the readout.
		/// </summary>
		/// <param name="Type">Field Types</param>
		/// <returns>If the corresponding field is included.</returns>
		public bool IsIncluded(FieldType Type)
		{
			return this.IsIncluded(null, DateTime.MinValue, Type);
		}

		/// <summary>
		/// Checks if a field with the given parameters is included in the readout.
		/// </summary>
		/// <param name="FieldName">Unlocalized name of field.</param>
		/// <param name="Type">Field Types</param>
		/// <returns>If the corresponding field is included.</returns>
		public bool IsIncluded(string FieldName, FieldType Type)
		{
			return this.IsIncluded(FieldName, DateTime.MinValue, Type);
		}

		/// <summary>
		/// Checks if a field with the given parameters is included in the readout.
		/// </summary>
		/// <param name="FieldName">Unlocalized name of field.</param>
		/// <param name="Timestamp">Timestamp of field.</param>
		/// <param name="Type">Field Types</param>
		/// <returns>If the corresponding field is included.</returns>
		public bool IsIncluded(string FieldName, DateTime Timestamp, FieldType Type)
		{
			if (!string.IsNullOrEmpty(FieldName) && this.fieldsNames != null && this.fieldsNames.Length > 0)
			{
				lock (this.fieldsNames)
				{
					if (this.fieldsByName == null)
						this.fieldsByName = new Dictionary<string, bool>();

					foreach (string Field in this.fieldsNames)
						this.fieldsByName[Field] = true;
				}

				if (!this.fieldsByName.ContainsKey(FieldName))
					return false;
			}

			if (Timestamp != DateTime.MinValue)
			{
				if (Timestamp < this.from || Timestamp > this.to)
					return false;
			}

			if ((int)Type != 0)
			{
				if ((this.types & Type) != Type)
					return false;
			}

			return true;
		}

	}
}
