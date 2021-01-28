using System;
using System.Collections.Generic;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;
using Waher.Security;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Gets an array of Tab IDs (as strings) of currently open pages, given certain conditions.
	/// </summary>
	public class GetTabIDs : FunctionMultiVariate
	{
		/// <summary>
		/// Gets an array of Tab IDs (as strings) of currently open pages, given certain conditions.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetTabIDs(int Start, int Length, Expression Expression)
			: base(new ScriptNode[0], argumentTypes0, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets an array of Tab IDs (as strings) of currently open pages, given certain conditions.
		/// </summary>
		/// <param name="Page">Page.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetTabIDs(ScriptNode Page, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Page }, argumentTypes1Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets an array of Tab IDs (as strings) of currently open pages, given certain conditions.
		/// </summary>
		/// <param name="Page">Page.</param>
		/// <param name="QueryFilter">Query filter.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetTabIDs(ScriptNode Page, ScriptNode QueryFilter, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Page, QueryFilter }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "GetTabIDs";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Page", "QueryFilter" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return new ObjectVector(GetTabs(Arguments, this));
		}

		/// <summary>
		/// Gets TabIDs based in restrictions in input arguments.
		/// </summary>
		/// <param name="Arguments">Tab restrictions.</param>
		/// <param name="Node">Script node getting the tabs.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabs(IElement[] Arguments, ScriptNode Node)
		{ 
			switch (Arguments.Length)
			{
				case 0:
					return ClientEvents.GetTabIDs();

				case 1:
					object Obj = Arguments[0].AssociatedObjectValue;
					if (Obj is Array A)
					{ 
						if (!(A is string[] Pages))
							Pages = (string[])Expression.ConvertTo(A, typeof(string[]), Node);
						
						return ClientEvents.GetTabIDsForLocations(Pages);
					}
					else if (Obj is IUser User)
						return ClientEvents.GetTabIDsForUser(User);
					else
						return ClientEvents.GetTabIDsForLocation(Obj?.ToString());

				case 2:
					return ClientEvents.GetTabIDsForLocation(Arguments[0].AssociatedObjectValue?.ToString(),
						GetQueryFilter(Arguments[1], Node));

				default:
					return new string[0];
			}
		}

		private static KeyValuePair<string, string>[] GetQueryFilter(IElement Argument, ScriptNode Node)
		{
			if (!(Argument.AssociatedObjectValue is IDictionary<string, IElement> Obj))
				throw new ScriptRuntimeException("Query filter must be an object ex nihilo.", Node);

			List<KeyValuePair<string, string>> Result = new List<KeyValuePair<string, string>>();

			foreach (KeyValuePair<string, IElement> P in Obj)
				Result.Add(new KeyValuePair<string, string>(P.Key, P.Value.AssociatedObjectValue?.ToString()));

			return Result.ToArray();
		}
	}
}
