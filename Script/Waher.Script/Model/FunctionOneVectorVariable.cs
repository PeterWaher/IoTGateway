using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
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
        public FunctionOneVectorVariable(ScriptNode Argument, int Start, int Length)
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
            IVector Vector = Argument as IVector;
            if (Vector != null)
                return this.EvaluateVector(Vector, Variables);
            else
            {
                IMatrix Matrix = Argument as IMatrix;
                if (Matrix != null)
                {
                    LinkedList<IElement> Elements = new LinkedList<IElement>();
                    int i, c = Matrix.Rows;

                    for (i = 0; i < c; i++)
                        Elements.AddLast(this.Evaluate(Matrix.GetRow(i), Variables));

                    return Argument.Encapsulate(Elements, this);
                }
                else
                {
                    ISet Set = Argument as ISet;
                    if (Set != null)
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

    }
}
