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
        private readonly ScriptNode[] elements;

        /// <summary>
        /// Represents a list of elements.
        /// </summary>
        /// <param name="Elements">Elements.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
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
                if (E is null)
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

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			int i, c = this.elements.Length;

			if (DepthFirst)
			{
				if (!ForAllChildNodes(Callback, this.elements, State, DepthFirst))
					return false;
			}

			for (i = 0; i < c; i++)
			{
				if (!Callback(ref this.elements[i], State))
					return false;
			}

			if (!DepthFirst)
			{
				for (i = 0; i < c; i++)
				{
					if (!ForAllChildNodes(Callback, this.elements, State, DepthFirst))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is ElementList O &&
				this.elements.Equals(O.elements) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.elements.GetHashCode();
			return Result;
		}

	}
}
