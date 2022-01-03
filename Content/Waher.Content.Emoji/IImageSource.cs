using System;

namespace Waher.Content.Emoji
{
	/// <summary>
	/// Contains information about an emoji image.
	/// </summary>
	public interface IImageSource
	{
		/// <summary>
		/// URL of image
		/// </summary>
		string Url { get; }

		/// <summary>
		/// Width of image, if available.
		/// </summary>
		int? Width { get; }

		/// <summary>
		/// Height of image, if available.
		/// </summary>
		int? Height { get; }
	}
}
