using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Events
{
	/// <summary>
	/// Interface for objects that can log events.
	/// </summary>
	public interface ILogObject : IDisposable
	{
		/// <summary>
		/// Object ID, used when logging events.
		/// </summary>
		string ObjectID
		{
			get;
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
		void LogDebug(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags);

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
		void LogDebug(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogDebug(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogDebug(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogDebug(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogDebug(string Message, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogDebug(string Message, params KeyValuePair<string, object>[] Tags);

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
		void LogInformational(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags);

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
		void LogInformational(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogInformational(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogInformational(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogInformational(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogInformational(string Message, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogInformational(string Message, params KeyValuePair<string, object>[] Tags);

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
		void LogNotice(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags);

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
		void LogNotice(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogNotice(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogNotice(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogNotice(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogNotice(string Message, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogNotice(string Message, params KeyValuePair<string, object>[] Tags);

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
		void LogWarning(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags);

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
		void LogWarning(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogWarning(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogWarning(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogWarning(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogWarning(string Message, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogWarning(string Message, params KeyValuePair<string, object>[] Tags);

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
		void LogError(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags);

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
		void LogError(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogError(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogError(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogError(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogError(string Message, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogError(string Message, params KeyValuePair<string, object>[] Tags);

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
		void LogError(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogError(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogError(Exception Exception, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogError(Exception Exception, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogError(Exception Exception, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogError(Exception Exception, params KeyValuePair<string, object>[] Tags);

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
		void LogCritical(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags);

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
		void LogCritical(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogCritical(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogCritical(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogCritical(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogCritical(string Message, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogCritical(string Message, params KeyValuePair<string, object>[] Tags);

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
		void LogCritical(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogCritical(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogCritical(Exception Exception, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogCritical(Exception Exception, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogCritical(Exception Exception, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogCritical(Exception Exception, params KeyValuePair<string, object>[] Tags);

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
		void LogAlert(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags);

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
		void LogAlert(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogAlert(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogAlert(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogAlert(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogAlert(string Message, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogAlert(string Message, params KeyValuePair<string, object>[] Tags);

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
		void LogAlert(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogAlert(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogAlert(Exception Exception, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogAlert(Exception Exception, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogAlert(Exception Exception, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogAlert(Exception Exception, params KeyValuePair<string, object>[] Tags);

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
		void LogEmergency(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags);

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
		void LogEmergency(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogEmergency(string Message, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogEmergency(string Message, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogEmergency(string Message, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogEmergency(string Message, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogEmergency(string Message, params KeyValuePair<string, object>[] Tags);

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
		void LogEmergency(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogEmergency(Exception Exception, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogEmergency(Exception Exception, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogEmergency(Exception Exception, string Actor, string EventId, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogEmergency(Exception Exception, string Actor, params KeyValuePair<string, object>[] Tags);

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		void LogEmergency(Exception Exception, params KeyValuePair<string, object>[] Tags);

		#endregion
	}
}
