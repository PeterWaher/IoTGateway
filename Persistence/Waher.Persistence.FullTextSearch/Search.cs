using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.FullTextSearch.Keywords;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Order in which results are returned.
	/// </summary>
	public enum	FullTextSearchOrder
	{
		/// <summary>
		/// Relevant to keywords used.
		/// </summary>
		Relevance,

		/// <summary>
		/// Occurrences of keyworkds.
		/// </summary>
		Occurrences,

		/// <summary>
		/// From newest to oldest
		/// </summary>
		Newest,

		/// <summary>
		/// From oldest to newest
		/// </summary>
		Oldest
	}

	/// <summary>
	/// How pagination in full-text-searches should be handled.
	/// </summary>
	public enum PaginationStrategy
	{
		/// <summary>
		/// Pagination is done over objects found in search. Incompatible types
		/// are returned as null. Makes pagination quicker, as objects do not need
		/// to be preloaded, and can be skipped quicker.
		/// </summary>
		PaginateOverObjectsNullIfIncompatible,

		/// <summary>
		/// Pagination is done over objects found in search. Only compatible
		/// objects are returned. Amount of objects returned might be less than
		/// number of objects found, making evaluation of next offset in paginated
		/// search difficult.
		/// </summary>
		PaginateOverObjectsOnlyCompatible,

		/// <summary>
		/// Pagination is done over compatible objects found in search. Pagination
		/// becomes more resource intensive, as all objects need to be loaded to be
		/// checked if they are compatible or not.
		/// </summary>
		PaginationOverCompatibleOnly
	}

	/// <summary>
	/// Static class for access to Full-Text-Search
	/// </summary>
	public static class Search
	{
		internal static async Task RaiseObjectAddedToIndex(object Sender, ObjectReferenceEventArgs e)
		{
			try
			{
				ObjectReferenceEventHandler h = ObjectAddedToIndex;
				if (!(h is null))
					await h(Sender, e);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when a new object instance has been indexed in the
		/// full-text-search index.
		/// </summary>
		public static event ObjectReferenceEventHandler ObjectAddedToIndex;

		internal static async Task RaiseObjectRemovedFromIndex(object Sender, ObjectReferenceEventArgs e)
		{
			try
			{
				ObjectReferenceEventHandler h = ObjectRemovedFromIndex;
				if (!(h is null))
					await h(Sender, e);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when an object instance has been removed from the
		/// full-text-search index.
		/// </summary>
		public static event ObjectReferenceEventHandler ObjectRemovedFromIndex;

		internal static async Task RaiseObjectUpdatedInIndex(object Sender, ObjectReferenceEventArgs e)
		{
			try
			{
				ObjectReferenceEventHandler h = ObjectUpdatedInIndex;
				if (!(h is null))
					await h(Sender, e);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when an object instance has been updated in the
		/// full-text-search index.
		/// </summary>
		public static event ObjectReferenceEventHandler ObjectUpdatedInIndex;

		/// <summary>
		/// Parses a search string into keyworkds.
		/// </summary>
		/// <param name="Search">Search string.</param>
		/// <returns>Keywords</returns>
		public static Keyword[] ParseKeywords(string Search)
		{
			return ParseKeywords(Search, false);
		}

		/// <summary>
		/// Parses a search string into keyworkds.
		/// </summary>
		/// <param name="Search">Search string.</param>
		/// <param name="TreatKeywordsAsPrefixes">If keywords should be treated as
		/// prefixes. Example: "test" would match "test", "tests" and "testing" if
		/// treated as a prefix, but also "tester", "testosterone", etc.
		/// Default is false.</param>
		/// <returns>Keywords</returns>
		public static Keyword[] ParseKeywords(string Search, bool TreatKeywordsAsPrefixes)
		{
			return FullTextSearchModule.ParseKeywords(Search, TreatKeywordsAsPrefixes);
		}

		/// <summary>
		/// Performs a Full-Text-Search
		/// </summary>
		/// <param name="IndexCollection">Index collection name.</param>
		/// <param name="Offset">Index of first object matching the keywords.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Keywords">Keywords to search for.</param>
		/// <returns>Array of objects. Depending on choice of
		/// <paramref name="PaginationStrategy"/>, null items may be returned
		/// if underlying object is not compatible with <typeparamref name="T"/>.</returns>
		public static Task<T[]> FullTextSearch<T>(string IndexCollection, 
			int Offset, int MaxCount, FullTextSearchOrder Order, params Keyword[] Keywords)
			where T : class
		{
			return FullTextSearchModule.FullTextSearch<T>(IndexCollection, Offset, MaxCount,
				Order, PaginationStrategy.PaginateOverObjectsNullIfIncompatible, Keywords);
		}
	}
}
