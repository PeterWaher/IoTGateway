using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP;

namespace Waher.Events.XMPP
{
	/// <summary>
	/// Event sink sending events to a destination over the XMPP network. 
	/// 
	/// The format is specified in XEP-0337:
	/// http://xmpp.org/extensions/xep-0337.html
	/// </summary>
	public class XmppEventSink : EventSink
	{
		/// <summary>
		/// urn:xmpp:eventlog
		/// </summary>
		public const string NamespaceEventLogging = "urn:xmpp:eventlog";

		private readonly XmppClient client;
		private readonly string destination;
		private bool connected;
		private uint eventsLost = 0;
		private readonly object synchObj = new object();
		private readonly Timer timer = null;

		/// <summary>
		/// Event sink sending events to a destination over the XMPP network. 
		/// 
		/// The format is specified in XEP-0337:
		/// http://xmpp.org/extensions/xep-0337.html
		/// </summary>
		/// <param name="ObjectID">Object ID of event sink.</param>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="Destination">Destination.</param>
		/// <param name="MaintainConnected">If the client should be maintained connected by the event sink (true), or if the caller
		/// is responsible for maintaining the client connected (false).</param>
		public XmppEventSink(string ObjectID, XmppClient Client, string Destination, bool MaintainConnected)
			: base(ObjectID)
		{
			this.client = Client;
			this.destination = Destination;

			this.client.OnStateChanged += new StateChangedEventHandler(Client_OnStateChanged);
			this.connected = this.client.State == XmppState.Connected;

			if (MaintainConnected)
				this.timer = new Timer(this.CheckConnection, null, 60000, 60000);
		}

		private void CheckConnection(object State)
		{
			if (this.client.State == XmppState.Offline || this.client.State == XmppState.Error || this.client.State == XmppState.Authenticating)
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

		private void Client_OnStateChanged(object Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					uint i;

					lock (this.synchObj)
					{
						i = this.eventsLost;
						this.eventsLost = 0;

						this.connected = true;
					}

					if (i > 0)
					{
						Log.Notice(i.ToString() + " events lost while XMPP connection was down.",
							this.ObjectID, string.Empty, "EventsLost", new KeyValuePair<string, object>("Nr", i));
					}
					break;

				case XmppState.Offline:
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
		/// XMPP Client
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// Destination of event messages.
		/// </summary>
		public string Destination
		{
			get { return this.destination; }
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

					if (Value is bool b)
					{
						Xml.Append("' type='xs:boolean' value='");
						Xml.Append(CommonTypes.Encode(b));
						Xml.Append("'/>");
					}
					else if (Value is byte ui8)
					{
						Xml.Append("' type='xs:unsignedByte' value='");
						Xml.Append(ui8.ToString());
						Xml.Append("'/>");
					}
					else if (Value is short i16)
					{
						Xml.Append("' type='xs:short' value='");
						Xml.Append(i16.ToString());
						Xml.Append("'/>");
					}
					else if (Value is int i32)
					{
						Xml.Append("' type='xs:int' value='");
						Xml.Append(i32.ToString());
						Xml.Append("'/>");
					}
					else if (Value is long i64)
					{
						Xml.Append("' type='xs:long' value='");
						Xml.Append(i64.ToString());
						Xml.Append("'/>");
					}
					else if (Value is sbyte i8)
					{
						Xml.Append("' type='xs:byte' value='");
						Xml.Append(i8.ToString());
						Xml.Append("'/>");
					}
					else if (Value is ushort ui16)
					{
						Xml.Append("' type='xs:unsignedShort' value='");
						Xml.Append(ui16.ToString());
						Xml.Append("'/>");
					}
					else if (Value is uint ui32)
					{
						Xml.Append("' type='xs:unsignedInt' value='");
						Xml.Append(ui32.ToString());
						Xml.Append("'/>");
					}
					else if (Value is ulong ui64)
					{
						Xml.Append("' type='xs:unsignedLong' value='");
						Xml.Append(ui64.ToString());
						Xml.Append("'/>");
					}
					else if (Value is decimal dc)
					{
						Xml.Append("' type='xs:decimal' value='");
						Xml.Append(CommonTypes.Encode(dc));
						Xml.Append("'/>");
					}
					else if (Value is double db)
					{
						Xml.Append("' type='xs:double' value='");
						Xml.Append(CommonTypes.Encode(db));
						Xml.Append("'/>");
					}
					else if (Value is float fl)
					{
						Xml.Append("' type='xs:float' value='");
						Xml.Append(CommonTypes.Encode(fl));
						Xml.Append("'/>");
					}
					else if (Value is DateTime dt)
					{
						Xml.Append("' type='xs:dateTime' value='");
						Xml.Append(XML.Encode(dt));
						Xml.Append("'/>");
					}
					else if (Value is string || Value is char)
					{
						Xml.Append("' type='xs:string' value='");
						Xml.Append(Value.ToString());
						Xml.Append("'/>");
					}
					else if (Value is TimeSpan ts)
					{
						Xml.Append("' type='xs:time' value='");
						Xml.Append(ts.ToString());
						Xml.Append("'/>");
					}
					else if (Value is Uri u)
					{
						Xml.Append("' type='xs:anyURI' value='");
						Xml.Append(u.ToString());
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

			this.client.SendMessage(MessageType.Normal, this.destination, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

			return Task.CompletedTask;
		}
	}
}
