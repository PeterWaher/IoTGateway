using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Events
{
	/// <summary>
	/// Base class for objects that can log events.
	/// </summary>
	public abstract class LogObject : ILogObject
	{
		private string objectId;

		/// <summary>
		/// Base class for objects that can log events.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		public LogObject(string ObjectID)
		{
			this.objectId = ObjectID;
		}

		/// <summary>
		/// Object ID, used when logging events.
		/// </summary>
		public virtual string ObjectID
		{
			get { return this.objectId; }
		}

		#region Debug

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogDebug(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Log.Debug(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, StackTrace, Tags);
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogDebug(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Debug(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogDebug(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Debug(Message, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogDebug(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Debug(Message, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogDebug(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Debug(Message, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogDebug(string Message, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Debug(Message, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogDebug(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Log.Debug(Message, this.ObjectID, Tags);
		}

		#endregion

		#region Informational

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogInformational(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Log.Informational(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, StackTrace, Tags);
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogInformational(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Informational(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogInformational(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Informational(Message, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogInformational(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Informational(Message, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogInformational(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Informational(Message, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogInformational(string Message, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Informational(Message, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogInformational(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Log.Informational(Message, this.ObjectID, Tags);
		}

		#endregion

		#region Notice

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogNotice(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Log.Notice(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, StackTrace, Tags);
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogNotice(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Notice(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogNotice(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Notice(Message, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogNotice(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Notice(Message, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogNotice(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Notice(Message, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogNotice(string Message, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Notice(Message, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogNotice(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Log.Notice(Message, this.ObjectID);
		}

		#endregion

		#region Warning

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogWarning(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Log.Warning(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, StackTrace, Tags);
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogWarning(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Warning(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogWarning(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Warning(Message, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogWarning(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Warning(Message, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogWarning(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Warning(Message, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogWarning(string Message, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Warning(Message, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogWarning(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Log.Warning(Message, this.ObjectID, Tags);
		}

		#endregion

		#region Error

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, StackTrace, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Message, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Message, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Message, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(string Message, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Message, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Message, this.ObjectID, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Exception, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Exception, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(Exception Exception, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Exception, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(Exception Exception, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Exception, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(Exception Exception, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Exception, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogError(Exception Exception, params KeyValuePair<string, object>[] Tags)
		{
			Log.Error(Exception, this.ObjectID, Tags);
		}

		#endregion

		#region Critical

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, StackTrace, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Message, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Message, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Message, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(string Message, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Message, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Message, this.ObjectID, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Exception, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Exception, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(Exception Exception, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Exception, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(Exception Exception, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Exception, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(Exception Exception, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Exception, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogCritical(Exception Exception, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Exception, this.ObjectID, Tags);
		}

		#endregion

		#region Alert

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, StackTrace, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Message, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Message, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Message, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(string Message, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Message, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Message, this.ObjectID, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Exception, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Exception, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(Exception Exception, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Exception, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(Exception Exception, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Exception, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(Exception Exception, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Exception, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogAlert(Exception Exception, params KeyValuePair<string, object>[] Tags)
		{
			Log.Alert(Exception, this.ObjectID, Tags);
		}

		#endregion

		#region Emergency

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, StackTrace, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Message, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Message, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Message, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Message, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(string Message, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Message, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Message, this.ObjectID, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Exception, this.ObjectID, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Exception, this.ObjectID, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(Exception Exception, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Exception, this.ObjectID, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(Exception Exception, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Exception, this.ObjectID, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(Exception Exception, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Exception, this.ObjectID, Actor, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public void LogEmergency(Exception Exception, params KeyValuePair<string, object>[] Tags)
		{
			Log.Emergency(Exception, this.ObjectID, Tags);
		}

		#endregion

		/// <summary>
		/// <see cref="IDisposable.Dispose()"/>
		/// </summary>
		public virtual void Dispose()
		{
		}
	}
}
