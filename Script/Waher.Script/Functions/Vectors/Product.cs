using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

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
        public Product(ScriptNode Argument, int Start, int Length)
            : base(Argument, Start, Length)
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
            get { return new string[] { "prod" }; }
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
                if (RE == null)
                    throw new ScriptRuntimeException("Elements cannot be multiplied.", this);

                if (Result == null)
                    Result = RE;
                else
                {
                    Product = Result.MultiplyRight(RE);
                    if (Product == null)
                        Product = (IRingElement)Operators.Arithmetics.Multiply.EvaluateMultiplication(Result, RE, this);

                    Result = Product;
                }
            }

            if (Result == null)
                return ObjectValue.Null;
            else
                return Result;
        }

    }
}
