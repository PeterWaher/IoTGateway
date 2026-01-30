using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Logs an event to the event log
	/// </summary>
	public class LogEvent : WafActionWithTags
	{
		private readonly string message;
		private readonly string type;
		private readonly string level;
		private readonly string @object;
		private readonly string actor;
		private readonly string eventId;
		private readonly string facility;
		private readonly string module;
		private readonly string stackTrace;

		/// <summary>
		/// Logs an event to the event log
		/// </summary>
		public LogEvent()
			: base()
		{
		}

		/// <summary>
		/// Logs an event to the event log
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public LogEvent(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.message = XML.Attribute(Xml, "message");
			this.type = XML.Attribute(Xml, "type");
			this.level = XML.Attribute(Xml, "level");
			this.@object = XML.Attribute(Xml, "object");
			this.actor = XML.Attribute(Xml, "actor");
			this.eventId = XML.Attribute(Xml, "eventId");
			this.facility = XML.Attribute(Xml, "facility");
			this.module = XML.Attribute(Xml, "module");
			this.stackTrace = XML.Attribute(Xml, "stackTrace");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(LogEvent);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new LogEvent(Xml, Parent, Document);
	}
}
