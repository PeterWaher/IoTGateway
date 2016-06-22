using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Emoji;
using Waher.Script;

namespace Waher.Content.Markdown
{
    /// <summary>
    /// Contains settings that the Markdown parser uses to customize its behavior.
    /// </summary>
    public class MarkdownSettings
    {
        private IEmojiSource emojiSource;
        private Variables variables;
        private bool parseMetaData;
		private string httpxProxy = null;

        /// <summary>
        /// Contains settings that the Markdown parser uses to customize its behavior.
        /// </summary>
        public MarkdownSettings()
            : this(null, true, null)
        {
        }

        /// <summary>
        /// Contains settings that the Markdown parser uses to customize its behavior.
        /// </summary>
        /// <param name="EmojiSource">Optional Emoji source. Emojis and smileys are only available if an emoji source is provided.</param>
        public MarkdownSettings(IEmojiSource EmojiSource)
            : this(EmojiSource, false, null)
        {
        }

        /// <summary>
        /// Contains settings that the Markdown parser uses to customize its behavior.
        /// </summary>
        /// <param name="EmojiSource">Optional Emoji source. Emojis and smileys are only available if an emoji source is provided.</param>
        /// <param name="ParseMetaData">If meta-data should be parsed or not. By default, this value is true, if no emoji source is provided, 
        /// and false, if an emoji source is not provided.</param>
        public MarkdownSettings(IEmojiSource EmojiSource, bool ParseMetaData)
            : this(EmojiSource, ParseMetaData, null)
        {
        }

        /// <summary>
        /// Contains settings that the Markdown parser uses to customize its behavior.
        /// </summary>
        /// <param name="EmojiSource">Optional Emoji source. Emojis and smileys are only available if an emoji source is provided.</param>
        /// <param name="ParseMetaData">If meta-data should be parsed or not. By default, this value is true, if no emoji source is provided, 
        /// and false, if an emoji source is not provided.</param>
        /// <param name="Variables">Collection of variables. Providing such a collection enables script execution inside markdown
        /// documents.</param>
        public MarkdownSettings(IEmojiSource EmojiSource, bool ParseMetaData, Variables Variables)
        {
            this.emojiSource = EmojiSource;
            this.parseMetaData = ParseMetaData;
            this.variables = Variables;
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

        /// <summary>
        /// Collection of variables. Providing such a collection enables script execution inside markdown documents.
        /// </summary>
        public Variables Variables
        {
            get { return this.variables; }
            set { this.variables = value; }
        }

		/// <summary>
		/// Modifies URLS using the HTTPX URI scheme, so that they point to a HTTPX proxy. The string %URL% is replaced with
		/// the original URL string.
		/// </summary>
		public string HttpxProxy
		{
			get { return this.httpxProxy; }
			set { this.httpxProxy = value; }
		}
    }
}
