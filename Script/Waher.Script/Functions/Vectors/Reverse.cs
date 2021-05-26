using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Reverse(v)
    /// </summary>
    public class Reverse : FunctionOneVectorVariable
    {
        /// <summary>
        /// Reverse(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Reverse(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "reverse"; }
        }

		/// <summary>
		/// Evaluates the function on a non-vector. By default, the non-vector argument is converted to a vector of length 1.
		/// </summary>
		/// <param name="Argument">Non-vector argument.</param>
		/// <param name="Variables">Variables.</param>
		/// <returns>Result</returns>
		protected override IElement EvaluateNonVector(IElement Argument, Variables Variables)
		{
			if (Argument is StringValue S)
			{
				string s = S.Value;
				if (string.IsNullOrEmpty(s))
					return S;

				char[] Characters = s.ToCharArray();

				System.Array.Reverse(Characters);

				return new StringValue(new string(Characters));
			}
			else
				return base.EvaluateNonVector(Argument, Variables);
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(IVector Argument, Variables Variables)
        {
			int i, c = Argument.Dimension;
			IElement[] E = new IElement[c];

			for (i = 0; i < c; i++)
				E[c - i - 1] = Argument.GetElement(i);
			
			return Argument.Encapsulate(E, this);
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
        {
			int i, c = Argument.Dimension;
			double[] E = new double[c];
			double[] V = Argument.Values;

			for (i = 0; i < c; i++)
				E[c - i - 1] = V[i];

			return new DoubleVector(E);
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(ComplexVector Argument, Variables Variables)
        {
			int i, c = Argument.Dimension;
			Complex[] E = new Complex[c];
			Complex[] V = Argument.Values;

			for (i = 0; i < c; i++)
				E[c - i - 1] = V[i];

			return new ComplexVector(E);
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(BooleanVector Argument, Variables Variables)
		{
			int i, c = Argument.Dimension;
			bool[] E = new bool[c];
			bool[] V = Argument.Values;

			for (i = 0; i < c; i++)
				E[c - i - 1] = V[i];

			return new BooleanVector(E);
		}

	}
}
