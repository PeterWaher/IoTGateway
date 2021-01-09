using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.IoTGateway.Svc.ScriptExtensions.Functions
{
    /// <summary>
    /// Returns an array of performance counter instance names, given a performance category.
    /// </summary>
    public class PerformanceInstances : FunctionOneScalarVariable
	{
        /// <summary>
        /// Returns an array of performance counter instance names, given a performance category.
        /// </summary>
        /// <param name="Category">Performance counter category.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public PerformanceInstances(ScriptNode Category, int Start, int Length, Expression Expression)
            : base(Category, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "PerformanceInstances"; }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
            string[] Result = Svc.PerformanceCounters.GetInstanceNames(Argument);
            return new ObjectVector(Result);
        }
    }
}
