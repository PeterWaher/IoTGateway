using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// Product(v), Prod(v)
    /// </summary>
    public class Product : FunctionOneVectorVariable
    {
        /// <summary>
        /// Product(v), Prod(v)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Product(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "product"; }
        }

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases
        {
            get { return new string[] { "prod", "∏" }; }
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
        {
            return new DoubleNumber(CalcProduct(Argument.Values));
        }

        /// <summary>
        /// Calculates the product of a set of double values.
        /// </summary>
        /// <param name="Values">Values</param>
        /// <returns>Product.</returns>
        public static double CalcProduct(double[] Values)
        {
            double Result = 1;
            int i, c = Values.Length;

            for (i = 0; i < c; i++)
                Result *= Values[i];

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
            return new ComplexNumber(CalcProduct(Argument.Values));
        }

        /// <summary>
        /// Calculates the product of a set of complex values.
        /// </summary>
        /// <param name="Values">Values</param>
        /// <returns>Product.</returns>
        public static Complex CalcProduct(Complex[] Values)
        {
            Complex Result = Complex.One;
            int i, c = Values.Length;

            for (i = 0; i < c; i++)
                Result *= Values[i];

            return Result;
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateVector(IVector Argument, Variables Variables)
        {
            IRingElement Result = null;
            IRingElement RE;
            IRingElement Product;

            foreach (IElement E in Argument.ChildElements)
            {
                RE = E as IRingElement;
                if (RE is null)
                    throw new ScriptRuntimeException("Elements cannot be multiplied.", this);

                if (Result is null)
                    Result = RE;
                else
                {
                    Product = Result.MultiplyRight(RE);
                    if (Product is null)
                        Product = (IRingElement)Operators.Arithmetics.Multiply.EvaluateMultiplication(Result, RE, this);

                    Result = Product;
                }
            }

            if (Result is null)
                return ObjectValue.Null;
            else
                return Result;
        }

    }
}
