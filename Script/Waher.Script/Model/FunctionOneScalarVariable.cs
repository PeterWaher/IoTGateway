using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for funcions of one scalar variable.
    /// </summary>
    public abstract class FunctionOneScalarVariable : FunctionOneVariable
    {
        /// <summary>
        /// Base class for funcions of one scalar variable.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public FunctionOneScalarVariable(ScriptNode Argument, int Start, int Length)
            : base(Argument, Start, Length)
        {
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            if (Argument.IsScalar)
            {
                DoubleNumber DoubleNumber = Argument as DoubleNumber;
                if (DoubleNumber != null)
                    return this.EvaluateScalar(DoubleNumber.Value, Variables);

                ComplexNumber ComplexNumber = Argument as ComplexNumber;
                if (ComplexNumber != null)
                    return this.EvaluateScalar(ComplexNumber.Value, Variables);

                BooleanValue BooleanValue = Argument as BooleanValue;
                if (BooleanValue != null)
                    return this.EvaluateScalar(BooleanValue.Value, Variables);

                StringValue StringValue = Argument as StringValue;
                if (StringValue != null)
                    return this.EvaluateScalar(StringValue.Value, Variables);

                return this.EvaluateScalar(Argument, Variables);
            }
            else
            {
                IVector Vector = Argument as IVector;
                if (Vector != null)
                {
                    LinkedList<IElement> Elements = new LinkedList<IElement>();
                    int i, c = Vector.Dimension;

                    if (Vector is DoubleVector)
                    {
                        double[] v = ((DoubleVector)Vector).Values;
                        for (i = 0; i < c; i++)
                            Elements.AddLast(this.EvaluateScalar(v[i], Variables));
                    }
                    else if (Vector is ComplexVector)
                    {
                        Complex[] v = ((ComplexVector)Vector).Values;
                        for (i = 0; i < c; i++)
                            Elements.AddLast(this.EvaluateScalar(v[i], Variables));
                    }
                    else if (Vector is BooleanVector)
                    {
                        bool[] v = ((BooleanVector)Vector).Values;
                        for (i = 0; i < c; i++)
                            Elements.AddLast(this.EvaluateScalar(v[i], Variables));
                    }
                    else
                    {
                        foreach (IElement E in Vector.ChildElements)
                            Elements.AddLast(this.Evaluate(E, Variables));
                    }

                    return Argument.Encapsulate(Elements, this);
                }
                else
                {
                    LinkedList<IElement> Elements = new LinkedList<IElement>();

                    foreach (IElement E in Argument.ChildElements)
                        Elements.AddLast(this.Evaluate(E, Variables));

                    return Argument.Encapsulate(Elements, this);
                }
            }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateScalar(IElement Argument, Variables Variables)
        {
            throw new ScriptRuntimeException("Type of scalar not supported.", this);
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateScalar(double Argument, Variables Variables)
        {
            throw new ScriptRuntimeException("Double-valued arguments not supported.", this);
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateScalar(Complex Argument, Variables Variables)
        {
            throw new ScriptRuntimeException("Complex-valued arguments not supported.", this);
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateScalar(bool Argument, Variables Variables)
        {
            throw new ScriptRuntimeException("Boolean-valued arguments not supported.", this);
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateScalar(string Argument, Variables Variables)
        {
            throw new ScriptRuntimeException("String-valued arguments not supported.", this);
        }

    }
}
