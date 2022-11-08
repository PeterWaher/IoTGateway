namespace Waher.Content
{
	/// <summary>
	/// Basic interface for Resource maps.
	/// </summary>
	public interface IResourceMap
	{
		/// <summary>
		/// Checks if a resource name needs to be mapped to an alternative resource.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		/// <returns>If resource is mapped, and has been updated to reflect the true resource name.</returns>
		bool CheckResource(ref string ResourceName);

		/// <summary>
		/// Tries to get the full path of a file-based resource.
		/// </summary>
		/// <param name="LocalUrl">Local URL</param>
		/// <param name="FileName">File name, if found.</param>
		/// <returns>If the resource points to a file.</returns>
		bool TryGetFileName(string LocalUrl, out string FileName);

		/// <summary>
		/// Tries to get the full path of a file-based resource.
		/// </summary>
		/// <param name="LocalUrl">Local URL</param>
		/// <param name="MustExist">If file must exist.</param>
		/// <param name="FileName">File name, if found.</param>
		/// <returns>If the resource points to a file.</returns>
		bool TryGetFileName(string LocalUrl, bool MustExist, out string FileName);

	}
}
