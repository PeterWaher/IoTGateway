using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime
{
    /// <summary>
    /// Checks if an expression exists, or has a valid value.
    /// </summary>
    public class Exists : FunctionOneVariable
    {
        /// <summary>
        /// Checks if an expression exists, or has a valid value.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Exists(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "exists"; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            try
            {
                IElement E = this.Argument.Evaluate(Variables);
                DoubleNumber D = E as DoubleNumber;
                if (D != null)
                {
                    if (double.IsNaN(D.Value))
                        return BooleanValue.False;
                    else
                        return BooleanValue.True;
                }

                ComplexNumber C = E as ComplexNumber;
                if (C != null)
                {
                    if (double.IsNaN(C.Value.Real) || double.IsNaN(C.Value.Imaginary))
                        return BooleanValue.False;
                    else
                        return BooleanValue.True;
                }

                ObjectValue O = E as ObjectValue;
                if (O != null && O.Value == null)
                    return BooleanValue.False;
                else
                    return BooleanValue.True;
            }
            catch (Exception)
            {
                return BooleanValue.False;
            }
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            return BooleanValue.True;
        }
    }
}
