using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Orders
{
	/// <summary>
	/// Orders entries from oldest to newest.
	/// </summary>
	public class OldestOrder : IComparer<MatchInformation>
	{
		/// <summary>
		/// Orders entries from oldest to newest.
		/// </summary>
		public OldestOrder()
		{
		}

		/// <summary>
		/// <see cref="IComparer{MatchInformation}.Compare"/>
		/// </summary>
		public int Compare(MatchInformation x, MatchInformation y)
		{
			return x.Timestamp.CompareTo(y.Timestamp);
		}


	}
}
