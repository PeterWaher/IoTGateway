using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Filters;
using Waher.Persistence.FullTextSearch.Orders;

namespace Waher.Persistence.FullTextSearch.Keywords
{
	/// <summary>
	/// Contains information about a search process.
	/// </summary>
	public class SearchProcess
	{
		private readonly Dictionary<ulong, ObjectReference> references = new Dictionary<ulong, ObjectReference>();
		private readonly string indexCollection;

		/// <summary>
		/// Contains information about a search process.
		/// </summary>
		/// <param name="Index">Index dictionary</param>
		/// <param name="IndexCollection">Index collection name</param>
		public SearchProcess(IPersistentDictionary Index, string IndexCollection)
		{
			this.Index = Index;
			this.indexCollection = IndexCollection;
			this.ReferencesByObject = new Dictionary<ulong, MatchInformation>();
			this.IsRestricted = false;
		}

		/// <summary>
		/// Index dictionary
		/// </summary>
		public IPersistentDictionary Index { get; }

		/// <summary>
		/// References found.
		/// </summary>
		public Dictionary<ulong, MatchInformation> ReferencesByObject { get; }

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

		/// <summary>
		/// Tries to get an object reference.
		/// </summary>
		/// <param name="ObjectIndex">Object index.</param>
		/// <param name="CanLoadFromDatabase">If the object can be loaded
		/// from the database if not found in the cache.</param>
		/// <returns>Object reference, if found, null otherwise.</returns>
		public async Task<ObjectReference> TryGetObjectReference(ulong ObjectIndex, bool CanLoadFromDatabase)
		{
			if (this.references.TryGetValue(ObjectIndex, out ObjectReference Ref))
				return Ref;

			if (CanLoadFromDatabase)
			{
				Ref = await Database.FindFirstIgnoreRest<ObjectReference>(new FilterAnd(
					new FilterFieldEqualTo("IndexCollection", this.indexCollection),
					new FilterFieldEqualTo("Index", ObjectIndex)));

				this.references[ObjectIndex] = Ref;
			}

			return Ref;
		}
	}
}
