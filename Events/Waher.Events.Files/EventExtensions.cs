using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Events.Files
{
	/// <summary>
	/// Event extensions.
	/// </summary>
	public static class EventExtensions
	{
		/// <summary>
		/// http://waher.se/Schema/EventOutput.xsd
		/// </summary>
		public const string LogNamespace = "http://waher.se/Schema/EventOutput.xsd";

		/// <summary>
		/// Exports the event object to XML.
		/// </summary>
		/// <param name="Event">Event object.</param>
		/// <returns>XML representation of event object.</returns>
		public static string ToXML(this Event Event)
		{
			StringBuilder sb = new StringBuilder();
			Event.ToXML(sb);
			return sb.ToString();
		}

		/// <summary>
		/// Exports the event object to XML.
		/// </summary>
		/// <param name="Event">Event object.</param>
		/// <param name="Xml">XML output.</param>
		public static void ToXML(this Event Event, StringBuilder Xml)
		{
			string ElementName = Event.Type.ToString();

			Xml.Append('<');
			Xml.Append(ElementName);
			Xml.Append(" xmlns='");
			Xml.Append(LogNamespace);
			Xml.Append("' timestamp='");
			Xml.Append(XML.Encode(Event.Timestamp));
			Xml.Append("' level='");
			Xml.Append(Event.Level.ToString());

			if (!string.IsNullOrEmpty(Event.EventId))
			{
				Xml.Append("' id='");
				Xml.Append(Event.EventId);
			}

			if (!string.IsNullOrEmpty(Event.Object))
			{
				Xml.Append("' object='");
				Xml.Append(Event.Object);
			}

			if (!string.IsNullOrEmpty(Event.Actor))
			{
				Xml.Append("' actor='");
				Xml.Append(Event.Actor);
			}

			if (!string.IsNullOrEmpty(Event.Module))
			{
				Xml.Append("' module='");
				Xml.Append(Event.Module);
			}

			if (!string.IsNullOrEmpty(Event.Facility))
			{
				Xml.Append("' facility='");
				Xml.Append(Event.Facility);
			}

			Xml.Append("'><Message>");

			foreach (string Row in GetRows(Event.Message))
			{
				Xml.Append("<Row>");
				Xml.Append(XML.Encode(Row));
				Xml.Append("</Row>");
			}

			Xml.Append("</Message>");

			if (!(Event.Tags is null) && Event.Tags.Length > 0)
			{
				foreach (KeyValuePair<string, object> Tag in Event.Tags)
				{
					Xml.Append("<Tag key='");
					Xml.Append(XML.Encode(Tag.Key));

					if (!(Tag.Value is null))
					{
						Xml.Append("' value='");
						Xml.Append(XML.Encode(Tag.Value.ToString()));
					}

					Xml.Append("'/>");
				}
			}

			if (Event.Type >= EventType.Critical && !string.IsNullOrEmpty(Event.StackTrace))
			{
				Xml.Append("<StackTrace>");

				foreach (string Row in GetRows(Event.StackTrace))
				{
					Xml.Append("<Row>");
					Xml.Append(XML.Encode(Row));
					Xml.Append("</Row>");
				}

				Xml.Append("</StackTrace>");
			}

			Xml.Append("</");
			Xml.Append(ElementName);
			Xml.Append('>');
		}

		internal static string[] GetRows(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		}

		/// <summary>
		/// Tries to parse an event object (or collection of event objects) from an XML document.
		/// </summary>
		/// <param name="Xml">XML Document.</param>
		/// <param name="Parsed">Parsed events.</param>
		/// <returns>If able to parse event or events.</returns>
		public static bool TryParse(string Xml, out Event[] Parsed)
		{
			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Xml);
			return TryParse(Xml, out Parsed);
		}

		/// <summary>
		/// Tries to parse an event object (or collection of event objects) from an XML document.
		/// </summary>
		/// <param name="Xml">XML Document.</param>
		/// <param name="Parsed">Parsed events.</param>
		/// <returns>If able to parse event or events.</returns>
		public static bool TryParse(XmlDocument Xml, out Event[] Parsed)
		{
			Parsed = null;

			if (Xml.DocumentElement is null || Xml.DocumentElement.NamespaceURI != LogNamespace)
				return false;

			if (Xml.DocumentElement.LocalName == "EventOutput")
			{
				List<Event> List = new List<Event>();

				foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
				{
					if (!(N is XmlElement E))
						continue;

					if (!TryParse(E, out Event ParsedElement))
						return false;

					List.Add(ParsedElement);
				}

				Parsed = List.ToArray();
				return true;
			}
			else if (TryParse(Xml.DocumentElement, out Event ParsedElement))
			{
				Parsed = new Event[] { ParsedElement };
				return true;
			}
			else
				return false;
		}


		/// <summary>
		/// Tries to parse an event object from an XML element.
		/// </summary>
		/// <param name="Xml">XML element.</param>
		/// <param name="Parsed">Parsed event.</param>
		/// <returns>If able to parse event.</returns>
		public static bool TryParse(XmlElement Xml, out Event Parsed)
		{
			if (Xml is null || Xml.NamespaceURI != LogNamespace)
			{
				Parsed = null;
				return false;
			}

			DateTime Timestamp = XML.Attribute(Xml, "timestamp", DateTime.Now);
			EventLevel Level = XML.Attribute(Xml, "level", EventLevel.Minor);
			string EventId = XML.Attribute(Xml, "id");
			string Object = XML.Attribute(Xml, "object");
			string Actor = XML.Attribute(Xml, "actor");
			string Module = XML.Attribute(Xml, "module");
			string Facility = XML.Attribute(Xml, "facility");
			string StackTrace = null;
			string Message = null;
			List<KeyValuePair<string, object>> Tags = new List<KeyValuePair<string, object>>();

			foreach (XmlNode N2 in Xml.ChildNodes)
			{
				if (!(N2 is XmlElement E2))
					continue;

				switch (E2.LocalName)
				{
					case "Message":
						Message = ReadRows(E2);
						break;

					case "StackTrace":
						StackTrace = ReadRows(E2);
						break;

					case "Tag":
						string Key = XML.Attribute(E2, "key");
						string Value = XML.Attribute(E2, "value");

						if (bool.TryParse(Value, out bool b))
							Tags.Add(new KeyValuePair<string, object>(Key, b));
						else if (int.TryParse(Value, out int i))
							Tags.Add(new KeyValuePair<string, object>(Key, i));
						else if (long.TryParse(Value, out long l))
							Tags.Add(new KeyValuePair<string, object>(Key, l));
						else if (double.TryParse(Value, out double d))
							Tags.Add(new KeyValuePair<string, object>(Key, d));
						else
							Tags.Add(new KeyValuePair<string, object>(Key, Value));
						break;
				}
			}

			if (!Enum.TryParse(Xml.LocalName, out EventType EventType))
			{
				Parsed = null;
				return false;
			}

			Parsed = new Event(Timestamp, EventType, Message ?? string.Empty, Object, Actor,
				EventId, Level, Facility, Module, StackTrace, Tags.ToArray());

			return true;
		}

		private static string ReadRows(XmlElement E)
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (XmlNode N in E.ChildNodes)
			{
				if (!(N is XmlElement E2))
					continue;

				if (E2.LocalName == "Row")
				{
					if (First)
						First = false;
					else
						sb.AppendLine();

					sb.Append(E2.InnerText);
				}
			}

			return sb.ToString();
		}
	}
}
