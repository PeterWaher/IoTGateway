using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Order
{
	/// <summary>
	/// Orders vectors based on values of elements at a given index position in the vectors.
	/// </summary>
	public class IndexOrder : IComparer<IElement>
	{
		private readonly ScriptNode node;
		private readonly int index;
		private readonly int sign;

		/// <summary>
		/// Orders vectors based on values of elements at a given index position in the vectors.
		/// </summary>
		/// <param name="Node">Node performing evaluation</param>
		/// <param name="Index">Index position to compare.</param>
		/// <param name="Sign">If ascending (1) or descending (-1) order is desired.</param>
		public IndexOrder(ScriptNode Node, int Index, int Sign)
		{
			this.node = Node;
			this.index = Index;
			this.sign = Sign;
		}

		/// <summary>
		/// Compares two elements.
		/// </summary>
		/// <param name="x">First element</param>
		/// <param name="y">Second element</param>
		/// <returns>Ordinal difference between elements.</returns>
		public int Compare(IElement x, IElement y)
		{
			if (x is IVector v1 && y is IVector v2)
			{
				IElement e1 = v1.GetElement(this.index);
				IElement e2 = v2.GetElement(this.index);

				return this.sign * ElementOrder.Compare(e1, e2, this.node);
			}
			else
				throw new ScriptRuntimeException("Elements in array must be vectors.", this.node);
		}
	}
}
