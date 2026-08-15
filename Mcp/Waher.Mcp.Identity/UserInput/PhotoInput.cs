using Waher.Content.Binary;

namespace Waher.Mcp.Identity.UserInput
{
	/// <summary>
	/// Abstract base class for user input of photos.
	/// </summary>
	internal abstract class PhotoInput
	{
		/// <summary>
		/// Gets the content of the photo input, if any.
		/// </summary>
		/// <returns>Custom encoding of photo.</returns>
		public abstract CustomEncoding? GetContent();
	}
}
