using System;
using System.Diagnostics;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.Svc.ScriptExtensions.Functions
{
    /// <summary>
    /// Returns a <see cref="PerformanceCounterCategory"/> object, given the category name.
    /// </summary>
    public class PerformanceCategory : FunctionOneScalarVariable
	{
        /// <summary>
        /// Returns a <see cref="PerformanceCounterCategory"/> object, given the category name.
        /// </summary>
        /// <param name="Category">Performance counter category.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public PerformanceCategory(ScriptNode Category, int Start, int Length, Expression Expression)
            : base(Category, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "PerformanceCategory"; }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
            PerformanceCounterCategory Result = Svc.PerformanceCounters.GetCategory(Argument);
            if (Result is null)
                throw new ScriptRuntimeException("Performance category not found: " + Argument, this);

            return new ObjectValue(Result);
        }
    }
}
