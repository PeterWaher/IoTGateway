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
		private string id;
		private string remoteJid;
		private string actor;
		private IThingReference[] nodes;
		private FieldType types;
		private string[] fieldsNames;
		private DateTime from;
		private DateTime to;
		private DateTime when;
		private string serviceToken;
		private string deviceToken;
		private string userToken;
		private object tag = null;
		private int nodesLeft;

		/// <summary>
		/// Base class for sensor data requests.
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
		/// <param name="ServiceToken">Optional service token.</param>
		/// <param name="DeviceToken">Optional device token.</param>
		/// <param name="UserToken">Optional user token.</param>
		internal SensorDataRequest(string Id, string RemoteJID, string Actor, IThingReference[] Nodes, FieldType Types, string[] FieldNames, 
			DateTime From, DateTime To, DateTime When, string ServiceToken, string DeviceToken, string UserToken)
		{
			this.id = Id;
			this.remoteJid = RemoteJID;
			this.actor = Actor;
			this.nodes = Nodes;
			this.nodesLeft = Nodes == null ? 0 : Nodes.Length;
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
		/// Requesst identity.
		/// </summary>
		public string Id { get { return this.id; } }

		/// <summary>
		/// JID of the other side of the conversation in the sensor data readout.
		/// </summary>
		public string RemoteJID { get { return this.remoteJid; } }

		/// <summary>
		/// Actor causing the request to be made.
		/// </summary>
		public string Actor { get { return this.actor; } }

		/// <summary>
		/// Nodes left before readout is complete.
		/// </summary>
		public int NodesLeft
		{
			get { return this.nodesLeft; }
		}

		/// <summary>
		/// Decreases the number of nodes left.
		/// </summary>
		/// <returns>If all nodes have been processed.</returns>
		protected bool DecNodesLeft()
		{
			if (this.nodesLeft > 0)
				this.nodesLeft--;

			return this.nodesLeft == 0;
		}

		/// <summary>
		/// Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.
		/// </summary>
		public IThingReference[] Nodes
		{
			get { return this.nodes; }
			internal set { this.nodes = value; }
		}

		/// <summary>
		/// Field Types to read.
		/// </summary>
		public FieldType Types
		{
			get { return this.types; }
			internal set { this.types = value; }
		}

		/// <summary>
		/// Names of fields to read.
		/// </summary>
		public string[] FieldNames
		{
			get { return this.fieldsNames; }
			internal set { this.fieldsNames = value; }
		}

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
		/// Optional service token.
		/// </summary>
		public string ServiceToken { get { return this.serviceToken; } }

		/// <summary>
		/// Optional device token.
		/// </summary>
		public string DeviceToken { get { return this.deviceToken; } }

		/// <summary>
		/// Optional user token.
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
