using System;
using System.IO;
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
		private string localHttpxResourcePath = null;
		private string rootFolder = null;
		private bool audioAutoplay = true;
		private bool audioControls = false;
		private bool videoAutoplay = false;
		private bool videoControls = true;
		private bool embedEmojis = false;
		private bool allowScriptTag = true;

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
		/// Modifies URLS using the HTTPX URI scheme, so that they point to an HTTPX proxy. The string %URL% is replaced with
		/// the original URL string.
		/// </summary>
		public string HttpxProxy
		{
			get { return this.httpxProxy; }
			set { this.httpxProxy = value; }
		}

		/// <summary>
		/// Local HTTPX Resource Path. HTTPX URLs starting with this string (if defined) will be considered local web resources.
		/// </summary>
		public string LocalHttpxResourcePath
		{
			get { return this.localHttpxResourcePath; }
			set { this.localHttpxResourcePath = value; }
		}

		/// <summary>
		/// If audio is to be played automatically. Default=true.
		/// </summary>
		public bool AudioAutoplay
		{
			get { return this.audioAutoplay; }
			set { this.audioAutoplay = value; }
		}

		/// <summary>
		/// If audio should be played with controls or not. Default=false.
		/// </summary>
		public bool AudioControls
		{
			get { return this.audioControls; }
			set { this.audioControls = value; }
		}

		/// <summary>
		/// If video is to be played automatically. Default=false.
		/// </summary>
		public bool VideoAutoplay
		{
			get { return this.videoAutoplay; }
			set { this.videoAutoplay = value; }
		}

		/// <summary>
		/// If video should be played with controls or not. Default=true.
		/// </summary>
		public bool VideoControls
		{
			get { return this.videoControls; }
			set { this.videoControls = value; }
		}

		/// <summary>
		/// If emojis should be embedded using the data URI scheme.
		/// </summary>
		public bool EmbedEmojis
		{
			get { return this.embedEmojis; }
			set { this.embedEmojis = value; }
		}

		/// <summary>
		/// File system root folder. If file references are absolute, and this property is provided, they are measured relative to this folder.
		/// </summary>
		public string RootFolder
		{
			get { return this.rootFolder; }
			set { this.rootFolder = value; }
		}

		/// <summary>
		/// If the HTML SCRIPT tag should be allowed or not.s
		/// </summary>
		public bool AllowScriptTag
		{
			get { return this.allowScriptTag; }
			set { this.allowScriptTag = value; }
		}

		/// <summary>
		/// Evaluates a file name from a file reference.
		/// </summary>
		/// <param name="DocumentFileName">Filename of original markdown document.</param>
		/// <param name="FileNameReference">Filename reference.</param>
		/// <returns>Physical filename.</returns>
		public string GetFileName(string DocumentFileName, string FileNameReference)
		{
			char ch;

			if (!string.IsNullOrEmpty(FileNameReference) && 
				((ch = FileNameReference[0]) == Path.DirectorySeparatorChar || ch == '/') &&
				!string.IsNullOrEmpty(this.rootFolder))
			{
				return Path.Combine(this.rootFolder, FileNameReference.Substring(1));
			}
			else
				return Path.Combine(Path.GetDirectoryName(DocumentFileName), FileNameReference);
		}

	}
}
