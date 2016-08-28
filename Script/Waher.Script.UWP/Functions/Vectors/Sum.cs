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
    public class Sum : FunctionOneVectorVariable
    {
        /// <summary>
        /// Sum(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Sum(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "sum"; }
        }

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
            ISemiGroupElement Result = null;
            ISemiGroupElement SE;
            ISemiGroupElement Sum;

            foreach (IElement E in Vector.ChildElements)
            {
                SE = E as ISemiGroupElement;
                if (SE == null)
                    throw new ScriptRuntimeException("Elements not addable.", Node);

                if (Result == null)
                    Result = SE;
                else
                {
                    Sum = Result.AddRight(SE);
                    if (Sum == null)
                        Sum = (ISemiGroupElement)Operators.Arithmetics.Add.EvaluateAddition(Result, SE, Node);

                    Result = Sum;
                }
            }

            if (Result == null)
                return ObjectValue.Null;
            else
                return Result;
        }

    }
}
