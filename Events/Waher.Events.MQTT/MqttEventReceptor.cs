using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Events;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.MQTT;

namespace Waher.Events.MQTT
{
	/// <summary>
	/// This class handles incoming events from the MQTT network. The default behaviour is to log incoming events to <see cref="Log"/>. 
	/// This behaviour can be overridden by provding an event handler for the <see cref="OnEvent"/> event.
	/// 
	/// The format of XML fragments understood is specified in XEP-0337:
	/// http://xmpp.org/extensions/xep-0337.html
	/// </summary>
	public class MqttEventReceptor : IDisposable
	{
		private MqttClient client;

		/// <summary>
		/// This class handles incoming events from the MQTT network. The default behaviour is to log incoming events to <see cref="Log"/>. 
		/// This behaviour can be overridden by provding an event handler for the <see cref="OnEvent"/> event.
		/// 
		/// The format of XML fragments understood is specified in XEP-0337:
		/// http://xmpp.org/extensions/xep-0337.html
		/// </summary>
		/// <param name="Client">Client on which to receive events from.</param>
		public MqttEventReceptor(MqttClient Client)
		{
			this.client = Client;
			this.client.OnContentReceived += Client_OnContentReceived;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.OnContentReceived -= Client_OnContentReceived;
		}

		private void Client_OnContentReceived(object Sender, MqttContent Content)
		{
			string Xml = System.Text.Encoding.UTF8.GetString(Content.Data);
			XmlDocument Doc = new XmlDocument();
			try
			{
				Doc.LoadXml(Xml);
			}
			catch (Exception)
			{
				return;
			}

			XmlElement E = Doc.DocumentElement;
			if (E.LocalName != "log" || E.NamespaceURI != MqttEventSink.NamespaceEventLogging)
				return;

			XmlElement E2;
			List<KeyValuePair<string, object>> Tags = new List<KeyValuePair<string, object>>();
			DateTime Timestamp = XML.Attribute(E, "timestamp", DateTime.MinValue);
			string EventId = XML.Attribute(E, "id");
			EventType Type = (EventType)XML.Attribute(E, "type", EventType.Informational);
			EventLevel Level = (EventLevel)XML.Attribute(E, "level", EventLevel.Minor);
			string Object = XML.Attribute(E, "object");
			string Actor = XML.Attribute(E, "subject");
			string Facility = XML.Attribute(E, "facility");
			string Module = XML.Attribute(E, "module");
			string Message = string.Empty;
			string StackTrace = string.Empty;

			foreach (XmlNode N in E.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "message":
						Message = N.InnerText;
						break;

					case "tag":
						E2 = (XmlElement)N;
						string TagName = XML.Attribute(E2, "name");
						string TagValue = XML.Attribute(E2, "value");
						string TagType = XML.Attribute(E2, "type");
						object TagValueParsed = TagValue;

						switch (TagType)
						{
							case "xs:anyURI":
								Uri URI;
								if (Uri.TryCreate(TagValue, UriKind.Absolute, out URI))
									TagValueParsed = URI;
								break;

							case "xs:boolean":
								bool b;
								if (CommonTypes.TryParse(TagValue, out b))
									TagValueParsed = b;
								break;

							case "xs:unsignedByte":
								byte ui8;
								if (byte.TryParse(TagValue, out ui8))
									TagValueParsed = ui8;
								break;

							case "xs:short":
								short i16;
								if (short.TryParse(TagValue, out i16))
									TagValueParsed = i16;
								break;

							case "xs:int":
								int i32;
								if (int.TryParse(TagValue, out i32))
									TagValueParsed = i32;
								break;

							case "xs:long":
								long i64;
								if (long.TryParse(TagValue, out i64))
									TagValueParsed = i64;
								break;

							case "xs:byte":
								sbyte i8;
								if (sbyte.TryParse(TagValue, out i8))
									TagValueParsed = i8;
								break;

							case "xs:unsignedShort":
								ushort ui16;
								if (ushort.TryParse(TagValue, out ui16))
									TagValueParsed = ui16;
								break;

							case "xs:unsignedInt":
								uint ui32;
								if (uint.TryParse(TagValue, out ui32))
									TagValueParsed = ui32;
								break;

							case "xs:unsignedLong":
								ulong ui64;
								if (ulong.TryParse(TagValue, out ui64))
									TagValueParsed = ui64;
								break;

							case "xs:decimal":
								decimal d;
								if (CommonTypes.TryParse(TagValue, out d))
									TagValueParsed = d;
								break;

							case "xs:double":
								double d2;
								if (CommonTypes.TryParse(TagValue, out d2))
									TagValueParsed = d2;
								break;

							case "xs:float":
								float f;
								if (CommonTypes.TryParse(TagValue, out f))
									TagValueParsed = f;
								break;

							case "xs:time":
								TimeSpan TS;
								if (TimeSpan.TryParse(TagValue, out TS))
									TagValueParsed = TS;
								break;

							case "xs:date":
							case "xs:dateTime":
								DateTime DT;
								if (XML.TryParse(TagValue, out DT))
									TagValueParsed = DT;
								break;

							case "xs:string":
							case "xs:language":
							default:
								break;
						}

						Tags.Add(new KeyValuePair<string, object>(TagName, TagValueParsed));
						break;

					case "stackTrace":
						StackTrace = N.InnerText;
						break;
				}
			}

			if (string.IsNullOrEmpty(Facility))
				Facility = Content.Topic;

			Event Event = new Event(Timestamp, Type, Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags.ToArray());
			EventEventHandler h = this.OnEvent;
			
			if (h == null)
				Log.Event(Event);
			else
			{
				try
				{
					h(this, new EventEventArgs(Content, Event));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised whenever an event has been received. If no event handler is defined, the default action is to log the event
		/// to <see cref="Log"/>.
		/// </summary>
		public event EventEventHandler OnEvent = null;

	}
}
