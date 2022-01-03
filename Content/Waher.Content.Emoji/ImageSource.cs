using System;

namespace Waher.Content.Emoji
{
	/// <summary>
	/// Contains information about an emoji image.
	/// </summary>
	public class ImageSource : IImageSource
	{
		/// <summary>
		/// URL of image
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Width of image, if available.
		/// </summary>
		public int? Width { get; set; }

		/// <summary>
		/// Height of image, if available.
		/// </summary>
		public int? Height { get; set; }
	}
}
