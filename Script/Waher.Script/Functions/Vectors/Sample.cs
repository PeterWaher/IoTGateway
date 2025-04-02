using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Sample(v)
    /// </summary>
    public class Sample : FunctionOneVectorVariable
    {
        private static readonly Random rnd = new Random();

        /// <summary>
        /// Sample(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Sample(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Sample);

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
				throw new ScriptRuntimeException("Dimension must be positive.", this);

			int i = rnd.Next(c);

			return Argument.GetElement(i);
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
        {
			int c = Argument.Dimension;
            if (c == 0)
                throw new ScriptRuntimeException("Dimension must be positive.", this);

            int i = rnd.Next(c);

            return Argument.GetElement(i);
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(ComplexVector Argument, Variables Variables)
        {
			int c = Argument.Dimension;
			if (c == 0)
				throw new ScriptRuntimeException("Dimension must be positive.", this);

			int i = rnd.Next(c);

			return Argument.GetElement(i);
		}
	}
}
