using System;

namespace Waher.Content
{
	/// <summary>
	/// Base interface for Internet content encoders or decoders.
	/// </summary>
	public interface IInternetContent
	{
		/// <summary>
		/// Supported content types.
		/// </summary>
		string[] ContentTypes
		{
			get;
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		string[] FileExtensions
		{
			get;
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		bool TryGetContentType(string FileExtension, out string ContentType);

		/// <summary>
		/// Tries to get the file extension of an item, given its Content-Type.
		/// </summary>
		/// <param name="ContentType">Content type.</param>
		/// <param name="FileExtension">File extension.</param>
		/// <returns>If the Content-Type was recognized.</returns>
		bool TryGetFileExtension(string ContentType, out string FileExtension);
	}
}
