using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// First(v)
    /// </summary>
    public class First : FunctionOneVectorVariable, IIterativeEvaluation
    {
        /// <summary>
        /// First(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public First(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(First);

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
                return Argument.GetElement(0);
        }

		#region IIterativeEvaluation

		/// <summary>
		/// If the node can be evaluated iteratively.
		/// </summary>
		public bool CanEvaluateIteratively => true;

		/// <summary>
		/// Creates an iterative evaluator for the node.
		/// </summary>
		/// <returns>Iterative evaluator reference.</returns>
		public IIterativeEvaluator CreateEvaluator() => new FirstEvaluator(this);

		#endregion

	}
}
