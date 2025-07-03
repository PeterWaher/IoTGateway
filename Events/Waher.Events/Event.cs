﻿using System;
using System.Collections.Generic;
using Waher.Runtime.Collections;

namespace Waher.Events
{
	/// <summary>
	/// Class representing an event.
	/// </summary>
	public class Event
	{
		private ChunkedList<IEventSink> toAvoid = null;
		private readonly DateTime timestamp;
		private readonly EventType type;
		private readonly EventLevel level;
		private readonly string message;
		private readonly string obj;
		private readonly string actor;
		private readonly string eventId;
		private readonly string module;
		private readonly string facility;
		private readonly string stackTrace;
		private readonly KeyValuePair<string, object>[] tags;

		/// <summary>
		/// Class representing an event.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public Event(DateTime Timestamp, EventType Type, string Message, string Object, string Actor, string EventId, EventLevel Level, string Facility,
			string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			this.timestamp = Timestamp;
			this.type = Type;
			this.message = Message;
			this.obj = Object;
			this.actor = Actor;
			this.eventId = EventId;
			this.level = Level;
			this.facility = Facility;
			this.module = Module;
			this.stackTrace = StackTrace;
			this.tags = Tags;
		}

		/// <summary>
		/// Class representing an event.
		/// </summary>
		/// <param name="Type">Event Type.</param>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public Event(EventType Type, string Message, string Object, string Actor, string EventId, EventLevel Level, string Facility, string Module,
			string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			this.timestamp = DateTime.UtcNow;
			this.type = Type;
			this.message = Message;
			this.obj = Object;
			this.actor = Actor;
			this.eventId = EventId;
			this.level = Level;
			this.facility = Facility;
			this.module = Module;
			this.stackTrace = StackTrace;
			this.tags = Tags;
		}

		/// <summary>
		/// Class representing an event.
		/// </summary>
		/// <param name="Type">Event Type.</param>
		/// <param name="Exception">Exception object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public Event(EventType Type, Exception Exception, string Object, string Actor, string EventId, EventLevel Level, string Facility, string Module,
			params KeyValuePair<string, object>[] Tags)
		{
			string s;

			this.timestamp = DateTime.UtcNow;
			this.type = Exception is IEventType Tp && Tp.Type.HasValue ? Tp.Type.Value : Type;
			this.message = Exception.Message;
			this.obj = Exception is IEventObject Obj && !string.IsNullOrEmpty(s = Obj.Object) ? s : Object;
			this.actor = Exception is IEventActor Act && !string.IsNullOrEmpty(s = Act.Actor) ? s : Actor;
			this.eventId = Exception is IEventId EvId && !string.IsNullOrEmpty(s = EvId.EventId) ? s : string.IsNullOrEmpty(EventId) ? Exception.GetType().Name : EventId;
			this.level = Exception is IEventLevel Lvl && Lvl.Level.HasValue ? Lvl.Level.Value : Level;
			this.facility = Exception is IEventFacility EvFa && !string.IsNullOrEmpty(s = EvFa.Facility) ? s : Facility;
			this.module = Exception is IEventModule Mod && !string.IsNullOrEmpty(s = Mod.Module) ? s : Module;
			this.stackTrace = Log.CleanStackTrace(Exception.StackTrace);
			this.tags = (Tags is null || Tags.Length == 0) && Exception is IEventTags Tgs ? Tgs.Tags.Join(Tags) : Tags;
		}

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp => this.timestamp;

		/// <summary>
		/// Type of event.
		/// </summary>
		public EventType Type => this.type;

		/// <summary>
		/// Event Level.
		/// </summary>
		public EventLevel Level => this.level;

		/// <summary>
		/// Free-text event message.
		/// </summary>
		public string Message => this.message;

		/// <summary>
		/// Object related to the event.
		/// </summary>
		public string Object => this.obj;

		/// <summary>
		/// Actor responsible for the action causing the event.
		/// </summary>
		public string Actor => this.actor;

		/// <summary>
		/// Computer-readable Event ID identifying type of even.
		/// </summary>
		public string EventId => this.eventId;

		/// <summary>
		/// Facility can be either a facility in the network sense or in the system sense.
		/// </summary>
		public string Facility => this.facility;

		/// <summary>
		/// Module where the event is reported.
		/// </summary>
		public string Module => this.module;

		/// <summary>
		/// Stack Trace of event.
		/// </summary>
		public string StackTrace => this.stackTrace;

		/// <summary>
		/// Variable set of tags providing event-specific information.
		/// </summary>
		public KeyValuePair<string, object>[] Tags => this.tags;

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.message;
		}

		/// <summary>
		/// If the event sink <paramref name="EventSink"/> should be avoided when logging the event.
		/// </summary>
		/// <param name="EventSink">Event sink</param>
		/// <returns>If the event sink should be avoided.</returns>
		public bool ShoudAvoid(IEventSink EventSink)
		{
			return (this.toAvoid?.Contains(EventSink) ?? false);
		}

		/// <summary>
		/// If the event sink <paramref name="EventSink"/> should be avoided when logging the event.
		/// </summary>
		/// <param name="EventSink">Event sink</param>
		/// <returns>If the event sink should be avoided.</returns>
		public void Avoid(IEventSink EventSink)
		{
			if (this.toAvoid is null)
				this.toAvoid = new ChunkedList<IEventSink>();

			if (!this.toAvoid.Contains(EventSink))
				this.toAvoid.Add(EventSink);
		}

		/// <summary>
		/// List of event sinks to avoid. Can be null.
		/// </summary>
		internal ChunkedList<IEventSink> ToAvoid => this.toAvoid;
	}
}
