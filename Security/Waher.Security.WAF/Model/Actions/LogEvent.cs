using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Events;
using Waher.Networking.HTTP.Interfaces;
using Waher.Script;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Logs an event to the event log
	/// </summary>
	public class LogEvent : WafActionWithTags
	{
		private readonly EnumAttribute<EventType> type;
		private readonly EnumAttribute<EventLevel> level;
		private readonly StringAttribute message;
		private readonly StringAttribute @object;
		private readonly StringAttribute actor;
		private readonly StringAttribute eventId;
		private readonly StringAttribute facility;
		private readonly StringAttribute module;
		private readonly StringAttribute stackTrace;

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
			this.type = new EnumAttribute<EventType>(Xml, "type");
			this.level = new EnumAttribute<EventLevel>(Xml, "level");
			this.message = new StringAttribute(Xml, "message");
			this.@object = new StringAttribute(Xml, "object");
			this.actor = new StringAttribute(Xml, "actor");
			this.eventId = new StringAttribute(Xml, "eventId");
			this.facility = new StringAttribute(Xml, "facility");
			this.module = new StringAttribute(Xml, "module");
			this.stackTrace = new StringAttribute(Xml, "stackTrace");
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

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			Variables Variables = State.Variables;
			EventType Type = await this.type.EvaluateAsync(Variables, EventType.Informational);
			EventLevel Level = await this.level.EvaluateAsync(Variables, EventLevel.Minor);
			string Message = await this.message.EvaluateAsync(Variables, string.Empty);
			string Object = await this.@object.EvaluateAsync(Variables, string.Empty);
			string Actor = await this.actor.EvaluateAsync(Variables, string.Empty);
			string EventId = await this.eventId.EvaluateAsync(Variables, string.Empty);
			string Facility = await this.facility.EvaluateAsync(Variables, string.Empty);
			string Module = await this.module.EvaluateAsync(Variables, string.Empty);
			string StackTrace = await this.stackTrace.EvaluateAsync(Variables, string.Empty);
			KeyValuePair<string, object>[] Tags = await this.EvaluateTags(State);

			Log.Event(new Event(Type, Message, Object, Actor, EventId, Level, Facility,
				Module, StackTrace, Tags));

			return null;
		}
	}
}
