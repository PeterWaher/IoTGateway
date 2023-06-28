using System.Collections.Generic;
using Waher.Content.Semantic;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Comparer for ordering a SPARQL result set.
	/// </summary>
	public class OrderResultSet : IComparer<ISparqlResultRecord>
	{
		private readonly KeyValuePair<string, bool>[] orderBy;
		private readonly int count;

		/// <summary>
		/// Comparer for ordering a SPARQL result set.
		/// </summary>
		/// <param name="OrderBy">Order by-statement, containing a vector
		/// of variable names, and corresponding ascending (true) or
		/// descending (false) direction.</param>
		public OrderResultSet(KeyValuePair<string, bool>[] OrderBy)
		{
			this.orderBy = OrderBy;
			this.count = OrderBy?.Length ?? 0;
		}

		/// <summary>
		/// Compares two records.
		/// </summary>
		/// <param name="x">Record 1</param>
		/// <param name="y">Record 2</param>
		/// <returns>
		/// Negative, if Record 1 is less than Record 2
		/// Zero, if records are equal
		/// Positive, if Record 1 is greated than Record 2
		/// </returns>
		public int Compare(ISparqlResultRecord x, ISparqlResultRecord y)
		{
			ISemanticElement e1, e2;
			int i, j;

			for (i = 0; i < this.count; i++)
			{
				e1 = x[this.orderBy[i].Key];
				e2 = y[this.orderBy[i].Key];

				j = e1.CompareTo(e2);
				if (j != 0)
					return this.orderBy[i].Value ? j : -j;
			}

			return 0;
		}
	}
}
