using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Functions.Logging
{
	/// <summary>
	/// Abstract base class for log functions
	/// </summary>
	public abstract class LogFunction : FunctionMultiVariate
	{
		/// <summary>
		/// Abstract base class for log functions
		/// </summary>
		/// <param name="Message">Message</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LogFunction(ScriptNode Message, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Message }, new ArgumentType[] { ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Abstract base class for log functions
		/// </summary>
		/// <param name="Message">Argument.</param>
		/// <param name="Tags">Tags</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LogFunction(ScriptNode Message, ScriptNode Tags, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Message, Tags }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Message" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			object M = Arguments[0].AssociatedObjectValue;
			string Object = string.Empty;
			string Actor = string.Empty;
			string EventId = string.Empty;
			string Facility = string.Empty;
			string Module = string.Empty;
			string StackTrace = string.Empty;
			EventLevel Level = EventLevel.Minor;
			List<KeyValuePair<string, object>> Tags = null;
			bool Detailed = false;

			if (!(M is string Message))
			{
				if (M is Exception ex)
				{
					string s;

					Message = ex.Message;
					Object = ex is IEventObject Obj2 && !string.IsNullOrEmpty(s = Obj2.Object) ? s : Object;
					Actor = ex is IEventActor Act && !string.IsNullOrEmpty(s = Act.Actor) ? s : Actor;
					EventId = ex is IEventId EvId && !string.IsNullOrEmpty(s = EvId.EventId) ? s : EventId;
					Level = ex is IEventLevel Lvl && Lvl.Level.HasValue ? Lvl.Level.Value : Level;
					Facility = ex is IEventFacility EvFa && !string.IsNullOrEmpty(s = EvFa.Facility) ? s : Facility;
					Module = ex is IEventModule Mod && !string.IsNullOrEmpty(s = Mod.Module) ? s : ex.Source;
					StackTrace = Log.CleanStackTrace(ex.StackTrace);

					if (ex is IEventTags Tags2)
					{
						KeyValuePair<string, object>[] Tags3 = Tags2.Tags;
						if (!(Tags3 is null))
						{
							Tags = new List<KeyValuePair<string, object>>();
							Tags.AddRange(Tags3);
						}
					}

					Detailed = true;
				}
				else
					Message = M?.ToString() ?? string.Empty;
			}

			if (Arguments.Length > 1 &&
				Arguments[1].AssociatedObjectValue is IDictionary<string, IElement> Obj)
			{
				foreach (KeyValuePair<string, IElement> P in Obj)
				{
					switch (P.Key.ToUpper())
					{
						case "OBJECT":
							Object = P.Value.AssociatedObjectValue?.ToString() ?? string.Empty;
							break;

						case "ACTOR":
							Actor = P.Value.AssociatedObjectValue?.ToString() ?? string.Empty;
							break;

						case "EVENTID":
							EventId = P.Value.AssociatedObjectValue?.ToString() ?? string.Empty;
							break;

						case "FACILITY":
							Facility = P.Value.AssociatedObjectValue?.ToString() ?? string.Empty;
							break;

						case "MODULE":
							Module = P.Value.AssociatedObjectValue?.ToString() ?? string.Empty;
							break;

						case "STACKTRACE":
							StackTrace = P.Value.AssociatedObjectValue?.ToString() ?? string.Empty;
							break;

						case "LEVEL":
							if (P.Value.AssociatedObjectValue is EventLevel L ||
								Enum.TryParse<EventLevel>(P.Value.AssociatedObjectValue?.ToString() ?? string.Empty, out L))
							{
								Level = L;
							}
							else
							{
								if (Tags is null)
									Tags = new List<KeyValuePair<string, object>>();

								Tags.Add(new KeyValuePair<string, object>(P.Key, P.Value.AssociatedObjectValue));
							}
							break;

						default:
							if (Tags is null)
								Tags = new List<KeyValuePair<string, object>>();

							Tags.Add(new KeyValuePair<string, object>(P.Key, P.Value.AssociatedObjectValue));
							break;
					}
				}

				Detailed = true;
			}

			if (Detailed)
			{
				this.DoLog(Message, Object, Actor, EventId, Level, Facility, Module, StackTrace,
					Tags?.ToArray() ?? new KeyValuePair<string, object>[0]);
			}
			else
				this.DoLog(Message);

			return Arguments[0];
		}

		/// <summary>
		/// Logs information to the event log.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		public abstract void DoLog(string Message);

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
		public abstract void DoLog(string Message, string Object, string Actor, string EventId, EventLevel Level,
			string Facility, string Module, string StackTrace, params KeyValuePair<string, object>[] Tags);
	}
}
