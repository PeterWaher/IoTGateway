using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Orders
{
	/// <summary>
	/// Orders entries from newest to oldest.
	/// </summary>
	public class NewestOrder : IComparer<MatchInformation>
	{
		/// <summary>
		/// Orders entries from newest to oldest.
		/// </summary>
		public NewestOrder()
		{
		}

		/// <summary>
		/// <see cref="IComparer{MatchInformation}.Compare"/>
		/// </summary>
		public int Compare(MatchInformation x, MatchInformation y)
		{
			return y.Timestamp.CompareTo(x.Timestamp);
		}
	}
}
