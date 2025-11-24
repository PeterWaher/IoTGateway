using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Events
{
	/// <summary>
	/// Static class managing the application event log. Applications and services log events on this static class,
	/// and it distributes them to registered event sinks.
	/// </summary>
	public static class Log
	{
		private static Type[] alertExceptionTypes = Array.Empty<Type>();
		private static bool alertExceptionTypesLocked = false;
		private static IEventSink[] staticSinks = Array.Empty<IEventSink>();
		private readonly static List<IEventSink> dynamicSinks = new List<IEventSink>();
		private static Type[] nestedExceptionTypes = new Type[]
		{
			typeof(TargetInvocationException),
			typeof(TypeInitializationException)
		};

		/// <summary>
		/// Registers an event sink with the event log. Call <see cref="Unregister(IEventSink)"/> to unregister it, or
		/// <see cref="Terminate"/> at the end of an application, to unregister and terminate all registered event sinks.
		/// </summary>
		/// <param name="EventSink">Event Sink.</param>
		public static void Register(IEventSink EventSink)
		{
			if (!(EventSink is null))
			{
				lock (dynamicSinks)
				{
					dynamicSinks.Add(EventSink);
					staticSinks = dynamicSinks.ToArray();
				}
			}
		}

		/// <summary>
		/// Unregisters an event sink from the event log.
		/// </summary>
		/// <param name="EventSink">Event Sink.</param>
		/// <returns>If the sink was found and removed.</returns>
		public static bool Unregister(IEventSink EventSink)
		{
			if (!(EventSink is null))
			{
				lock (dynamicSinks)
				{
					if (dynamicSinks.Remove(EventSink))
					{
						staticSinks = dynamicSinks.ToArray();
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Register an exception type to unnest in logging. By default, only the <see cref="TargetInvocationException"/> is unnested.
		/// </summary>
		/// <param name="ExceptionType">Type of exception to unnest.</param>
		public static bool RegisterExceptionToUnnest(Type ExceptionType)
		{
			if (!typeof(Exception).IsAssignableFrom(ExceptionType.GetTypeInfo()))
				return false;

			lock (dynamicSinks)
			{
				if (Array.IndexOf(nestedExceptionTypes, ExceptionType) >= 0)
					return false;

				int c = nestedExceptionTypes.Length;
				Type[] Result = new Type[c + 1];
				Array.Copy(nestedExceptionTypes, 0, Result, 0, c);
				Result[c] = ExceptionType;
				nestedExceptionTypes = Result;
			}

			return true;
		}

		/// <summary>
		/// Must be called when the application is terminated. Stops all event sinks that have been registered.
		/// </summary>
		[Obsolete("Use TerminateAsync() instead.")]
		public static void Terminate()
		{
			TerminateAsync().Wait();
		}

		/// <summary>
		/// Must be called when the application is terminated. Stops all event sinks that have been registered.
		/// </summary>
		public static async Task TerminateAsync()
		{
			foreach (IEventSink Sink in Sinks)
			{
				Unregister(Sink);

				try
				{
					if (Sink is IDisposableAsync DisposableAsync)
						await DisposableAsync.DisposeAsync();
					else if (Sink is IDisposable Disposable)
						Disposable.Dispose();
				}
				catch (Exception)
				{
					// Ignore.
				}
			}

			EventHandlerAsync h = Terminating;
			if (!(h is null))
				await h(null, EventArgs.Empty);
		}

		/// <summary>
		/// Event raised when the application is terminating.
		/// </summary>
		public static event EventHandlerAsync Terminating = null;

		/// <summary>
		/// Registered sinks.
		/// </summary>
		public static IEventSink[] Sinks => staticSinks;

		/// <summary>
		/// Logs an event. It will be distributed to registered event sinks.
		/// </summary>
		/// <param name="Event">Event to log.</param>
		public static async void Event(Event Event)
		{
			foreach (IEventSink EventSink in staticSinks)
			{
				if (!Event.ShoudAvoid(EventSink))
				{
					try
					{
						await EventSink.Queue(Event);

						if (hasReportedErrors)
						{
							lock (reportedErrors)
							{
								if (reportedErrors.Remove(EventSink))
									hasReportedErrors = reportedErrors.Count > 0;
							}
						}
					}
					catch (Exception ex)
					{
						lock (reportedErrors)
						{
							if (reportedErrors.TryGetValue(EventSink, out bool b) && b)
								continue;

							reportedErrors[EventSink] = true;
							hasReportedErrors = true;
						}

						Event Event2 = new Event(DateTime.UtcNow, EventType.Critical, ex.Message, EventSink.ObjectID, string.Empty, string.Empty,
							EventLevel.Major, string.Empty, ex.Source, CleanStackTrace(ex.StackTrace));

						if (!(Event.ToAvoid is null))
						{
							foreach (IEventSink EventSink2 in Event.ToAvoid)
								Event2.Avoid(EventSink2);
						}

						Event2.Avoid(EventSink);

						Log.Event(Event2);
					}
				}
			}
		}

		private readonly static Dictionary<IEventSink, bool> reportedErrors = new Dictionary<IEventSink, bool>();
		private static bool hasReportedErrors = false;
		private static readonly char[] crlf = new char[] { '\r', '\n' };

		/// <summary>
		/// Cleans a Stack Trace string, removing entries from the asynchronous execution model, making the result more readable to developers.
		/// </summary>
		/// <param name="StackTrace">Stack Trace</param>
		/// <returns>Cleaned stack trace</returns>
		public static string CleanStackTrace(string StackTrace)
		{
			if (string.IsNullOrEmpty(StackTrace))
				return StackTrace;
			else
			{
				StringBuilder Result = new StringBuilder();

				foreach (string Row in StackTrace.Split(crlf, StringSplitOptions.RemoveEmptyEntries))
				{
					if (Row.StartsWith("   at System.") || (Row.StartsWith("--- ") && Row.EndsWith(" ---")))
						continue;

					Result.AppendLine(Row);
				}

				return Result.ToString();
			}
		}

		#region Debug

		/// <summary>
		/// Logs a debug event.
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
		public static void Debug(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Debug, Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags));
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Debug(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Debug, Message, Object, Actor, EventId, Level, Facility, Module, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Debug(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Debug, Message, Object, Actor, EventId, Level, Facility, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Debug(string Message, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Debug, Message, Object, Actor, EventId, Level, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Debug(string Message, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Debug, Message, Object, Actor, EventId, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Debug(string Message, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Debug, Message, Object, Actor, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Debug(string Message, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Debug, Message, Object, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a debug event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Debug(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Debug, Message, string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		#endregion

		#region Informational

		/// <summary>
		/// Logs an informational event.
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
		public static void Informational(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Informational, Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags));
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Informational(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Informational, Message, Object, Actor, EventId, Level, Facility, Module, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Informational(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Informational, Message, Object, Actor, EventId, Level, Facility, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Informational(string Message, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Informational, Message, Object, Actor, EventId, Level, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Informational(string Message, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Informational, Message, Object, Actor, EventId, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Informational(string Message, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Informational, Message, Object, Actor, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Informational(string Message, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Informational, Message, Object, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an informational event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Informational(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Informational, Message, string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		#endregion

		#region Notice

		/// <summary>
		/// Logs a notice event.
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
		public static void Notice(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Notice, Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags));
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Notice(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Notice, Message, Object, Actor, EventId, Level, Facility, Module, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Notice(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Notice, Message, Object, Actor, EventId, Level, Facility, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Notice(string Message, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Notice, Message, Object, Actor, EventId, Level, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Notice(string Message, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Notice, Message, Object, Actor, EventId, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Notice(string Message, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Notice, Message, Object, Actor, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Notice(string Message, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Notice, Message, Object, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a notice event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Notice(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Notice, Message, string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		#endregion

		#region Warning

		/// <summary>
		/// Logs a warning event.
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
		public static void Warning(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Warning, Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags));
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Warning(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Warning, Message, Object, Actor, EventId, Level, Facility, Module, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Warning(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Warning, Message, Object, Actor, EventId, Level, Facility, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Warning(string Message, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Warning, Message, Object, Actor, EventId, Level, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Warning(string Message, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Warning, Message, Object, Actor, EventId, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Warning(string Message, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Warning, Message, Object, Actor, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Warning(string Message, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Warning, Message, Object, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a warning event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Warning(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Warning, Message, string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		#endregion

		#region Error

		/// <summary>
		/// Logs an error event.
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
		public static void Error(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Error, Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Error, Message, Object, Actor, EventId, Level, Facility, Module, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Error, Message, Object, Actor, EventId, Level, Facility, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(string Message, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Error, Message, Object, Actor, EventId, Level, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(string Message, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Error, Message, Object, Actor, EventId, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(string Message, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Error, Message, Object, Actor, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(string Message, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Error, Message, Object, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Error, Message, string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Error, Exception, Object, Actor, EventId, Level, Facility, Module, Tags);
		}

		private static void Event(EventType Type, Exception Exception, string Object, string Actor, string EventId,
			EventLevel Level, string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Exception = UnnestException(Exception);

			if (Exception is AggregateException ex)
			{
				foreach (Exception ex2 in ex.InnerExceptions)
					Event(Type, ex2, Object, Actor, EventId, Level, Facility, Module, Tags);
			}
			else
				Event(new Event(Type, Exception, Object, Actor, EventId, Level, Facility, Module, Tags));
		}

		/// <summary>
		/// Unnests an exception, to extract the relevant inner exception.
		/// </summary>
		/// <param name="Exception">Exception</param>
		/// <returns>Unnested exception.</returns>
		public static Exception UnnestException(Exception Exception)
		{
			while (!(Exception?.InnerException is null))
			{
				Type T = Exception.GetType();
				if (Array.IndexOf(nestedExceptionTypes, T) < 0)
				{
					if (Exception is AggregateException AggregateException &&
						AggregateException.InnerExceptions.Count == 1)
					{
						Exception = AggregateException.InnerExceptions[0];
						continue;
					}

					break;
				}

				Exception = Exception.InnerException;
			}

			return Exception;
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Error, Exception, Object, Actor, EventId, Level, Facility, Tags);
		}

		private static void Event(EventType Type, Exception Exception, string Object, string Actor, string EventId,
			EventLevel Level, string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Exception = UnnestException(Exception);

			if (Exception is AggregateException ex)
			{
				foreach (Exception ex2 in ex.InnerExceptions)
					Event(Type, ex2, Object, Actor, EventId, Level, Facility, Tags);
			}
			else
				Event(new Event(Type, Exception, Object, Actor, EventId, Level, Facility, Exception.Source, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Error, Exception, Object, Actor, EventId, Level, Tags);
		}

		private static void Event(EventType Type, Exception Exception, string Object, string Actor, string EventId,
			EventLevel Level, params KeyValuePair<string, object>[] Tags)
		{
			Exception = UnnestException(Exception);

			if (Exception is AggregateException ex)
			{
				foreach (Exception ex2 in ex.InnerExceptions)
					Event(Type, ex2, Object, Actor, EventId, Level, Tags);
			}
			else
				Event(new Event(Type, Exception, Object, Actor, EventId, Level, string.Empty, Exception.Source, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(Exception Exception, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Error, Exception, Object, Actor, EventId, Tags);
		}

		private static void Event(EventType Type, Exception Exception, string Object, string Actor, string EventId,
			params KeyValuePair<string, object>[] Tags)
		{
			Exception = UnnestException(Exception);

			if (Exception is AggregateException ex)
			{
				foreach (Exception ex2 in ex.InnerExceptions)
					Event(Type, ex2, Object, Actor, EventId, Tags);
			}
			else
				Event(new Event(Type, Exception, Object, Actor, string.Empty, EventLevel.Minor, string.Empty, Exception.Source, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(Exception Exception, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Error, Exception, Object, Actor, Tags);
		}

		private static void Event(EventType Type, Exception Exception, string Object, string Actor,
			params KeyValuePair<string, object>[] Tags)
		{
			Exception = UnnestException(Exception);

			if (Exception is AggregateException ex)
			{
				foreach (Exception ex2 in ex.InnerExceptions)
					Event(Type, ex2, Object, Actor, Tags);
			}
			else
				Event(new Event(Type, Exception, Object, Actor, string.Empty, EventLevel.Minor, string.Empty, Exception.Source, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(Exception Exception, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Error, Exception, Object, Tags);
		}

		private static void Event(EventType Type, Exception Exception, string Object,
			params KeyValuePair<string, object>[] Tags)
		{
			Exception = UnnestException(Exception);

			if (Exception is AggregateException ex)
			{
				foreach (Exception ex2 in ex.InnerExceptions)
					Event(Type, ex2, Object, Tags);
			}
			else
				Event(new Event(Type, Exception, Object, string.Empty, string.Empty, EventLevel.Minor, string.Empty, Exception.Source, Tags));
		}

		/// <summary>
		/// Logs an error event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Error(Exception Exception, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Error, Exception, Tags);
		}

		private static void Event(EventType Type, Exception Exception, params KeyValuePair<string, object>[] Tags)
		{
			Exception = UnnestException(Exception);

			if (Exception is AggregateException ex)
			{
				foreach (Exception ex2 in ex.InnerExceptions)
					Event(Type, ex2, Tags);
			}
			else
				Event(new Event(Type, Exception, string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, Exception.Source, Tags));
		}

		#endregion

		#region Critical

		/// <summary>
		/// Logs a critical event.
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
		public static void Critical(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Critical, Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags));
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Critical, Message, Object, Actor, EventId, Level, Facility, Module, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Critical, Message, Object, Actor, EventId, Level, Facility, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(string Message, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Critical, Message, Object, Actor, EventId, Level, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(string Message, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Critical, Message, Object, Actor, EventId, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(string Message, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Critical, Message, Object, Actor, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(string Message, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Critical, Message, Object, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Critical, Message, string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Critical, Exception, Object, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Critical, Exception, Object, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Critical, Exception, Object, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(Exception Exception, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Critical, Exception, Object, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(Exception Exception, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Critical, Exception, Object, Actor, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(Exception Exception, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Critical, Exception, Object, Tags);
		}

		/// <summary>
		/// Logs a critical event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Critical(Exception Exception, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Critical, Exception, Tags);
		}

		#endregion

		#region Alert

		/// <summary>
		/// Logs an alert event.
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
		public static void Alert(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Alert, Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags));
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Alert, Message, Object, Actor, EventId, Level, Facility, Module, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Alert, Message, Object, Actor, EventId, Level, Facility, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(string Message, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Alert, Message, Object, Actor, EventId, Level, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(string Message, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Alert, Message, Object, Actor, EventId, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(string Message, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Alert, Message, Object, Actor, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(string Message, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Alert, Message, Object, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Alert, Message, string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Alert, Exception, Object, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Alert, Exception, Object, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Alert, Exception, Object, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(Exception Exception, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Alert, Exception, Object, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(Exception Exception, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Alert, Exception, Object, Actor, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(Exception Exception, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Alert, Exception, Object, Tags);
		}

		/// <summary>
		/// Logs an alert event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Alert(Exception Exception, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Alert, Exception, Tags);
		}

		#endregion

		#region Emergency

		/// <summary>
		/// Logs an emergency event.
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
		public static void Emergency(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Emergency, Message, Object, Actor, EventId, Level, Facility, Module, StackTrace, Tags));
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Emergency, Message, Object, Actor, EventId, Level, Facility, Module, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Emergency, Message, Object, Actor, EventId, Level, Facility, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(string Message, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Emergency, Message, Object, Actor, EventId, Level, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(string Message, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Emergency, Message, Object, Actor, EventId, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(string Message, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Emergency, Message, Object, Actor, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(string Message, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Emergency, Message, Object, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Event(new Event(EventType.Emergency, Message, string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty, Tags));
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Emergency, Exception, Object, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Emergency, Exception, Object, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Emergency, Exception, Object, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(Exception Exception, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Emergency, Exception, Object, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(Exception Exception, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Emergency, Exception, Object, Actor, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(Exception Exception, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Emergency, Exception, Object, Tags);
		}

		/// <summary>
		/// Logs an emergency event.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Emergency(Exception Exception, params KeyValuePair<string, object>[] Tags)
		{
			Event(EventType.Emergency, Exception, Tags);
		}

		#endregion

		#region Exceptions


		/// <summary>
		/// Logs an exception. Event type will be determined by the severity of the exception.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Exception(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, params KeyValuePair<string, object>[] Tags)
		{
			Event(GetEventType(Exception), Exception, Object, Actor, EventId, Level, Facility, Module, Tags);
		}

		/// <summary>
		/// Logs an exception. Event type will be determined by the severity of the exception.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Exception(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, params KeyValuePair<string, object>[] Tags)
		{
			Event(GetEventType(Exception), Exception, Object, Actor, EventId, Level, Facility, Tags);
		}

		/// <summary>
		/// Logs an exception. Event type will be determined by the severity of the exception.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Exception(Exception Exception, string Object, string Actor, string EventId, EventLevel Level,
			params KeyValuePair<string, object>[] Tags)
		{
			Event(GetEventType(Exception), Exception, Object, Actor, EventId, Level, Tags);
		}

		/// <summary>
		/// Logs an exception. Event type will be determined by the severity of the exception.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Exception(Exception Exception, string Object, string Actor, string EventId, params KeyValuePair<string, object>[] Tags)
		{
			Event(GetEventType(Exception), Exception, Object, Actor, EventId, Tags);
		}

		/// <summary>
		/// Logs an exception. Event type will be determined by the severity of the exception.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Exception(Exception Exception, string Object, string Actor, params KeyValuePair<string, object>[] Tags)
		{
			Event(GetEventType(Exception), Exception, Object, Actor, Tags);
		}

		/// <summary>
		/// Logs an exception. Event type will be determined by the severity of the exception.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Exception(Exception Exception, string Object, params KeyValuePair<string, object>[] Tags)
		{
			Event(GetEventType(Exception), Exception, Object, Tags);
		}

		/// <summary>
		/// Logs an exception. Event type will be determined by the severity of the exception.
		/// </summary>
		/// <param name="Exception">Exception Object.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public static void Exception(Exception Exception, params KeyValuePair<string, object>[] Tags)
		{
			Exception = UnnestException(Exception);
			Event(GetEventType(Exception), Exception, Tags);
		}

		/// <summary>
		/// Gets the event type corresponding to a given exception object.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		/// <returns>Event Type corresponding to <paramref name="Exception"/>.</returns>
		public static EventType GetEventType(Exception Exception)
		{
			if (Array.IndexOf(alertExceptionTypes, Exception.GetType()) >= 0)
				return EventType.Alert;
			else
				return EventType.Critical;
		}

		/// <summary>
		/// Registers a set of Exception types as Exception types that should generate 
		/// alert log entries when logged.
		/// </summary>
		/// <param name="Lock">If list of exceptions should be locked after the call.</param>
		/// <param name="ExceptionTypes">Types of exceptions.</param>
		/// <exception cref="InvalidOperationException">If list has been locked and new types cannot be added.</exception>
		public static void RegisterAlertExceptionType(bool Lock, params Type[] ExceptionTypes)
		{
			List<Type> NewTypes = new List<Type>();
			bool Changed = false;

			NewTypes.AddRange(alertExceptionTypes);

			foreach (Type ExceptionType in ExceptionTypes)
			{
				if (!NewTypes.Contains(ExceptionType))
				{
					if (alertExceptionTypesLocked)
						throw new InvalidOperationException("List of alert exception types has been locked.");

					NewTypes.Add(ExceptionType);
					Changed = true;
				}
			}

			if (Changed)
				alertExceptionTypes = NewTypes.ToArray();

			alertExceptionTypesLocked = Lock;
		}

		#endregion

		#region Extensions

		/// <summary>
		/// Joins two arrays
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="Array">Original array</param>
		/// <param name="NewElements">New elements to join.</param>
		/// <returns>Joined array.</returns>
		public static T[] Join<T>(this T[] Array, params T[] NewElements)
		{
			int c = Array?.Length ?? 0;
			if (c == 0)
				return NewElements;

			int d = NewElements?.Length ?? 0;
			if (d == 0)
				return Array;

			T[] Result = new T[c + d];

			System.Array.Copy(Array, 0, Result, 0, c);
			System.Array.Copy(NewElements, 0, Result, c, d);

			return Result;
		}

		/// <summary>
		/// Compares two arrays to see if their elements are equal
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="Array1">First array</param>
		/// <param name="Array2">Second array.</param>
		/// <returns>If the elements of both arrays are equal.</returns>
		public static bool ElementEquals<T>(this T[] Array1, params T[] Array2)
		{
			int c1 = Array1?.Length ?? 0;
			int c2 = Array2?.Length ?? 0;
			int i = c1 - c2;

			if (i != 0)
				return false;

			for (int j = 0; j < c1; j++)
			{
				if (!Array1[j].Equals(Array2[j]))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Compares two arrays
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="Array1">First array</param>
		/// <param name="Array2">Second array.</param>
		/// <returns>Comparison of the arrays.</returns>
		public static int Compare<T>(this T[] Array1, params T[] Array2)
			where T : IComparable
		{
			int c1 = Array1?.Length ?? 0;
			int c2 = Array2?.Length ?? 0;
			int i = c1 - c2;

			if (i != 0)
				return i;

			for (int j = 0; j < c1; j++)
			{
				i = Array1[j].CompareTo(Array2[j]);
				if (i != 0)
					return i;
			}

			return 0;
		}

		#endregion
	}
}
