using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Events;

namespace Waher.Events.XMPP
{
	/// <summary>
	/// This class handles incoming events from the XMPP network. The default behaviour is to log incoming events to <see cref="Log"/>. 
	/// This behaviour can be overridden by provding an event handler for the <see cref="OnEvent"/> event.
	/// 
	/// The format is specified in XEP-0337:
	/// http://xmpp.org/extensions/xep-0337.html
	/// </summary>
	public class XmppEventReceptor : IDisposable
	{
		private readonly XmppClient client;

		/// <summary>
		/// This class handles incoming events from the XMPP network. The default behaviour is to log incoming events to <see cref="Log"/>. 
		/// This behaviour can be overridden by provding an event handler for the <see cref="OnEvent"/> event.
		/// 
		/// The format is specified in XEP-0337:
		/// http://xmpp.org/extensions/xep-0337.html
		/// </summary>
		/// <param name="Client">Client on which to receive events from.</param>
		public XmppEventReceptor(XmppClient Client)
		{
			this.client = Client;

			this.client.RegisterMessageHandler("log", XmppEventSink.NamespaceEventLogging, this.EventMessageHandler, true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterMessageHandler("log", XmppEventSink.NamespaceEventLogging, this.EventMessageHandler, true);
		}

		private async Task EventMessageHandler(object Sender, MessageEventArgs e)
		{
			XmlElement E = e.Content;
			XmlElement E2;
			List<KeyValuePair<string, object>> Tags = new List<KeyValuePair<string, object>>();
			DateTime Timestamp = XML.Attribute(E, "timestamp", DateTime.MinValue);
			string EventId = XML.Attribute(E, "id");
			EventType Type = XML.Attribute(E, "type", EventType.Informational);
			EventLevel Level = XML.Attribute(E, "level", EventLevel.Minor);
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

						if (!TryParse(TagValue, TagType, out object TagValueParsed))
							TagValueParsed = TagValue;

						Tags.Add(new KeyValuePair<string, object>(TagName, TagValueParsed));
						break;

					case "stackTrace":
						StackTrace = N.InnerText;
						break;
				}
			}

			if (string.IsNullOrEmpty(Facility))
				Facility = e.FromBareJID;

			Event Event = new Event(Timestamp, Type, Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags.ToArray());
			EventHandlerAsync<EventEventArgs> h = this.OnEvent;

			if (h is null)
				Log.Event(Event);
			else
				await h.Raise(this, new EventEventArgs(e, Event));
		}

		/// <summary>
		/// Event raised whenever an event has been received. If no event handler is defined, the default action is to log the event
		/// to <see cref="Log"/>.
		/// </summary>
		public event EventHandlerAsync<EventEventArgs> OnEvent = null;

		/// <summary>
		/// Tries to parse a simple value.
		/// </summary>
		/// <param name="Value">String-representation of value.</param>
		/// <param name="Type">Type</param>
		/// <param name="ParsedValue">Parsed value.</param>
		/// <returns>If able to parse value.</returns>
		public static bool TryParse(string Value, string Type, out object ParsedValue)
		{
			switch (Type)
			{
				case "xs:anyURI":
					Uri URI;
					if (Uri.TryCreate(Value, UriKind.Absolute, out URI))
					{
						ParsedValue = URI;
						return true;
					}
					break;

				case "xs:boolean":
					bool b;
					if (CommonTypes.TryParse(Value, out b))
					{
						ParsedValue = b;
						return true;
					}
					break;

				case "xs:unsignedByte":
					byte ui8;
					if (byte.TryParse(Value, out ui8))
					{
						ParsedValue = ui8;
						return true;
					}
					break;

				case "xs:short":
					short i16;
					if (short.TryParse(Value, out i16))
					{
						ParsedValue = i16;
						return true;
					}
					break;

				case "xs:int":
					int i32;
					if (int.TryParse(Value, out i32))
					{
						ParsedValue = i32;
						return true;
					}
					break;

				case "xs:long":
					long i64;
					if (long.TryParse(Value, out i64))
					{
						ParsedValue = i64;
						return true;
					}
					break;

				case "xs:byte":
					sbyte i8;
					if (sbyte.TryParse(Value, out i8))
					{
						ParsedValue = i8;
						return true;
					}
					break;

				case "xs:unsignedShort":
					ushort ui16;
					if (ushort.TryParse(Value, out ui16))
					{
						ParsedValue = ui16;
						return true;
					}
					break;

				case "xs:unsignedInt":
					uint ui32;
					if (uint.TryParse(Value, out ui32))
					{
						ParsedValue = ui32;
						return true;
					}
					break;

				case "xs:unsignedLong":
					ulong ui64;
					if (ulong.TryParse(Value, out ui64))
					{
						ParsedValue = ui64;
						return true;
					}
					break;

				case "xs:decimal":
					decimal d;
					if (CommonTypes.TryParse(Value, out d))
					{
						ParsedValue = d;
						return true;
					}
					break;

				case "xs:double":
					double d2;
					if (CommonTypes.TryParse(Value, out d2))
					{
						ParsedValue = d2;
						return true;
					}
					break;

				case "xs:float":
					float f;
					if (CommonTypes.TryParse(Value, out f))
					{
						ParsedValue = f;
						return true;
					}
					break;

				case "xs:time":
					TimeSpan TS;
					if (TimeSpan.TryParse(Value, out TS))
					{
						ParsedValue = TS;
						return true;
					}
					break;

				case "xs:date":
				case "xs:dateTime":
					DateTime DT;
					if (XML.TryParse(Value, out DT))
					{
						ParsedValue = DT;
						return true;
					}
					break;

				case "xs:string":
				case "xs:language":
				default:
					ParsedValue = Value;
					return true;
			}

			ParsedValue = Value;
			return false;
		}

		/// <summary>
		/// Tries to get the type of a parsed value.
		/// </summary>
		/// <param name="Value">Parsed value.</param>
		/// <param name="Type">Type</param>
		/// <returns>If able to get the type.</returns>
		public static bool TryGetType(object Value, out string Type)
		{
			if (Value is null)
				Type = null;
			else if (Value is bool)
				Type = "xs:boolean";
			else if (Value is byte)
				Type = "xs:unsignedByte";
			else if (Value is short)
				Type = "xs:short";
			else if (Value is int)
				Type = "xs:int";
			else if (Value is long)
				Type = "xs:long";
			else if (Value is sbyte)
				Type = "xs:byte";
			else if (Value is ushort)
				Type = "xs:unsignedShort";
			else if (Value is uint)
				Type = "xs:unsignedInt";
			else if (Value is ulong)
				Type = "xs:unsignedLong";
			else if (Value is decimal)
				Type = "xs:decimal";
			else if (Value is double)
				Type = "xs:double";
			else if (Value is float)
				Type = "xs:float";
			else if (Value is TimeSpan)
				Type = "xs:time";
			else if (Value is DateTime)
				Type = "xs:dateTime";
			else if (Value is string)
				Type = "xs:string";
			else if (Value is Uri)
				Type = "xs:anyURI";
			else
				Type = null;

			return !(Type is null);
		}

	}
}
