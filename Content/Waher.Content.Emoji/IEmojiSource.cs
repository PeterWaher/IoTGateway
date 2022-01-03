using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Emoji
{
	/// <summary>
	/// Interface for Emoji sources. Emoji sources provide emojis to content providers.
	/// </summary>
	public interface IEmojiSource
	{
		/// <summary>
		/// If the emoji is supported by the emoji source.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		/// <returns>If emoji is supported.</returns>
		bool EmojiSupported(EmojiInfo Emoji);

		/// <summary>
		/// Generates HTML for a given Emoji.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Emoji">Emoji</param>
		/// <param name="EmbedImage">If image should be embedded into the generated HTML, using the data URI scheme.</param>
		Task GenerateHTML(StringBuilder Output, EmojiInfo Emoji, bool EmbedImage);

		/// <summary>
		/// Generates HTML for a given Emoji.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Emoji">Emoji</param>
		/// <param name="Level">Level (number of colons used to define the emoji)</param>
		/// <param name="EmbedImage">If image should be embedded into the generated HTML, using the data URI scheme.</param>
		Task GenerateHTML(StringBuilder Output, EmojiInfo Emoji, int Level, bool EmbedImage);

		/// <summary>
		/// Gets the image source of an emoji.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		Task<IImageSource> GetImageSource(EmojiInfo Emoji);

		/// <summary>
		/// Gets the image source of an emoji.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		/// <param name="Level">Level (number of colons used to define the emoji)</param>
		Task<IImageSource> GetImageSource(EmojiInfo Emoji, int Level);
	}
}
