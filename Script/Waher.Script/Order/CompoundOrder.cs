using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Order
{
	/// <summary>
	/// Orders elements based on a sequence of comparers.
	/// </summary>
	public class CompoundOrder : IComparer<IElement>
	{
		private readonly IComparer<IElement>[] comparers;
		private readonly int c;

		/// <summary>
		/// Orders elements based on a sequence of comparers.
		/// </summary>
		/// <param name="Comparers">Comparers</param>
		public CompoundOrder(IComparer<IElement>[] Comparers)
		{
			this.comparers = Comparers;
			this.c = Comparers.Length;
		}

		/// <summary>
		/// Compares two elements.
		/// </summary>
		/// <param name="x">First element</param>
		/// <param name="y">Second element</param>
		/// <returns>Ordinal difference between elements.</returns>
		public int Compare(IElement x, IElement y)
		{
			int i, j;

			for (i = 0; i < c; i++)
			{
				j = this.comparers[i].Compare(x, y);
				if (j != 0)
					return j;
			}

			return 0;
		}
	}
}
