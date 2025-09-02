using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Emoji
{
	/// <summary>
	/// An emoji source that does not support any emojis. Use this emoji source if you
	/// explicitly want to avoid using emojis.
	/// </summary>
	public class NoEmojis : IEmojiSource
	{
		/// <summary>
		/// An emoji source that does not support any emojis.
		/// </summary>
		public NoEmojis()
		{
		}

		/// <summary>
		/// If the emoji is supported by the emoji source.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		/// <returns>If emoji is supported.</returns>
		public bool EmojiSupported(EmojiInfo Emoji) => false;

		/// <summary>
		/// Generates HTML for a given Emoji.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Emoji">Emoji</param>
		/// <param name="EmbedImage">If image should be embedded into the generated HTML, using the data URI scheme.</param>
		public Task GenerateHTML(StringBuilder Output, EmojiInfo Emoji, bool EmbedImage) => Task.CompletedTask;

		/// <summary>
		/// Generates HTML for a given Emoji.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Emoji">Emoji</param>
		/// <param name="Level">Level (number of colons used to define the emoji)</param>
		/// <param name="EmbedImage">If image should be embedded into the generated HTML, using the data URI scheme.</param>
		public Task GenerateHTML(StringBuilder Output, EmojiInfo Emoji, int Level, bool EmbedImage) => Task.CompletedTask;

		/// <summary>
		/// Gets the image source of an emoji.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		public Task<IImageSource> GetImageSource(EmojiInfo Emoji) => Task.FromResult<IImageSource>(null);

		/// <summary>
		/// Gets the image source of an emoji.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		/// <param name="Level">Level (number of colons used to define the emoji)</param>
		public Task<IImageSource> GetImageSource(EmojiInfo Emoji, int Level) => Task.FromResult<IImageSource>(null);
	}
}
