namespace Waher.Networking.HTTP.Interfaces
{
	/// <summary>
	/// Interface for resources hosting files in a folder.
	/// </summary>
	public interface IFolderResource
	{
		/// <summary>
		/// Tries to get the full path of a file-based resource.
		/// </summary>
		/// <param name="SubPath">Sub-path idendifying the local resource</param>
		/// <param name="Host">Host name used in request.</param>
		/// <param name="MustExist">If file must exist.</param>
		/// <param name="FileName">File name, if found.</param>
		/// <returns>If the resource points to a file.</returns>
		bool TryGetFileName(string SubPath, string Host, bool MustExist, out string FileName);
	}
}
