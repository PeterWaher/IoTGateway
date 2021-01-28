using System;
using Waher.Runtime.Cache;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
    /// <summary>
    /// Clears all caches defined by Waher.Runtime.Cache.
    /// </summary>
    public class ClearCaches : FunctionMultiVariate
	{
		/// <summary>
		/// Clears all caches defined by Waher.Runtime.Cache.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ClearCaches(int Start, int Length, Expression Expression)
			: base(new ScriptNode[0], argumentTypes0, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "ClearCaches";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[0];

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Arguments">Function arguments.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			Caches.ClearAll();
			return ObjectValue.Null;
		}
	}
}
