using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Operators.Matrices
{
	/// <summary>
	/// Creates a matrix.
	/// </summary>
	public class MatrixDefinition : ElementList
	{
		/// <summary>
		/// Creates a matrix.
		/// </summary>
		/// <param name="Rows">Row vectors.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public MatrixDefinition(ScriptNode[] Rows, int Start, int Length)
			: base(Rows, Start, Length)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			LinkedList<IElement> Rows = new LinkedList<IElement>();

			foreach (ScriptNode Node in this.Elements)
				Rows.AddLast(Node.Evaluate(Variables));

			return Encapsulate(Rows, this);
		}

		/// <summary>
		/// Encapsulates the elements of a matrix.
		/// </summary>
		/// <param name="Rows">Matrix rows.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated matrix.</returns>
		public static IElement Encapsulate(ICollection<IElement> Rows, ScriptNode Node)
		{
			LinkedList<IElement> Elements = new LinkedList<IElement>();
			IVectorSpaceElement Vector;
			int? Columns = null;
			int i;

			foreach (IElement Row in Rows)
			{
				Vector = Row as IVectorSpaceElement;

				if (Vector == null)
				{
					Columns = -1;
					break;
				}
				else
				{
					i = Vector.Dimension;
					if (Columns.HasValue)
					{
						if (Columns.Value != i)
						{
							Columns = -1;
							break;
						}
					}
					else
						Columns = i;

					foreach (IElement Element in Vector.ChildElements)
						Elements.AddLast(Element);
				}
			}

			if (!Columns.HasValue || Columns.Value < 0)
				return Operators.Vectors.VectorDefinition.Encapsulate(Rows, Node);
			else
				return Encapsulate(Elements, Rows.Count, Columns.Value, Node);
		}

		/// <summary>
		/// Encapsulates the elements of a matrix.
		/// </summary>
		/// <param name="Elements">Matrix elements.</param>
		/// <param name="Rows">Rows</param>
		/// <param name="Columns">Columns</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated matrix.</returns>
		public static IElement Encapsulate(ICollection<IElement> Elements, int Rows, int Columns, ScriptNode Node)
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
					CommonSuperSet = Element.AssociatedSet;
				}
				else
				{
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
						Set = Element.AssociatedSet;

						if (Set.Equals(CommonSuperSet))
							SuperElements.AddLast(Element);
						else
						{
							Element2 = Element;
							if (Expression.Upgrade(ref Element2, ref Set, ref SuperSetExample, ref CommonSuperSet, Node) && Element2 is IVectorSpaceElement)
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
						return new DoubleMatrix(Rows, Columns, Elements);
					else if (CommonSuperSet is BooleanValues)
						return new BooleanMatrix(Rows, Columns, Elements);
				}
			}

			return new ObjectMatrix(Rows, Columns, Elements);
		}

	}
}
