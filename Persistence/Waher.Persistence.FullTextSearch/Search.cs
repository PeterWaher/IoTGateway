using System;
using System.Threading.Tasks;
using Waher.Events;

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

		/// <summary>
		/// Performs a Full-Text-Search
		/// </summary>
		/// <param name="IndexCollection">Index collection name.</param>
		/// <param name="Offset">Index of first object matching the keywords.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Keywords">Keywords to search for.</param>
		/// <returns>Array of objects</returns>
		public static Task<T[]> FullTextSearch<T>(string IndexCollection, 
			int Offset, int MaxCount, FullTextSearchOrder Order, params string[] Keywords)
			where T : class
		{
			return FullTextSearchModule.FullTextSearch<T>(IndexCollection, Offset, MaxCount, Order, Keywords);
		}
	}
}
