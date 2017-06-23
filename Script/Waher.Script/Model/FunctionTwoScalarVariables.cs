using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for funcions of two scalar variables.
    /// </summary>
    public abstract class FunctionTwoScalarVariables : FunctionTwoVariables
    {
        /// <summary>
        /// Base class for funcions of one scalar variable.
        /// </summary>
        /// <param name="Argument1">Argument 1.</param>
        /// <param name="Argument2">Argument 2.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public FunctionTwoScalarVariables(ScriptNode Argument1, ScriptNode Argument2, int Start, int Length, Expression Expression)
            : base(Argument1, Argument2, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument1">Function argument 1.</param>
        /// <param name="Argument2">Function argument 2.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement Argument1, IElement Argument2, Variables Variables)
        {
            if (Argument1.IsScalar)
            {
                if (Argument2.IsScalar)
                {
                    ISet Set1 = Argument1.AssociatedSet;
                    ISet Set2 = Argument2.AssociatedSet;

                    if (Set1 != Set2)
                    {
                        if (!Expression.Upgrade(ref Argument1, ref Set1, ref Argument2, ref Set2, this))
                            return this.EvaluateScalar(Argument1, Argument2, Variables);
                    }

                    DoubleNumber DoubleNumber1 = Argument1 as DoubleNumber;
                    DoubleNumber DoubleNumber2 = Argument2 as DoubleNumber;
                    if (DoubleNumber1 != null && DoubleNumber2 != null)
                        return this.EvaluateScalar(DoubleNumber1.Value, DoubleNumber2.Value, Variables);

                    ComplexNumber ComplexNumber1 = Argument1 as ComplexNumber;
                    ComplexNumber ComplexNumber2 = Argument2 as ComplexNumber;
                    if (ComplexNumber1 != null && ComplexNumber2 != null)
                        return this.EvaluateScalar(ComplexNumber1.Value, ComplexNumber2.Value, Variables);

                    BooleanValue BooleanValue1 = Argument1 as BooleanValue;
                    BooleanValue BooleanValue2 = Argument2 as BooleanValue;
                    if (BooleanValue1 != null && BooleanValue2 != null)
                        return this.EvaluateScalar(BooleanValue1.Value, BooleanValue2.Value, Variables);

                    StringValue StringValue1 = Argument1 as StringValue;
                    StringValue StringValue2 = Argument2 as StringValue;
                    if (StringValue1 != null && StringValue2 != null)
                        return this.EvaluateScalar(StringValue1.Value, StringValue2.Value, Variables);

					double arg1, arg2;
					PhysicalQuantity Q;

					if (DoubleNumber1 != null)
						arg1 = DoubleNumber1.Value;
					else
					{
						Q = Argument1 as PhysicalQuantity;
						if (Q != null)
							arg1 = Q.Magnitude;
						else
							return this.EvaluateScalar(Argument1, Argument2, Variables);
					}

					if (DoubleNumber2 != null)
						arg2 = DoubleNumber2.Value;
					else
					{
						Q = Argument2 as PhysicalQuantity;
						if (Q != null)
							arg2 = Q.Magnitude;
						else
							return this.EvaluateScalar(Argument1, Argument2, Variables);
					}

					return this.EvaluateScalar(arg1, arg2, Variables);
                }
                else
                {
                    LinkedList<IElement> Elements = new LinkedList<IElement>();

                    foreach (IElement E in Argument2.ChildElements)
                        Elements.AddLast(this.Evaluate(Argument1, E, Variables));

                    return Argument2.Encapsulate(Elements, this);
                }
            }
            else
            {
                if (Argument2.IsScalar)
                {
                    LinkedList<IElement> Elements = new LinkedList<IElement>();

                    foreach (IElement E in Argument1.ChildElements)
                        Elements.AddLast(this.Evaluate(E, Argument2, Variables));

                    return Argument1.Encapsulate(Elements, this);
                }
                else
                {
                    ICollection<IElement> Argument1Children = Argument1.ChildElements;
                    ICollection<IElement> Argument2Children = Argument2.ChildElements;

                    if (Argument1Children.Count == Argument2Children.Count)
                    {
                        LinkedList<IElement> Elements = new LinkedList<IElement>();
                        IEnumerator<IElement> eArgument1 = Argument1Children.GetEnumerator();
                        IEnumerator<IElement> eArgument2 = Argument2Children.GetEnumerator();

                        try
                        {
                            while (eArgument1.MoveNext() && eArgument2.MoveNext())
                                Elements.AddLast(this.Evaluate(eArgument1.Current, eArgument2.Current, Variables));
                        }
                        finally
                        {
                            eArgument1.Dispose();
                            eArgument2.Dispose();
                        }

                        return Argument1.Encapsulate(Elements, this);
                    }
                    else
                    {
                        LinkedList<IElement> Argument1Result = new LinkedList<IElement>();

                        foreach (IElement Argument1Child in Argument1Children)
                        {
                            LinkedList<IElement> Argument2Result = new LinkedList<IElement>();

                            foreach (IElement Argument2Child in Argument2Children)
                                Argument2Result.AddLast(this.Evaluate(Argument1Child, Argument2Child, Variables));

                            Argument1Result.AddLast(Argument2.Encapsulate(Argument2Result, this));
                        }

                        return Argument1.Encapsulate(Argument1Result, this);
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates the function on two scalar arguments.
        /// </summary>
        /// <param name="Argument1">Function argument 1.</param>
        /// <param name="Argument2">Function argument 2.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
        {
            throw new ScriptRuntimeException("Type of scalar not supported.", this);
        }

        /// <summary>
        /// Evaluates the function on two scalar arguments.
        /// </summary>
        /// <param name="Argument1">Function argument 1.</param>
        /// <param name="Argument2">Function argument 2.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateScalar(double Argument1, double Argument2, Variables Variables)
        {
            throw new ScriptRuntimeException("Double-valued arguments not supported.", this);
        }

        /// <summary>
        /// Evaluates the function on two scalar arguments.
        /// </summary>
        /// <param name="Argument1">Function argument 1.</param>
        /// <param name="Argument2">Function argument 2.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateScalar(Complex Argument1, Complex Argument2, Variables Variables)
        {
            throw new ScriptRuntimeException("Complex-valued arguments not supported.", this);
        }

        /// <summary>
        /// Evaluates the function on two scalar arguments.
        /// </summary>
        /// <param name="Argument1">Function argument 1.</param>
        /// <param name="Argument2">Function argument 2.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateScalar(bool Argument1, bool Argument2, Variables Variables)
        {
            throw new ScriptRuntimeException("Boolean-valued arguments not supported.", this);
        }

        /// <summary>
        /// Evaluates the function on two scalar arguments.
        /// </summary>
        /// <param name="Argument1">Function argument 1.</param>
        /// <param name="Argument2">Function argument 2.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
        {
            throw new ScriptRuntimeException("String-valued arguments not supported.", this);
        }

    }
}
