using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Content.Emoji;
using Waher.Script;
using Waher.Script.Model;

namespace Waher.Content.Markdown
{
	/// <summary>
	/// Delegate for expression authorization methods.
	/// </summary>
	/// <param name="Expression">Expression to be authorized.</param>
	/// <returns>If the expression is authorized to execute, null is returned. If it is prohibited, 
	/// <see cref="ScriptNode"/> that is prohibited is returned.</returns>
	public delegate Task<ScriptNode> AuthorizeExpression(Expression Expression);

	/// <summary>
	/// Contains settings that the Markdown parser uses to customize its behavior.
	/// </summary>
	public class MarkdownSettings
	{
		private static IEmojiSource defaultEmojiSource = null;
		private static bool defaultEmojiSourceLocked = false;

		private IEmojiSource emojiSource;
		private Variables variables;
		private AuthorizeExpression authorizeExpression;
		private IResourceMap resourceMap = null;
		private ICodecProgress progress;
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
		private bool allowHtml = true;
		private bool allowInlineScript = true;

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
			this.emojiSource = EmojiSource ?? defaultEmojiSource;
			this.parseMetaData = ParseMetaData;
			this.variables = Variables;
		}

		/// <summary>
		/// Optional Emoji source. Emojis and smileys are only available if an emoji source is provided.
		/// </summary>
		public IEmojiSource EmojiSource
		{
			get => this.emojiSource;
			set => this.emojiSource = value;
		}

		/// <summary>
		/// If meta-data should be parsed or not.
		/// </summary>
		public bool ParseMetaData
		{
			get => this.parseMetaData;
			set => this.parseMetaData = value;
		}

		/// <summary>
		/// Collection of variables. Providing such a collection enables script execution inside markdown documents.
		/// </summary>
		public Variables Variables
		{
			get => this.variables;
			set => this.variables = value;
		}

		/// <summary>
		/// Modifies URLS using the HTTPX URI scheme, so that they point to an HTTPX proxy. The string %URL% is replaced with
		/// the original URL string.
		/// </summary>
		public string HttpxProxy
		{
			get => this.httpxProxy;
			set => this.httpxProxy = value;
		}

		/// <summary>
		/// Local HTTPX Resource Path. HTTPX URLs starting with this string (if defined) will be considered local web resources.
		/// </summary>
		public string LocalHttpxResourcePath
		{
			get => this.localHttpxResourcePath;
			set => this.localHttpxResourcePath = value;
		}

		/// <summary>
		/// If audio is to be played automatically. Default=true.
		/// </summary>
		public bool AudioAutoplay
		{
			get => this.audioAutoplay;
			set => this.audioAutoplay = value;
		}

		/// <summary>
		/// If audio should be played with controls or not. Default=false.
		/// </summary>
		public bool AudioControls
		{
			get => this.audioControls;
			set => this.audioControls = value;
		}

		/// <summary>
		/// If video is to be played automatically. Default=false.
		/// </summary>
		public bool VideoAutoplay
		{
			get => this.videoAutoplay;
			set => this.videoAutoplay = value;
		}

		/// <summary>
		/// If video should be played with controls or not. Default=true.
		/// </summary>
		public bool VideoControls
		{
			get => this.videoControls;
			set => this.videoControls = value;
		}

		/// <summary>
		/// If emojis should be embedded using the data URI scheme.
		/// </summary>
		public bool EmbedEmojis
		{
			get => this.embedEmojis;
			set => this.embedEmojis = value;
		}

		/// <summary>
		/// File system root folder. If file references are absolute, and this property is provided, they are measured relative to this folder.
		/// </summary>
		public string RootFolder
		{
			get => this.rootFolder;
			set => this.rootFolder = value;
		}

		/// <summary>
		/// If the HTML SCRIPT tag should be allowed or not.
		/// </summary>
		public bool AllowScriptTag
		{
			get => this.allowScriptTag;
			set => this.allowScriptTag = value;
		}

		/// <summary>
		/// If HTML tags are allowed to be embedded in the Markdown.
		/// </summary>
		public bool AllowHtml
		{
			get => this.allowHtml;
			set => this.allowHtml = value;
		}

		/// <summary>
		/// If inline script is allowed embedded in the Markdown.
		/// </summary>
		public bool AllowInlineScript
		{
			get => this.allowInlineScript;
			set => this.allowInlineScript = value;
		}

		/// <summary>
		/// Optional method to call to authorize execution of script expressions.
		/// </summary>
		public AuthorizeExpression AuthorizeExpression
		{
			get => this.authorizeExpression;
			set => this.authorizeExpression = value;
		}

		/// <summary>
		/// Optional resource map to apply to resources referred to in document.
		/// </summary>
		public IResourceMap ResourceMap
		{
			get => this.resourceMap;
			set => this.resourceMap = value;
		}

		/// <summary>
		/// Optional progress reporting of encoding/decoding. Can be null.
		/// </summary>
		public ICodecProgress Progress
		{
			get => this.progress;
			set => this.progress = value;
		}

		/// <summary>
		/// Evaluates a file name from a file reference.
		/// </summary>
		/// <param name="DocumentFileName">Filename of original markdown document.</param>
		/// <param name="FileNameReference">Filename reference.</param>
		/// <returns>Physical filename.</returns>
		public string GetFileName(string DocumentFileName, string FileNameReference)
		{
			string FileName;
			char ch;

			if (!string.IsNullOrEmpty(FileNameReference) &&
				((ch = FileNameReference[0]) == Path.DirectorySeparatorChar || ch == '/') &&
				!string.IsNullOrEmpty(this.rootFolder))
			{
				if (this.resourceMap is null || !this.resourceMap.TryGetFileName(FileNameReference, true, out FileName))
					FileName = Path.Combine(this.rootFolder, FileNameReference.Substring(1));
			}
			else
				FileName = Path.Combine(Path.GetDirectoryName(DocumentFileName), FileNameReference);

			return FileName;
		}

		/// <summary>
		/// Sets the default emoji source.
		/// </summary>
		/// <param name="EmojiSource">Emoji source to use if no other source is specified.</param>
		/// <param name="Lock">If the default emoji source is to be locked.</param>
		/// <exception cref="InvalidOperationException">If attempting to change a locked 
		/// default emoji source.</exception>
		public static void SetDefaultEmojiSource(IEmojiSource EmojiSource, bool Lock)
		{
			if (defaultEmojiSourceLocked)
			{
				if (defaultEmojiSource == EmojiSource)
					return;
				else
					throw new InvalidOperationException("Default emoji source is locked.");
			}

			defaultEmojiSource = EmojiSource;
			defaultEmojiSourceLocked = Lock;
		}
	}
}
