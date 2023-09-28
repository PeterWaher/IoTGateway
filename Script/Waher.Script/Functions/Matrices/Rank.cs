using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Matrices
{
    /// <summary>
    /// Rank(x)
    /// </summary>
    public class Rank : FunctionOneVariable
    {
        /// <summary>
        /// Rank(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Rank(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Rank);

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement Argument, Variables Variables)
        {
			if (Argument is IMatrix M)
			{
                if (M.Reduce(false, false, out int Rank, out _) is null || Rank < 0)
					throw new ScriptRuntimeException("Unable to compute rank of matrix.", this);

				return new DoubleNumber(Rank);
			}
            else
				throw new ScriptRuntimeException("Expected matrix argument.", this);
		}
	}
}
