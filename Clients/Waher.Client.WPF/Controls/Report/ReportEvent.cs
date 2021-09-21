using System;
using System.Xml;
using Waher.Things.Queries;

namespace Waher.Client.WPF.Controls.Report
{
	/// <summary>
	/// Contains information about a report event.
	/// </summary>
	public class ReportEvent : ReportElement
	{
		private readonly string eventMessage;
		private readonly QueryEventType type;
		private readonly QueryEventLevel level;

		/// <summary>
		/// Contains information about a report event.
		/// </summary>
		/// <param name="Type">Event Type</param>
		/// <param name="Level">Event Level</param>
		/// <param name="EventMessage">Event Message</param>
		public ReportEvent(QueryEventType Type, QueryEventLevel Level, string EventMessage)
		{
			this.type = Type;
			this.level = Level;
			this.eventMessage = EventMessage;
		}

		/// <summary>
		/// Event type
		/// </summary>
		public QueryEventType EventType => this.type;

		/// <summary>
		/// Event level
		/// </summary>
		public QueryEventLevel EventLevel => this.level;

		/// <summary>
		/// Event Message
		/// </summary>
		public string EventMessage
		{
			get { return this.eventMessage; }
		}

		/// <summary>
		/// Exports element to XML
		/// </summary>
		/// <param name="Output">XML output</param>
		public override void ExportXml(XmlWriter Output)
		{
			Output.WriteStartElement("Event");
			Output.WriteAttributeString("type", this.type.ToString());
			Output.WriteAttributeString("level", this.level.ToString());
			Output.WriteValue(this.eventMessage);
			Output.WriteEndElement();
		}
	}
}
