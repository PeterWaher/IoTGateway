using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Keywords
{
	/// <summary>
	/// Contains information about a search process.
	/// </summary>
	public class SearchProcess
	{
		/// <summary>
		/// Contains information about a search process.
		/// </summary>
		/// <param name="Index">Index dictionary</param>
		public SearchProcess(IPersistentDictionary Index)
		{
			this.Index = Index;
			this.ReferencesByObject = new Dictionary<ulong, LinkedList<TokenReference>>();
			this.IsRestricted = false;
		}

		/// <summary>
		/// Index dictionary
		/// </summary>
		public IPersistentDictionary Index { get; }

		/// <summary>
		/// References found.
		/// </summary>
		public Dictionary<ulong, LinkedList<TokenReference>> ReferencesByObject { get; }

		/// <summary>
		/// If the search process is restricted.
		/// </summary>
		public bool IsRestricted 
		{ 
			get;
			private set;
		}

		/// <summary>
		/// Objects found during the processing of the current keyword.
		/// </summary>
		public Dictionary<ulong, bool> Found
		{
			get;
			private set;
		}

		/// <summary>
		/// Increments counter of number of restricted keywords.
		/// </summary>
		public void IncRestricted()
		{
			if (!this.IsRestricted)
			{
				this.IsRestricted = true;
				this.Found = new Dictionary<ulong, bool>();
			}
		}
	}
}
