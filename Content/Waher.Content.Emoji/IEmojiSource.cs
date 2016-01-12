using System;
using System.Collections.Generic;
using System.Text;

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
		void GenerateHTML(StringBuilder Output, EmojiInfo Emoji);
	}
}
