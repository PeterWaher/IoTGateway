using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Elements.Interfaces;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Creates a vector.
	/// </summary>
	public class VectorDefinition : ElementList
	{
		/// <summary>
		/// Creates a vector.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public VectorDefinition(ScriptNode[] Elements, int Start, int Length)
			: base(Elements, Start, Length)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			LinkedList<IElement> VectorElements = new LinkedList<IElement>();

			foreach (ScriptNode Node in this.Elements)
				VectorElements.AddLast(Node.Evaluate(Variables));

			return Encapsulate(VectorElements, this);
		}

		/// <summary>
		/// Encapsulates the elements of a vector.
		/// </summary>
		/// <param name="Elements">Vector elements.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated vector.</returns>
		public static IVector Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			IElement SuperSetExample = null;
			IElement Element2;
			ISet CommonSuperSet = null;
			ISet Set;
			bool Upgraded = false;

			foreach (IElement Element in Elements)
			{
				if (CommonSuperSet == null)
				{
					SuperSetExample = Element;

                    if (Element == null)
                        CommonSuperSet = new ObjectValues();
                    else
                        CommonSuperSet = Element.AssociatedSet;
				}
				else
				{
                    if (Element == null)
                        Set = new ObjectValues();
                    else
                        Set = Element.AssociatedSet;

					if (!Set.Equals(CommonSuperSet))
					{
						Element2 = Element;
						if (!Expression.Upgrade(ref Element2, ref Set, ref SuperSetExample, ref CommonSuperSet, Node))
						{
							CommonSuperSet = null;
							break;
						}
						else
							Upgraded = true;
					}
				}
			}

			if (CommonSuperSet != null)
			{
				if (Upgraded)
				{
					LinkedList<IElement> SuperElements = new LinkedList<IElement>();

					foreach (IElement Element in Elements)
					{
                        if (Element == null)
                            Set = new ObjectValues();
                        else
                            Set = Element.AssociatedSet;

						if (Set.Equals(CommonSuperSet))
							SuperElements.AddLast(Element);
						else
						{
							Element2 = Element;
							if (Expression.Upgrade(ref Element2, ref Set, ref SuperSetExample, ref CommonSuperSet, Node))
								SuperElements.AddLast(Element2);
							else
							{
								SuperElements = null;
								CommonSuperSet = null;
								break;
							}
						}
					}

					if (SuperElements != null)
						Elements = SuperElements;
				}

				if (CommonSuperSet != null)
				{
					if (CommonSuperSet is DoubleNumbers)
						return new DoubleVector(Elements);
					else if (CommonSuperSet is BooleanValues)
						return new BooleanVector(Elements);
				}
			}

			return new ObjectVector(Elements);
		}

	}
}
