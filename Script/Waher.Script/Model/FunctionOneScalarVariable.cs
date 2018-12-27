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
		/// <param name="Expression">Expression containing script.</param>
        public FunctionOneScalarVariable(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
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
                if (Argument is DoubleNumber DoubleNumber)
                    return this.EvaluateScalar(DoubleNumber.Value, Variables);

                if (Argument is ComplexNumber ComplexNumber)
                    return this.EvaluateScalar(ComplexNumber.Value, Variables);

                if (Argument is BooleanValue BooleanValue)
                    return this.EvaluateScalar(BooleanValue.Value, Variables);

                if (Argument is StringValue StringValue)
                    return this.EvaluateScalar(StringValue.Value, Variables);

				if (Argument is PhysicalQuantity PhysicalQuantity)
					return this.EvaluateScalar(PhysicalQuantity.Magnitude, Variables);

				return this.EvaluateScalar(Argument, Variables);
            }
            else
            {
                if (Argument is IVector Vector)
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
			object Value = Argument.AssociatedObjectValue;

			if (Expression.TryConvert<string>(Value, out string s))
				return this.EvaluateScalar(s, Variables);
			else if (Expression.TryConvert<double>(Value, out double d))
				return this.EvaluateScalar(d, Variables);
			else if (Expression.TryConvert<bool>(Value, out bool b))
				return this.EvaluateScalar(b, Variables);
			else if (Expression.TryConvert<Complex>(Value, out Complex z))
				return this.EvaluateScalar(z, Variables);
			else
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
