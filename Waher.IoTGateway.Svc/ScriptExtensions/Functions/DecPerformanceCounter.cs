using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.Svc.ScriptExtensions.Functions
{
    /// <summary>
    /// Decrements a performance counter, given a performance category name, optional instance name, as well as a counter name.
    /// </summary>
    public class DecPerformanceCounter : FunctionMultiVariate
	{
        /// <summary>
        /// Decrements a performance counter, given a performance category name, optional instance name, as well as a counter name.
        /// </summary>
        /// <param name="Category">Performance counter category.</param>
        /// <param name="CounterName">Performance counter name.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public DecPerformanceCounter(ScriptNode Category, ScriptNode CounterName, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Category, CounterName }, argumentTypes2Scalar, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Decrements a performance counter, given a performance category name, optional instance name, as well as a counter name.
        /// </summary>
        /// <param name="Category">Performance counter category.</param>
        /// <param name="InstanceName">Performance counter category instance name.</param>
        /// <param name="CounterName">Performance counter name.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public DecPerformanceCounter(ScriptNode Category, ScriptNode InstanceName, ScriptNode CounterName, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Category, InstanceName, CounterName }, argumentTypes3Scalar, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "DecPerformanceCounter"; }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "Category", "Instance", "Counter" };

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Arguments">Function arguments.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
            System.Diagnostics.PerformanceCounter Result;

            if (Arguments.Length == 2)
            {
                Result = Svc.PerformanceCounters.GetCounter(
                    Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty,
                    Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty);
            }
            else
            {
                Result = Svc.PerformanceCounters.GetCounter(
                    Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty,
                    Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty,
                    Arguments[2].AssociatedObjectValue?.ToString() ?? string.Empty);
            }

            return new DoubleNumber(Result.Decrement());
        }
    }
}
