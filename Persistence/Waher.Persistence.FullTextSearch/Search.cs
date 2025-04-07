using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.FullTextSearch.Files;
using Waher.Persistence.FullTextSearch.Keywords;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Order in which results are returned.
	/// </summary>
	public enum FullTextSearchOrder
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
		internal static Task RaiseObjectAddedToIndex(object Sender, ObjectReferenceEventArgs e)
		{
			return ObjectAddedToIndex.Raise(Sender, e);
		}

		/// <summary>
		/// Event raised when a new object instance has been indexed in the
		/// full-text-search index.
		/// </summary>
		public static event EventHandlerAsync<ObjectReferenceEventArgs> ObjectAddedToIndex;

		internal static Task RaiseObjectRemovedFromIndex(object Sender, ObjectReferenceEventArgs e)
		{
			return ObjectRemovedFromIndex.Raise(Sender, e);
		}

		/// <summary>
		/// Event raised when an object instance has been removed from the
		/// full-text-search index.
		/// </summary>
		public static event EventHandlerAsync<ObjectReferenceEventArgs> ObjectRemovedFromIndex;

		internal static Task RaiseObjectUpdatedInIndex(object Sender, ObjectReferenceEventArgs e)
		{
			return ObjectUpdatedInIndex.Raise(Sender, e);
		}

		/// <summary>
		/// Event raised when an object instance has been updated in the
		/// full-text-search index.
		/// </summary>
		public static event EventHandlerAsync<ObjectReferenceEventArgs> ObjectUpdatedInIndex;

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
		/// <param name="Order">Sort order of result set.</param>
		/// <returns>Array of objects. Noncompatible (with <typeparamref name="T"/>) items are returned as null 
		/// in the array..</returns>
		public static Task<T[]> FullTextSearch<T>(string IndexCollection,
			int Offset, int MaxCount, FullTextSearchOrder Order, params Keyword[] Keywords)
			where T : class
		{
			return FullTextSearch<T>(IndexCollection, Offset, MaxCount, Order,
				PaginationStrategy.PaginateOverObjectsNullIfIncompatible, Keywords);
		}

		/// <summary>
		/// Performs a Full-Text-Search
		/// </summary>
		/// <param name="IndexCollection">Index collection name.</param>
		/// <param name="Offset">Index of first object matching the keywords.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Keywords">Keywords to search for.</param>
		/// <param name="Order">Sort order of result set.</param>
		/// <param name="PaginationStrategy">How to handle noncompatible items.</param>
		/// <returns>Array of objects. Depending on choice of
		/// <paramref name="PaginationStrategy"/>, null items may be returned
		/// if underlying object is not compatible with <typeparamref name="T"/>.</returns>
		public static Task<T[]> FullTextSearch<T>(string IndexCollection,
			int Offset, int MaxCount, FullTextSearchOrder Order,
			PaginationStrategy PaginationStrategy, params Keyword[] Keywords)
			where T : class
		{
			return FullTextSearchModule.FullTextSearch<T>(IndexCollection, Offset, MaxCount,
				Order, PaginationStrategy, Keywords);
		}

		/// <summary>
		/// Registers stop-words with the search-engine.
		/// Stop-words are ignored in searches.
		/// </summary>
		/// <param name="StopWords">Stop words.</param>
		public static void RegisterStopWords(params string[] StopWords)
		{
			FullTextSearchModule.RegisterStopWords(StopWords);
		}

		/// <summary>
		/// Checks if a word is a stop word.
		/// </summary>
		/// <param name="StopWord">Word to check.</param>
		/// <returns>If word is a stop word.</returns>
		public static bool IsStopWord(string StopWord)
		{
			return FullTextSearchModule.IsStopWord(StopWord);
		}

		/// <summary>
		/// Tokenizes a set of objects using available tokenizers.
		/// Tokenizers are classes with a default contructor, implementing
		/// the <see cref="ITokenizer"/> interface.
		/// </summary>
		/// <param name="Objects">Objects to tokenize.</param>
		/// <returns>Tokens</returns>
		public static Task<TokenCount[]> Tokenize(IEnumerable<object> Objects)
		{
			return FullTextSearchModule.Tokenize(Objects);
		}

		/// <summary>
		/// Tokenizes a set of objects using available tokenizers.
		/// Tokenizers are classes with a default contructor, implementing
		/// the <see cref="ITokenizer"/> interface.
		/// </summary>
		/// <param name="Objects">Objects to tokenize.</param>
		/// <returns>Tokens</returns>
		public static Task<TokenCount[]> Tokenize(params object[] Objects)
		{
			return FullTextSearchModule.Tokenize(Objects);
		}

		/// <summary>
		/// Defines the Full-text-search index collection name, for objects in a given collection.
		/// </summary>
		/// <param name="IndexCollection">Collection name for full-text-search index of objects in the given collection.</param>
		/// <param name="CollectionName">Collection of objects to index.</param>
		/// <returns>If the configuration was changed.</returns>
		public static Task<bool> SetFullTextSearchIndexCollection(string IndexCollection, string CollectionName)
		{
			return FullTextSearchModule.SetFullTextSearchIndexCollection(IndexCollection, CollectionName);
		}

		/// <summary>
		/// Adds properties for full-text-search indexation.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <param name="Properties">Properties to index.</param>
		/// <returns>If new property names were found and added.</returns>
		public static Task<bool> AddFullTextSearch(string CollectionName, params PropertyDefinition[] Properties)
		{
			return FullTextSearchModule.AddFullTextSearch(CollectionName, Properties);
		}

		/// <summary>
		/// Removes properties from full-text-search indexation.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <param name="Properties">Properties to remove from indexation.</param>
		/// <returns>If property names were found and removed.</returns>
		public static Task<bool> RemoveFullTextSearch(string CollectionName, params PropertyDefinition[] Properties)
		{
			return FullTextSearchModule.RemoveFullTextSearch(CollectionName, Properties);
		}

		/// <summary>
		/// Gets indexed properties for full-text-search indexation.
		/// </summary>
		/// <returns>Dictionary of indexed properties, per collection.</returns>
		public static Task<Dictionary<string, PropertyDefinition[]>> GetFullTextSearchIndexedProperties()
		{
			return FullTextSearchModule.GetFullTextSearchIndexedProperties();
		}

		/// <summary>
		/// Gets indexed properties for full-text-search indexation.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <returns>Array of indexed properties.</returns>
		public static Task<PropertyDefinition[]> GetFullTextSearchIndexedProperties(string CollectionName)
		{
			return FullTextSearchModule.GetFullTextSearchIndexedProperties(CollectionName);
		}

		/// <summary>
		/// Gets the database collections that get indexed into a given index colltion.
		/// </summary>
		/// <returns>Collection Names indexed in the full-text-search index by index collection.</returns>
		public static Task<Dictionary<string, string[]>> GetCollectionNames()
		{
			return FullTextSearchModule.GetCollectionNames();
		}

		/// <summary>
		/// Gets the database collections that get indexed into a given index colltion.
		/// </summary>
		/// <param name="IndexCollectionName">Index Collection Name</param>
		/// <returns>Collection Names indexed in the full-text-search index
		/// defined by <paramref name="IndexCollectionName"/>.</returns>
		public static Task<string[]> GetCollectionNames(string IndexCollectionName)
		{
			return FullTextSearchModule.GetCollectionNames(IndexCollectionName);
		}

		/// <summary>
		/// Reindexes the full-text-search index for a database collection.
		/// </summary>
		/// <param name="IndexCollectionName">Index Collection</param>
		/// <returns>Number of objects reindexed.</returns>
		public static Task<long> ReindexCollection(string IndexCollectionName)
		{
			return FullTextSearchModule.ReindexCollection(IndexCollectionName);
		}

		/// <summary>
		/// Indexes or reindexes files in a folder.
		/// </summary>
		/// <param name="IndexCollection">Name of index collection.</param>
		/// <param name="Folder">Folder name.</param>
		/// <param name="Recursive">If processing of files in subfolders should be performed.</param>
		/// <param name="ExcludeSubfolders">Any subfolders to exclude (in recursive mode).</param>
		/// <returns>Statistics about indexation process.</returns>
		public static Task<FolderIndexationStatistics> IndexFolder(string IndexCollection, string Folder, bool Recursive,
			params string[] ExcludeSubfolders)
		{
			return FullTextSearchModule.IndexFolder(IndexCollection, Folder, Recursive, ExcludeSubfolders);
		}

		/// <summary>
		/// Indexes or reindexes a file.
		/// </summary>
		/// <param name="IndexCollection">Name of index collection.</param>
		/// <param name="FileName">File name.</param>
		/// <returns>If index was updated.</returns>
		public static Task<bool> IndexFile(string IndexCollection, string FileName)
		{
			return FullTextSearchModule.IndexFile(IndexCollection, FileName);
		}
	}
}
