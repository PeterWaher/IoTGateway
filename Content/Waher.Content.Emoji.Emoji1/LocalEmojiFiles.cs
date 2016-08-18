using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Emoji.Emoji1
{
	/// <summary>
	/// What source files to use when displaying emoji.
	/// </summary>
	public enum Emoji1SourceFileType
	{
		/// <summary>
		/// 64x64 PNG files stored in the Graphics/Emoji1/png/64x64 folder.
		/// </summary>
		Png64,

		/// <summary>
		/// 128x128 PNG files stored in the Graphics/Emoji1/png/128x128 folder.
		/// </summary>
		Png128,

		/// <summary>
		/// 512x512 PNG files stored in the Graphics/Emoji1/png/512x512 folder.
		/// </summary>
		Png512,

		/// <summary>
		/// SVG files stored in the Graphics/Emoji1/svg folder.
		/// </summary>
		Svg
	}

	/// <summary>
	/// Delegate to a FileExists method.
	/// </summary>
	/// <param name="path">Path to check.</param>
	/// <returns>If the file exists.</returns>
	public delegate bool FileExistsHandler(string path);

	/// <summary>
	/// Delegate to a REadAllBytes method.
	/// </summary>
	/// <param name="path">Path of file to load.</param>
	/// <returns>Contents of file.</returns>
	public delegate byte[] ReadAllBytesHandler(string path);

	/// <summary>
	/// Provides emojis from Emoji One (http://emojione.com/) stored as local files.
	/// </summary>
	public class Emoji1LocalFiles : IEmojiSource
	{
		private Emoji1SourceFileType sourceFileType;
		private FileExistsHandler fileexists;
		private ReadAllBytesHandler readAllBytes;
		private string imageUrl;
		private int width;
		private int height;

		/// <summary>
		/// Provides emojis from Emoji One (http://emojione.com/) stored as local files.
		/// </summary>
		/// <param name="SourceFileType">Type of files to use.</param>
		/// <param name="Width">Desired width of emojis.</param>
		/// <param name="Height">Desired height of emojis.</param>
		public Emoji1LocalFiles(Emoji1SourceFileType SourceFileType, int Width, int Height, 
			FileExistsHandler FileExists, ReadAllBytesHandler ReadAllBytes)
			: this(SourceFileType, Width, Height, string.Empty, FileExists, ReadAllBytes)
		{
		}

		/// <summary>
		/// Provides emojis from Emoji One (http://emojione.com/) stored as local files.
		/// </summary>
		/// <param name="SourceFileType">Type of files to use.</param>
		/// <param name="Width">Desired width of emojis.</param>
		/// <param name="Height">Desired height of emojis.</param>
		/// <param name="ImageURL">URL for remote clients to fetch the image. If not provided, images are embedded into generated pages.
		/// Include the string %FILENAME% where the name of the emoji image file is to be inserted.</param>
		/// <param name="FileExists">Delegate to method used to check if a file exists.</param>
		public Emoji1LocalFiles(Emoji1SourceFileType SourceFileType, int Width, int Height, string ImageURL, 
			FileExistsHandler FileExists, ReadAllBytesHandler ReadAllBytes)
		{
			this.sourceFileType = SourceFileType;
			this.width = Width;
			this.height = Height;
			this.imageUrl = ImageURL;
			this.fileexists = FileExists;
			this.readAllBytes = ReadAllBytes;
		}

		/// <summary>
		/// Type of files to use.
		/// </summary>
		public Emoji1SourceFileType SourceFileType
		{
			get { return this.sourceFileType; }
		}

		/// <summary>
		/// Desired width of emojis.
		/// </summary>
		public int Width
		{
			get { return this.width; }
		}

		/// <summary>
		/// Desired height of emojis.
		/// </summary>
		public int Height
		{
			get { return this.height; }
		}

		/// <summary>
		/// If the emoji is supported by the emoji source.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		/// <returns>If emoji is supported.</returns>
		public bool EmojiSupported(EmojiInfo Emoji)
		{
			string LocalFileName = this.GetLocalFileName(Emoji);
			return this.fileexists(LocalFileName);
		}

		/// <summary>
		/// Gets the local file name for a given emoji.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		/// <returns>Local file name.</returns>
		public string GetLocalFileName(EmojiInfo Emoji)
		{
			StringBuilder Result = new StringBuilder();
			string FileName = Emoji.FileName;

			Result.Append("Graphics");
			Result.Append(Path.DirectorySeparatorChar);
			Result.Append("Emoji1");
			Result.Append(Path.DirectorySeparatorChar);

			switch (this.sourceFileType)
			{
				case Emoji1SourceFileType.Png64:
					Result.Append("png");
					Result.Append(Path.DirectorySeparatorChar);
					Result.Append("64x64");
					break;

				case Emoji1SourceFileType.Png128:
					Result.Append("png");
					Result.Append(Path.DirectorySeparatorChar);
					Result.Append("128x128");
					break;

				case Emoji1SourceFileType.Png512:
					Result.Append("png");
					Result.Append(Path.DirectorySeparatorChar);
					Result.Append("512x512");
					break;

				case Emoji1SourceFileType.Svg:
					Result.Append("svg");
					if (FileName.EndsWith(".png"))
						FileName = FileName.Substring(0, FileName.Length - 3) + "svg";
					break;
			}

			Result.Append(Path.DirectorySeparatorChar);
			Result.Append(FileName);

			return Result.ToString();
		}

		/// <summary>
		/// Generates HTML for a given Emoji.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Emoji">Emoji</param>
		/// <param name="EmbedImage">If image should be embedded into the generated HTML, using the data URI scheme.</param>
		public void GenerateHTML(StringBuilder Output, EmojiInfo Emoji, bool EmbedImage)
		{
			Output.Append("<img alt=\":");
			Output.Append(Encode(Emoji.ShortName));
			Output.Append(":\" title=\"");
			Output.Append(Encode(Emoji.Description));
			Output.Append("\" width=\"");
			Output.Append(this.width.ToString());
			Output.Append("\" height=\"");
			Output.Append(this.height.ToString());
			Output.Append("\" src=\"");
			Output.Append(Encode(this.GetUrl(Emoji, EmbedImage)));
			Output.Append("\"/>");
		}

		private static string Encode(string s)
		{
			if (s.IndexOfAny(specialCharacters) < 0)
				return s;

			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;").
				Replace("\"", "&quot;").
				Replace("'", "&apos;");
		}

		private static readonly char[] specialCharacters = new char[] { '<', '>', '&', '"', '\'' };

		/// <summary>
		/// Gets an URL for the emoji.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		/// <param name="Embed">If emoji should be embedded.</param>
		/// <returns>URL</returns>
		public string GetUrl(EmojiInfo Emoji, bool Embed)
		{
			if (Embed || string.IsNullOrEmpty(this.imageUrl))
			{
				StringBuilder Output = new StringBuilder();

				Output.Append("data:image/");

				if (this.sourceFileType == Emoji1SourceFileType.Svg)
					Output.Append("svg+xml");
				else
					Output.Append("png");

				Output.Append(";base64,");

				string LocalFileName = this.GetLocalFileName(Emoji);
				byte[] Data = this.readAllBytes(LocalFileName);

				Output.Append(Convert.ToBase64String(Data));

				return Output.ToString();
			}
			else
			{
				string s = Emoji.FileName;
				if (this.sourceFileType == Emoji1SourceFileType.Svg && s.EndsWith(".png"))
					s = s.Substring(0, s.Length - 3) + "svg";

				return this.imageUrl.Replace("%FILENAME%", s);
			}
		}

		/// <summary>
		/// Gets the image source of an emoji.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		/// <param name="Url">URL to emoji.</param>
		/// <param name="Width">Width of emoji.</param>
		/// <param name="Height">Height of emoji.</param>
		public void GetImageSource(EmojiInfo Emoji, out string Url, out int Width, out int Height)
		{
			Url = this.GetUrl(Emoji, false);
			Width = this.width;
			Height = this.height;
		}

	}
}
