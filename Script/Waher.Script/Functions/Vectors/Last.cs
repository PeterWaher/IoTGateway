using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Last(v)
    /// </summary>
    public class Last : FunctionOneVectorVariable, IIterativeEvaluator
    {
        /// <summary>
        /// Last(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Last(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Last);

        /// <summary>
        /// Evaluates the function on a non-vector. By default, the non-vector argument is converted to a vector of length 1.
        /// </summary>
        /// <param name="Argument">Non-vector argument.</param>
        /// <param name="Variables">Variables.</param>
        /// <returns>Result</returns>
        protected override IElement EvaluateNonVector(IElement Argument, Variables Variables)
        {
            return Argument;
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(IVector Argument, Variables Variables)
        {
            int c = Argument.Dimension;
            if (c == 0)
                return ObjectValue.Null;
            else
                return Argument.GetElement(c - 1);
        }

		#region IIterativeEvaluator

		private IElement last = null;

		/// <summary>
		/// If the evaluator can perform the computation iteratively.
		/// </summary>
		public bool CanEvaluateIteratively => true;

		/// <summary>
		/// Creates a new instance of the iterative evaluator.
		/// </summary>
		/// <returns>Reference to new instance.</returns>
		public IIterativeEvaluator CreateNewEvaluator()
		{
			return new Last(this.Argument, this.Start, this.Length, this.Expression);
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.last = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			this.last = Element;
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			return this.last ?? ObjectValue.Null;
		}

		#endregion

	}
}
