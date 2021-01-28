using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
    /// <summary>
    /// Reloads all open pages (tabs) showing a specific page, that include the Events.js file.
    /// </summary>
    public class ReloadPage : FunctionMultiVariate
	{
		/// <summary>
		/// Gets an array of Tab IDs (as strings) of currently open pages, given certain conditions.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ReloadPage(int Start, int Length, Expression Expression)
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
		public ReloadPage(ScriptNode Page, int Start, int Length, Expression Expression)
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
		public ReloadPage(ScriptNode Page, ScriptNode QueryFilter, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Page, QueryFilter }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "ReloadPage";

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
			string[] TabIDs = GetTabIDs.GetTabs(Arguments, this);

			if (TabIDs.Length > 0)
				ClientEvents.PushEvent(TabIDs, "Reload", "");

			return new ObjectVector(TabIDs);
		}
	}
}
