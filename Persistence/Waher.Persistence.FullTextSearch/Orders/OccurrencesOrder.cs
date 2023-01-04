using System;
using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Orders
{
	/// <summary>
	/// Orders entries based on occurrences of keywords.
	/// </summary>
	public class OccurrencesOrder : IComparer<LinkedList<TokenReference>>
	{
		/// <summary>
		/// Orders entries based on occurrences of keywords.
		/// </summary>
		public OccurrencesOrder()
		{
		}

		/// <summary>
		/// <see cref="IComparer<string>.Compare"/>
		/// </summary>
		public int Compare(LinkedList<TokenReference> x, LinkedList<TokenReference> y)
		{
			int CountX = 0;
			int CountY = 0;
			ulong NrTokensX = 0;
			ulong NrTokensY = 0;

			foreach (TokenReference Ref in x)
			{
				CountX++;
				NrTokensX += Ref.Count;
			}

			foreach (TokenReference Ref in y)
			{
				CountY++;
				NrTokensY += Ref.Count;
			}

			long l = (long)(NrTokensY - NrTokensX);

			if (l < 0)
				return -1;
			else if (l > 0)
				return 1;

			int i = CountY - CountX;
			if (i != 0)
				return i;

			DateTime TimestampX = x.First?.Value?.Timestamp ?? DateTime.MinValue;
			DateTime TimestampY = y.First?.Value?.Timestamp ?? DateTime.MinValue;

			return TimestampY.CompareTo(TimestampX);
		}


	}
}
