using Waher.Content.Binary;
using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Identity.UserInput
{
	/// <summary>
	/// Class containing user input parameters for a environment photo attachment to be 
	/// added.
	/// </summary>
	internal class EnvironmentPhotoInput : PhotoInput
	{
		/// <summary>
		/// Content of the attachment.
		/// </summary>
		[McpFileUploadParameter("Content", "Content of the attachment.", "image/*",
			CameraCapture.Environment)]
		public CustomEncoding? Content = null;

		/// <summary>
		/// Gets the content of the photo input, if any.
		/// </summary>
		/// <returns>Custom encoding of photo.</returns>
		public override CustomEncoding? GetContent() => this.Content;
	}
}
