using System;
using System.Collections.Generic;
using Waher.Events;
using Waher.Script.Model;

namespace Waher.Script.Functions.Logging
{
	/// <summary>
	/// Logs a critical event to the event log.
	/// </summary>
	public class LogCritical : LogFunction
	{
		/// <summary>
		/// Logs a critical event to the event log.
		/// </summary>
		/// <param name="Message">Message</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LogCritical(ScriptNode Message, int Start, int Length, Expression Expression)
			: base(Message, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Logs a critical event to the event log.
		/// </summary>
		/// <param name="Message">Argument.</param>
		/// <param name="Tags">Tags</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LogCritical(ScriptNode Message, ScriptNode Tags, int Start, int Length, Expression Expression)
			: base(Message, Tags, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "LogCritical";

		/// <summary>
		/// Logs information to the event log.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		public override void DoLog(string Message)
		{
			Log.Critical(Message);
		}

		/// <summary>
		/// Logs information to the event log.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="StackTrace">Stack Trace of event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public override void DoLog(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Log.Critical(Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags);
		}
	}
}
