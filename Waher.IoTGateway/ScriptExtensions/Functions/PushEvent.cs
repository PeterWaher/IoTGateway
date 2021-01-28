using System;
using Waher.Content;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Pushes an event to all open pages (tabs) showing a specific page, that include the Events.js file.
	/// </summary>
	public class PushEvent : FunctionMultiVariate
	{
		/// <summary>
		/// Pushes an event to all open pages (tabs) showing a specific page, that include the Events.js file.
		/// </summary>
		/// <param name="Event">Event to push.</param>
		/// <param name="Data">Data to push with the event.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PushEvent(ScriptNode Event, ScriptNode Data, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Event, Data }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Pushes an event to all open pages (tabs) showing a specific page, that include the Events.js file.
		/// </summary>
		/// <param name="Page">Page.</param>
		/// <param name="Event">Event to push.</param>
		/// <param name="Data">Data to push with the event.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PushEvent(ScriptNode Page, ScriptNode Event, ScriptNode Data, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Page, Event, Data }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Pushes an event to all open pages (tabs) showing a specific page, that include the Events.js file.
		/// </summary>
		/// <param name="Page">Page.</param>
		/// <param name="Event">Event to push.</param>
		/// <param name="Data">Data to push with the event.</param>
		/// <param name="QueryFilter">Query filter.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PushEvent(ScriptNode Page, ScriptNode QueryFilter, ScriptNode Event, ScriptNode Data, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Page, QueryFilter, Event, Data }, argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "PushEvent";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Page", "QueryFilter", "Event", "Data" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int c = Arguments.Length;
			int d = c - 2;
			IElement[] A = new IElement[d];

			Array.Copy(Arguments, 0, A, 0, d);
			string[] TabIDs = GetTabIDs.GetTabs(A, this);

			if (TabIDs.Length > 0)
			{
				object Data = Arguments[c - 1].AssociatedObjectValue;

				if (Data is string s)
					ClientEvents.PushEvent(TabIDs, Arguments[c - 2].AssociatedObjectValue?.ToString(), s, false);
				else
					ClientEvents.PushEvent(TabIDs, Arguments[c - 2].AssociatedObjectValue?.ToString(), JSON.Encode(Data, false), true);
			}

			return new ObjectVector(TabIDs);
		}
	}
}
