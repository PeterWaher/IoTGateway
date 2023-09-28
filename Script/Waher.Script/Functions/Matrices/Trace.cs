using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Vectors;
using Waher.Script.Model;

namespace Waher.Script.Functions.Matrices
{
    /// <summary>
    /// Trace(x)
    /// </summary>
    public class Trace : FunctionOneVariable
    {
        /// <summary>
        /// Trace(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Trace(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Trace);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "tr" };

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
                int N = Math.Min(M.Columns, M.Rows);
                IElement[] Elements = new IElement[N];
                int i;

                for (i = 0; i < N; i++)
                    Elements[i] = M.GetElement(i, i);

                return Sum.EvaluateSum(Elements, this);
			}
            else
				throw new ScriptRuntimeException("Expected matrix argument.", this);
		}
	}
}
