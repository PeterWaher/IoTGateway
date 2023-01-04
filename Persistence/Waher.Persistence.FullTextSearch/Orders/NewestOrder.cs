using System;
using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Orders
{
	/// <summary>
	/// Orders entries from newest to oldest.
	/// </summary>
	public class NewestOrder : IComparer<LinkedList<TokenReference>>
	{
		/// <summary>
		/// Orders entries from newest to oldest.
		/// </summary>
		public NewestOrder()
		{
		}

		/// <summary>
		/// <see cref="IComparer<string>.Compare"/>
		/// </summary>
		public int Compare(LinkedList<TokenReference> x, LinkedList<TokenReference> y)
		{
			DateTime TimestampX = x.First?.Value?.Timestamp ?? DateTime.MinValue;
			DateTime TimestampY = y.First?.Value?.Timestamp ?? DateTime.MinValue;

			return TimestampY.CompareTo(TimestampX);
		}


	}
}
