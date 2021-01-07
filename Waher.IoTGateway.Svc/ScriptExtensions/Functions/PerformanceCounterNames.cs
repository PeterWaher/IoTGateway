using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.IoTGateway.Svc.ScriptExtensions.Functions
{
    /// <summary>
    /// Returns an array of names of performance counters within a given performance category.
    /// </summary>
    public class PerformanceCounterNames : FunctionMultiVariate
    {
        /// <summary>
        /// Returns an array of names of performance counters within a given performance category.
        /// </summary>
        /// <param name="Category">Performance counter category.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public PerformanceCounterNames(ScriptNode Category, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Category }, argumentTypes1Scalar, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Returns an array of names of performance counters within a given performance category.
        /// </summary>
        /// <param name="Category">Performance counter category.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public PerformanceCounterNames(ScriptNode Category, ScriptNode InstanceName, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Category, InstanceName }, argumentTypes2Scalar, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "PerformanceCounterNames"; }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "Category", "Instance" };

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Arguments">Function arguments.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            string[] Result;

            if (Arguments.Length == 1)
                Result = Svc.PerformanceCounters.GetCounterNames(Arguments[0].AssociatedObjectValue.ToString());
            else
            {
                Result = Svc.PerformanceCounters.GetCounterNames(
                    Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty,
                    Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty);
            }

            return new ObjectVector(Result);
        }
    }
}
