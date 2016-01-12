using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Networking;

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
	/// Provides emojis from Emoji One (http://emojione.com/) stored as local files.
	/// </summary>
	public class Emoji1LocalFiles : IEmojiSource
	{
		private Emoji1SourceFileType sourceFileType;
		private string imageUrl;
		private int width;
		private int height;

		/// <summary>
		/// Provides emojis from Emoji One (http://emojione.com/) stored as local files.
		/// </summary>
		/// <param name="SourceFileType">Type of files to use.</param>
		/// <param name="Width">Desired width of emojis.</param>
		/// <param name="Height">Desired height of emojis.</param>
		public Emoji1LocalFiles(Emoji1SourceFileType SourceFileType, int Width, int Height)
			: this(SourceFileType, Width, Height, string.Empty)
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
		public Emoji1LocalFiles(Emoji1SourceFileType SourceFileType, int Width, int Height, string ImageURL)
		{
			this.sourceFileType = SourceFileType;
			this.width = Width;
			this.height = Height;
			this.imageUrl = ImageURL;
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
			return File.Exists(this.GetLocalFileName(Emoji));
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
		public void GenerateHTML(StringBuilder Output, EmojiInfo Emoji)
		{
			Output.Append("<img alt=\":");
			Output.Append(XML.Encode(Emoji.ShortName));
			Output.Append(":\" title=\"");
			Output.Append(XML.Encode(Emoji.Description));
			Output.Append("\" width=\"");
			Output.Append(this.width.ToString());
			Output.Append("\" height=\"");
			Output.Append(this.height.ToString());
			Output.Append("\" src=\"");

			if (string.IsNullOrEmpty(this.imageUrl))
			{
				Output.Append("data:image/");

				if (this.sourceFileType == Emoji1SourceFileType.Svg)
					Output.Append("svg+xml");
				else
					Output.Append("png");

				Output.Append(";base64,");

				byte[] Data = File.ReadAllBytes(this.GetLocalFileName(Emoji));

				Output.Append(Convert.ToBase64String(Data));
			}
			else
			{
				string s = Emoji.FileName;
				if (this.sourceFileType == Emoji1SourceFileType.Svg && s.EndsWith(".png"))
					s = s.Substring(0, s.Length - 3) + "svg";

				Output.Append(XML.Encode(this.imageUrl.Replace("%FILENAME%", s)));
			}

			Output.Append("\"/>");
		}
	}
}
