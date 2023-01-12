using System;
using System.Text;

namespace Waher.Persistence.FullTextSearch.Orders
{
	/// <summary>
	/// Contains matching information about a document in a search.
	/// </summary>
	public class MatchInformation
	{
		private DateTime timestamp = DateTime.MinValue;
		private uint nrDistinctTokens = 0;
		private ulong nrTokens = 0;
		private ulong objectReference = 0;
		private bool first = true;

		/// <summary>
		/// Contains matching information about a document in a search.
		/// </summary>
		public MatchInformation()
		{
		}

		/// <summary>
		/// Number of distinct tokens found.
		/// </summary>
		public uint NrDistinctTokens => this.nrDistinctTokens;

		/// <summary>
		/// Number of tokens found.
		/// </summary>
		public ulong NrTokens => this.nrTokens;

		/// <summary>
		/// Timestamp
		/// </summary>
		public DateTime Timestamp => this.timestamp;

		/// <summary>
		/// Object reference.
		/// </summary>
		public ulong ObjectReference => this.objectReference;

		/// <summary>
		/// Adds a token reference.
		/// </summary>
		/// <param name="Reference">Token reference.</param>
		public void AddTokenReference(TokenReference Reference)
		{
			if (this.first)
			{
				this.first = false;
				this.timestamp = Reference.Timestamp;
				this.objectReference = Reference.ObjectReference;
			}

			//this.tokenReferences.AddLast(Reference);
			this.nrDistinctTokens++;
			this.nrTokens += Reference.Count;
		}

		/// <see cref="object.ToString()"/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Distinct: ");
			sb.Append(this.NrDistinctTokens.ToString());
			sb.Append(", Total: ");
			sb.Append(this.NrTokens.ToString());
			sb.Append(", Timestamp: ");
			sb.Append(this.Timestamp.ToString());

			return sb.ToString();
		}
	}
}
