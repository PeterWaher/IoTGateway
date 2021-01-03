using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for funcions of one vector variable.
    /// </summary>
    public abstract class FunctionOneVectorVariable : FunctionOneVariable
    {
        /// <summary>
        /// Base class for funcions of one vector variable.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public FunctionOneVectorVariable(ScriptNode Argument, int Start, int Length, Expression Expression)
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
            if (Argument is IVector Vector)
            {
                if (Vector is DoubleVector DoubleVector)
                    return this.EvaluateVector(DoubleVector, Variables);

                if (Vector is ComplexVector ComplexVector)
                    return this.EvaluateVector(ComplexVector, Variables);

                if (Vector is BooleanVector BooleanVector)
                    return this.EvaluateVector(BooleanVector, Variables);

                return this.EvaluateVector(Vector, Variables);
            }
            else
            {
                if (Argument is IMatrix Matrix)
                {
                    LinkedList<IElement> Elements = new LinkedList<IElement>();
                    int i, c = Matrix.Rows;

                    if (Matrix is DoubleMatrix)
                    {
                        for (i = 0; i < c; i++)
                            Elements.AddLast(this.Evaluate((DoubleVector)Matrix.GetRow(i), Variables));
                    }
                    else if (Matrix is ComplexMatrix)
                    {
                        for (i = 0; i < c; i++)
                            Elements.AddLast(this.Evaluate((ComplexVector)Matrix.GetRow(i), Variables));
                    }
                    else if (Matrix is BooleanMatrix)
                    {
                        for (i = 0; i < c; i++)
                            Elements.AddLast(this.Evaluate((BooleanVector)Matrix.GetRow(i), Variables));
                    }
                    else
                    {
                        for (i = 0; i < c; i++)
                            Elements.AddLast(this.Evaluate(Matrix.GetRow(i), Variables));
                    }

                    return Argument.Encapsulate(Elements, this);
                }
                else
                {
                    if (Argument is ISet Set)
                    {
                        LinkedList<IElement> Elements = new LinkedList<IElement>();

                        foreach (IElement E in Set.ChildElements)
                            Elements.AddLast(this.Evaluate(E, Variables));

                        return Argument.Encapsulate(Elements, this);
                    }
                    else
                        return this.EvaluateVector((IVector)VectorDefinition.Encapsulate(new IElement[] { Argument }, false, this), Variables);
                }
            }
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public abstract IElement EvaluateVector(IVector Argument, Variables Variables);

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateVector(DoubleVector Argument, Variables Variables)
        {
            return this.EvaluateVector((IVector)Argument, Variables);
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateVector(ComplexVector Argument, Variables Variables)
        {
            return this.EvaluateVector((IVector)Argument, Variables);
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual IElement EvaluateVector(BooleanVector Argument, Variables Variables)
        {
            return this.EvaluateVector((IVector)Argument, Variables);
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames
        {
            get { return new string[] { "v" }; }
        }

    }
}
