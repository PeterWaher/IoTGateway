using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Operators
{
    /// <summary>
    /// Represents a list of elements.
    /// </summary>
    public class ElementList : ScriptNode
    {
        ScriptNode[] elements;

        /// <summary>
        /// Represents a list of elements.
        /// </summary>
        /// <param name="Elements">Elements.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public ElementList(ScriptNode[] Elements, int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
            this.elements = Elements;
        }

        /// <summary>
        /// Elements.
        /// </summary>
        public ScriptNode[] Elements
        {
            get { return this.elements; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            LinkedList<IElement> List = new LinkedList<IElement>();
            int c = 0;

            foreach (ScriptNode E in this.elements)
            {
                if (E == null)
                    List.AddLast((IElement)null);
                else
                    List.AddLast(E.Evaluate(Variables));

                c++;
            }

            switch (c)
            {
                case 0:
                    return ObjectValue.Null;

                case 1:
                    return List.First.Value;

                case 2:
                    DoubleNumber Re, Im;

                    if ((Re=List.First.Value as DoubleNumber)!=null &&
                        (Im=List.First.Next.Value as DoubleNumber)!= null)
                    {
                        return new ComplexNumber(new Complex(Re.Value, Im.Value));
                    }
                    break;
            }

            return VectorDefinition.Encapsulate(List, true, this);
        }

    }
}
