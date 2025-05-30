﻿using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Sum(v)
    /// </summary>
    public class Sum : FunctionOneVectorVariable, IIterativeEvaluation
    {
        /// <summary>
        /// Sum(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Sum(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Sum);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "∑" };

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(IVector Argument, Variables Variables)
        {
            return EvaluateSum(Argument, this);
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
        {
            return new DoubleNumber(CalcSum(Argument.Values));
        }

        /// <summary>
        /// Calculates the sum of a set of double values.
        /// </summary>
        /// <param name="Values">Values</param>
        /// <returns>Sum.</returns>
        public static double CalcSum(double[] Values)
        {
            double Result = 0;
            int i, c = Values.Length;

            for (i = 0; i < c; i++)
                Result += Values[i];

            return Result;
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(ComplexVector Argument, Variables Variables)
        {
            return new ComplexNumber(CalcSum(Argument.Values));
        }

        /// <summary>
        /// Calculates the sum of a set of complex values.
        /// </summary>
        /// <param name="Values">Values</param>
        /// <returns>Sum.</returns>
        public static Complex CalcSum(Complex[] Values)
        {
            Complex Result = Complex.Zero;
            int i, c = Values.Length;

            for (i = 0; i < c; i++)
                Result += Values[i];

            return Result;
        }

        /// <summary>
        /// Sums the elements of a vector.
        /// </summary>
        /// <param name="Vector">Vector</param>
        /// <param name="Node">Node performing evaluation.</param>
        /// <returns>Sum of elements.</returns>
        public static IElement EvaluateSum(IVector Vector, ScriptNode Node)
        {
            return EvaluateSum(Vector.VectorElements, Node);
        }

		/// <summary>
		/// Sums the elements of a vector.
		/// </summary>
		/// <param name="Elements">Elements to sum.</param>
		/// <param name="Node">Node performing evaluation.</param>
		/// <returns>Sum of elements.</returns>
		public static IElement EvaluateSum(ICollection<IElement> Elements, ScriptNode Node)
		{
			ISemiGroupElement Result = null;
            ISemiGroupElement SE;
            ISemiGroupElement Sum;

            foreach (IElement E in Elements)
            {
                SE = E as ISemiGroupElement;
                if (SE is null)
				{
					if (Elements.Count == 1)
						return E;
					else
						throw new ScriptRuntimeException("Elements not addable.", Node);
				}

				if (Result is null)
                    Result = SE;
                else
                {
                    Sum = Result.AddRight(SE);
                    if (Sum is null)
                        Sum = (ISemiGroupElement)Operators.Arithmetics.Add.EvaluateAddition(Result, SE, Node);

                    Result = Sum;
                }
            }

            if (Result is null)
                return ObjectValue.Null;
            else
                return Result;
        }

		#region IIterativeEvalaution

		/// <summary>
		/// If the node can be evaluated iteratively.
		/// </summary>
		public bool CanEvaluateIteratively => true;

        /// <summary>
        /// Creates an iterative evaluator for the node.
        /// </summary>
        /// <returns>Iterative evaluator reference.</returns>
        public IIterativeEvaluator CreateEvaluator() => new SumEvaluator(this);

		#endregion
	}
}
