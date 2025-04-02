using System.Threading.Tasks;
using Waher.Runtime.Collections;
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
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "v" };

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            if (!(Argument is IVector Vector))
            {
                if (Argument is IMatrix Matrix)
                {
					ChunkedList<IElement> Elements = new ChunkedList<IElement>();
                    int i, c = Matrix.Rows;

                    if (Matrix is DoubleMatrix)
                    {
                        for (i = 0; i < c; i++)
                            Elements.Add(this.Evaluate((DoubleVector)Matrix.GetRow(i), Variables));
                    }
                    else if (Matrix is ComplexMatrix)
                    {
                        for (i = 0; i < c; i++)
                            Elements.Add(this.Evaluate((ComplexVector)Matrix.GetRow(i), Variables));
                    }
                    else if (Matrix is BooleanMatrix)
                    {
                        for (i = 0; i < c; i++)
                            Elements.Add(this.Evaluate((BooleanVector)Matrix.GetRow(i), Variables));
                    }
                    else
                    {
                        for (i = 0; i < c; i++)
                            Elements.Add(this.Evaluate(Matrix.GetRow(i), Variables));
                    }

                    return Argument.Encapsulate(Elements, this);
                }
                else
                {
                    if (Argument is ISet Set)
                    {
						ChunkedList<IElement> Elements = new ChunkedList<IElement>();

                        foreach (IElement E in Set.ChildElements)
                            Elements.Add(this.Evaluate(E, Variables));

                        return Argument.Encapsulate(Elements, this);
                    }
                    else
                        return this.EvaluateNonVector(Argument, Variables);
                }
            }

            if (Vector is DoubleVector DoubleVector)
                return this.EvaluateVector(DoubleVector, Variables);

            if (Vector is ComplexVector ComplexVector)
                return this.EvaluateVector(ComplexVector, Variables);

            if (Vector is BooleanVector BooleanVector)
                return this.EvaluateVector(BooleanVector, Variables);

            return this.EvaluateVector(Vector, Variables);
        }

        /// <summary>
        /// Evaluates the function on a non-vector. By default, the non-vector argument is converted to a vector of length 1.
        /// </summary>
        /// <param name="Argument">Non-vector argument.</param>
        /// <param name="Variables">Variables.</param>
        /// <returns>Result</returns>
        protected virtual IElement EvaluateNonVector(IElement Argument, Variables Variables)
        {
            IVector Vector = VectorDefinition.Encapsulate(new IElement[] { Argument }, false, this);

            if (Vector is DoubleVector DoubleVector)
                return this.EvaluateVector(DoubleVector, Variables);

            if (Vector is ComplexVector ComplexVector)
                return this.EvaluateVector(ComplexVector, Variables);

            if (Vector is BooleanVector BooleanVector)
                return this.EvaluateVector(BooleanVector, Variables);

            return this.EvaluateVector(Vector, Variables);
        }


        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override async Task<IElement> EvaluateAsync(IElement Argument, Variables Variables)
        {
            if (!(Argument is IVector Vector))
            {
                if (Argument is IMatrix Matrix)
                {
					ChunkedList<IElement> Elements = new ChunkedList<IElement>();
                    int i, c = Matrix.Rows;

                    if (Matrix is DoubleMatrix)
                    {
                        for (i = 0; i < c; i++)
                            Elements.Add(await this.EvaluateAsync((DoubleVector)Matrix.GetRow(i), Variables));
                    }
                    else if (Matrix is ComplexMatrix)
                    {
                        for (i = 0; i < c; i++)
                            Elements.Add(await this.EvaluateAsync((ComplexVector)Matrix.GetRow(i), Variables));
                    }
                    else if (Matrix is BooleanMatrix)
                    {
                        for (i = 0; i < c; i++)
                            Elements.Add(await this.EvaluateAsync((BooleanVector)Matrix.GetRow(i), Variables));
                    }
                    else
                    {
                        for (i = 0; i < c; i++)
                            Elements.Add(await this.EvaluateAsync(Matrix.GetRow(i), Variables));
                    }

                    return Argument.Encapsulate(Elements, this);
                }
                else
                {
                    if (Argument is ISet Set)
                    {
						ChunkedList<IElement> Elements = new ChunkedList<IElement>();

                        foreach (IElement E in Set.ChildElements)
                            Elements.Add(await this.EvaluateAsync(E, Variables));

                        return Argument.Encapsulate(Elements, this);
                    }
                    else
                        return await this.EvaluateNonVectorAsync(Argument, Variables);
                }
            }

            if (Vector is DoubleVector DoubleVector)
                return await this.EvaluateVectorAsync(DoubleVector, Variables);

            if (Vector is ComplexVector ComplexVector)
                return await this.EvaluateVectorAsync(ComplexVector, Variables);

            if (Vector is BooleanVector BooleanVector)
                return await this.EvaluateVectorAsync(BooleanVector, Variables);

            return await this.EvaluateVectorAsync(Vector, Variables);
        }

        /// <summary>
        /// Evaluates the function on a non-vector. By default, the non-vector argument is converted to a vector of length 1.
        /// </summary>
        /// <param name="Argument">Non-vector argument.</param>
        /// <param name="Variables">Variables.</param>
        /// <returns>Result</returns>
        protected virtual async Task<IElement> EvaluateNonVectorAsync(IElement Argument, Variables Variables)
        {
            IVector Vector = VectorDefinition.Encapsulate(new IElement[] { Argument }, false, this);

            if (Vector is DoubleVector DoubleVector)
                return await this.EvaluateVectorAsync(DoubleVector, Variables);

            if (Vector is ComplexVector ComplexVector)
                return await this.EvaluateVectorAsync(ComplexVector, Variables);

            if (Vector is BooleanVector BooleanVector)
                return await this.EvaluateVectorAsync(BooleanVector, Variables);

            return await this.EvaluateVectorAsync(Vector, Variables);
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
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual Task<IElement> EvaluateVectorAsync(IVector Argument, Variables Variables)
		{
            return Task.FromResult(this.EvaluateVector(Argument, Variables));
		}

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual Task<IElement> EvaluateVectorAsync(DoubleVector Argument, Variables Variables)
        {
            return Task.FromResult(this.EvaluateVector(Argument, Variables));
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual Task<IElement> EvaluateVectorAsync(ComplexVector Argument, Variables Variables)
        {
            return Task.FromResult(this.EvaluateVector(Argument, Variables));
        }

        /// <summary>
        /// Evaluates the function on a vector argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public virtual Task<IElement> EvaluateVectorAsync(BooleanVector Argument, Variables Variables)
        {
            return Task.FromResult(this.EvaluateVector(Argument, Variables));
        }
    }
}
