using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Emoji;

namespace Waher.Content.Markdown
{
	/// <summary>
	/// Contains settings that the Markdown parser uses to customize its behavior.
	/// </summary>
	public class MarkdownSettings
	{
		private IEmojiSource emojiSource;
		private bool parseMetaData;

		/// <summary>
		/// Contains settings that the Markdown parser uses to customize its behavior.
		/// </summary>
		public MarkdownSettings()
			: this(null, true)
		{
		}

		/// <summary>
		/// Contains settings that the Markdown parser uses to customize its behavior.
		/// </summary>
		/// <param name="EmojiSource">Optional Emoji source. Emojis and smileys are only available if an emoji source is provided.</param>
		public MarkdownSettings(IEmojiSource EmojiSource)
			: this(EmojiSource, false)
		{
		}

		/// <summary>
		/// Contains settings that the Markdown parser uses to customize its behavior.
		/// </summary>
		/// <param name="EmojiSource">Optional Emoji source. Emojis and smileys are only available if an emoji source is provided.</param>
		/// <param name="ParseMetaData">If meta-data should be parsed or not. By default, this value is true, if no emoji source is provided, 
		/// and false, if an emoji source is not provided.</param>
		public MarkdownSettings(IEmojiSource EmojiSource, bool ParseMetaData)
		{
			this.emojiSource = EmojiSource;
			this.parseMetaData = ParseMetaData;
		}

		/// <summary>
		/// Optional Emoji source. Emojis and smileys are only available if an emoji source is provided.
		/// </summary>
		public IEmojiSource EmojiSource
		{
			get { return this.emojiSource; }
			set { this.emojiSource = value; } 
		}

		/// <summary>
		/// If meta-data should be parsed or not.
		/// </summary>
		public bool ParseMetaData
		{
			get { return this.parseMetaData; }
			set { this.parseMetaData = value; }
		}
	}
}
