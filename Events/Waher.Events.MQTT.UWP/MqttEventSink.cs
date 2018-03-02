using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.MQTT;

namespace Waher.Events.MQTT
{
	/// <summary>
	/// Event sink sending events to a topic on an MQTT server. Events are sent as XML fragments.
	/// 
	/// The XML format is specified in XEP-0337:
	/// http://xmpp.org/extensions/xep-0337.html
	/// </summary>
	public class MqttEventSink : EventSink
	{
		/// <summary>
		/// urn:xmpp:eventlog
		/// </summary>
		internal const string NamespaceEventLogging = "urn:xmpp:eventlog";

		private MqttClient client;
		private string topic;
		private bool connected;
		private uint eventsLost = 0;
		private object synchObj = new object();
		private Timer timer = null;

		/// <summary>
		/// Event sink sending events to a topic on an MQTT server. Events are sent as XML fragments.
		/// 
		/// The XML format is specified in XEP-0337:
		/// http://xmpp.org/extensions/xep-0337.html
		/// </summary>
		/// <param name="ObjectID">Object ID of event sink.</param>
		/// <param name="Client">MQTT Client.</param>
		/// <param name="Topic">Destination topic.</param>
		/// <param name="MaintainConnected">If the client should be maintained connected by the event sink (true), or if the caller
		/// is responsible for maintaining the client connected (false).</param>
		public MqttEventSink(string ObjectID, MqttClient Client, string Topic, bool MaintainConnected)
			: base(ObjectID)
		{
			this.client = Client;
			this.topic = Topic;

			this.client.OnStateChanged += new StateChangedEventHandler(Client_OnStateChanged);
			this.connected = this.client.State == MqttState.Connected;

			if (MaintainConnected)
				this.timer = new Timer(this.CheckConnection, null, 60000, 60000);
		}

		private void CheckConnection(object State)
		{
			if (this.client.State == MqttState.Offline || this.client.State == MqttState.Error || this.client.State == MqttState.Authenticating)
			{
				try
				{
					this.client.Reconnect();
				}
				catch (Exception ex)
				{
					this.LogCritical(ex);
				}
			}
		}

		private void Client_OnStateChanged(object Sender, MqttState NewState)
		{
			switch (NewState)
			{
				case MqttState.Connected:
					uint i;

					lock (this.synchObj)
					{
						i = this.eventsLost;
						this.eventsLost = 0;

						this.connected = true;
					}

					if (i > 0)
					{
						Log.Notice(i.ToString() + " events lost while MQTT connection was down.",
							this.ObjectID, string.Empty, "EventsLost", new KeyValuePair<string, object>("Nr", i));
					}
					break;

				case MqttState.Offline:
					bool ImmediateReconnect;
					lock (this.synchObj)
					{
						ImmediateReconnect = this.connected;
						this.connected = false;
					}

					if (ImmediateReconnect && this.timer != null)
						this.client.Reconnect();
					break;
			}
		}

		/// <summary>
		/// MQTT Client
		/// </summary>
		public MqttClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// Destination topic of event messages.
		/// </summary>
		public string Topic
		{
			get { return this.topic; }
		}

		/// <summary>
		/// <see cref="EventSink.Queue"/>
		/// </summary>
		public override Task Queue(Event Event)
		{
			lock (this.synchObj)
			{
				if (!this.connected)
				{
					this.eventsLost++;
					return Task.CompletedTask;
				}
			}

			StringBuilder Xml = new StringBuilder();
			string s;

			Xml.Append("<log xmlns='");
			Xml.Append(NamespaceEventLogging);
			Xml.Append("' timestamp='");
			Xml.Append(XML.Encode(Event.Timestamp.ToUniversalTime()));
			Xml.Append("' type='");
			Xml.Append(Event.Type.ToString());
			Xml.Append("' level='");
			Xml.Append(Event.Level.ToString());

			if (!string.IsNullOrEmpty(s = Event.EventId))
			{
				Xml.Append("' id='");
				Xml.Append(XML.Encode(Event.EventId));
			}

			if (!string.IsNullOrEmpty(s = Event.Object))
			{
				Xml.Append("' object='");
				Xml.Append(XML.Encode(Event.Object));
			}

			if (!string.IsNullOrEmpty(s = Event.Actor))
			{
				Xml.Append("' subject='");
				Xml.Append(XML.Encode(Event.Actor));
			}

			if (!string.IsNullOrEmpty(s = Event.Facility))
			{
				Xml.Append("' facility='");
				Xml.Append(XML.Encode(Event.Facility));
			}

			if (!string.IsNullOrEmpty(s = Event.Module))
			{
				Xml.Append("' module='");
				Xml.Append(XML.Encode(Event.Module));
			}

			Xml.Append("'><message>");
			Xml.Append(XML.Encode(Event.Message));
			Xml.Append("</message>");

			foreach (KeyValuePair<string, object> Tag in Event.Tags)
			{
				Xml.Append("<tag name='");
				Xml.Append(XML.Encode(Tag.Key));

				if (Tag.Value == null)
					Xml.Append("' value=''/>");
				else
				{
					object Value = Tag.Value;

					if (Value is bool)
					{
						Xml.Append("' type='xs:boolean' value='");
						Xml.Append(CommonTypes.Encode((bool)Value));
						Xml.Append("'/>");
					}
					else if (Value is byte)
					{
						Xml.Append("' type='xs:unsignedByte' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is short)
					{
						Xml.Append("' type='xs:short' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is int)
					{
						Xml.Append("' type='xs:int' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is long)
					{
						Xml.Append("' type='xs:long' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is sbyte)
					{
						Xml.Append("' type='xs:byte' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is ushort)
					{
						Xml.Append("' type='xs:unsignedShort' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is uint)
					{
						Xml.Append("' type='xs:unsignedInt' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is ulong)
					{
						Xml.Append("' type='xs:unsignedLong' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is decimal)
					{
						Xml.Append("' type='xs:decimal' value='");
						Xml.Append(CommonTypes.Encode((decimal)Value));
						Xml.Append("'/>");
					}
					else if (Value is double)
					{
						Xml.Append("' type='xs:double' value='");
						Xml.Append(CommonTypes.Encode((double)Value));
						Xml.Append("'/>");
					}
					else if (Value is float)
					{
						Xml.Append("' type='xs:float' value='");
						Xml.Append(CommonTypes.Encode((float)Value));
						Xml.Append("'/>");
					}
					else if (Value is DateTime)
					{
						Xml.Append("' type='xs:dateTime' value='");
						Xml.Append(XML.Encode((DateTime)Value));
						Xml.Append("'/>");
					}
					else if (Value is string || Value is char)
					{
						Xml.Append("' type='xs:string' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is TimeSpan)
					{
						Xml.Append("' type='xs:time' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is Uri)
					{
						Xml.Append("' type='xs:anyURI' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else
					{
						Xml.Append("' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
				}
			}

			if (!string.IsNullOrEmpty(s = Event.StackTrace))
			{
				Xml.Append("<stackTrace>");
				Xml.Append(XML.Encode(Event.StackTrace));
				Xml.Append("</stackTrace>");
			}

			Xml.Append("</log>");

			byte[] Data = System.Text.Encoding.UTF8.GetBytes(Xml.ToString());

			this.client.PUBLISH(this.topic, MqttQualityOfService.AtLeastOnce, false, Data);

			return Task.CompletedTask;
		}
	}
}
