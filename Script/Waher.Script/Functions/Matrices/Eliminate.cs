using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Functions.Matrices
{
    /// <summary>
    /// Eliminate(x)
    /// </summary>
    public class Eliminate : FunctionOneVariable
    {
        /// <summary>
        /// Eliminate(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Eliminate(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Eliminate);

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
                IMatrix N = M.Reduce(true, false, out _, out _)
                    ?? throw new ScriptRuntimeException("Unable to reduce and eliminate matrix.", this);

				return N;
			}
            else
				throw new ScriptRuntimeException("Expected matrix argument.", this);
		}
	}
}
