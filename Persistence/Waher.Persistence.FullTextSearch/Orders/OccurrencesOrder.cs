using System;
using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Orders
{
	/// <summary>
	/// Orders entries based on occurrences of keywords.
	/// </summary>
	public class OccurrencesOrder : IComparer<MatchInformation>
	{
		/// <summary>
		/// Orders entries based on occurrences of keywords.
		/// </summary>
		public OccurrencesOrder()
		{
		}

		/// <summary>
		/// <see cref="IComparer{MatchInformation}.Compare"/>
		/// </summary>
		public int Compare(MatchInformation x, MatchInformation y)
		{
			long l = (long)(y.NrTokens - x.NrTokens);

			if (l < 0)
				return -1;
			else if (l > 0)
				return 1;

			int i = (int)(y.NrDistinctTokens - x.NrDistinctTokens);
			if (i != 0)
				return i;

			return y.Timestamp.CompareTo(x.Timestamp);
		}


	}
}
