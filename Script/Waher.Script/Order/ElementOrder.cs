using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Order
{
	/// <summary>
	/// Orders elements based on their values.
	/// </summary>
	public class ElementOrder : IComparer<IElement>
	{
		private readonly ScriptNode node;

		/// <summary>
		/// Orders elements based on their values.
		/// </summary>
		/// <param name="Node">Node performing evaluation.</param>
		public ElementOrder(ScriptNode Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Compares two elements.
		/// </summary>
		/// <param name="x">First element</param>
		/// <param name="y">Second element</param>
		/// <returns>Ordinal difference between elements.</returns>
		public int Compare(IElement x, IElement y)
		{
			return Compare(x, y, this.node);
		}

		/// <summary>
		/// Compares two elements using the order operator defined by their associated sets.
		/// </summary>
		/// <param name="x">First operand.</param>
		/// <param name="y">Second operand.</param>
		/// <param name="Node">Node performing the comparison.</param>
		/// <returns>Comparison result.</returns>
		public static int Compare(IElement x, IElement y, ScriptNode Node)
		{
			if (x.AssociatedSet is IOrderedSet OrderedSet1)
				return OrderedSet1.Compare(x, y);
			else if (y.AssociatedSet is IOrderedSet OrderedSet2)
				return OrderedSet2.Compare(x, y);
			else
				throw new ScriptRuntimeException("Elements not ordered", Node);
		}

	}
}
