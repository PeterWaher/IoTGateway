using System;
using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Orders
{
	/// <summary>
	/// Orders entries from oldest to newest.
	/// </summary>
	public class OldestOrder : IComparer<LinkedList<TokenReference>>
	{
		/// <summary>
		/// Orders entries from oldest to newest.
		/// </summary>
		public OldestOrder()
		{
		}

		/// <summary>
		/// <see cref="IComparer<string>.Compare"/>
		/// </summary>
		public int Compare(LinkedList<TokenReference> x, LinkedList<TokenReference> y)
		{
			DateTime TimestampX = x.First?.Value?.Timestamp ?? DateTime.MinValue;
			DateTime TimestampY = y.First?.Value?.Timestamp ?? DateTime.MinValue;

			return TimestampX.CompareTo(TimestampY);
		}


	}
}
