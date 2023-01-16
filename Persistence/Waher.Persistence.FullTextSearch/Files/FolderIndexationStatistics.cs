namespace Waher.Persistence.FullTextSearch.Files
{
	/// <summary>
	/// Contains statistics about a files folder (re)indexation procedure.
	/// </summary>
	public class FolderIndexationStatistics
	{
		/// <summary>
		/// Number of files added to the index.
		/// </summary>
		public int NrAdded { get; internal set; } = 0;

		/// <summary>
		/// Number of files updated in the index.
		/// </summary>
		public int NrUpdated { get; internal set; } = 0;

		/// <summary>
		/// Number of files deleted from the index.
		/// </summary>
		public int NrDeleted { get; internal set; } = 0;

		/// <summary>
		/// Number of files processed.
		/// </summary>
		public int NrFiles { get; internal set; } = 0;

		/// <summary>
		/// Total number of files changed in the index.
		/// </summary>
		public int TotalChanges { get; internal set; } = 0;
	}
}
