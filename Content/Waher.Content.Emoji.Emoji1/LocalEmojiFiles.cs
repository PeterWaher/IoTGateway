using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Settings;

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
	/// Delegate to a ReadAllBytes method.
	/// </summary>
	/// <param name="path">Path of file to load.</param>
	/// <returns>Contents of file.</returns>
	public delegate byte[] ReadAllBytesHandler(string path);

	/// <summary>
	/// Provides emojis from Emoji One (http://emojione.com/) stored as local files.
	/// </summary>
	public class Emoji1LocalFiles : IEmojiSource, IDisposable
	{
		private Emoji1SourceFileType sourceFileType;
		private ManualResetEvent initialized = new ManualResetEvent(false);
		private string zipFileName;
		private string programDataFolder;
		private string imageUrl;
		private int width;
		private int height;

		/// <summary>
		/// Provides emojis from Emoji One (http://emojione.com/) stored as local files.
		/// </summary>
		/// <param name="SourceFileType">Type of files to use.</param>
		/// <param name="Width">Desired width of emojis.</param>
		/// <param name="Height">Desired height of emojis.</param>
		/// <param name="ZipFileName">Full path of the emoji1 zip file. It will be deleted after unpacking to <paramref name="ProgramDataFolder"/>.</param>
		/// <param name="ProgramDataFolder">Folder to unzip emojis to.</param>
		public Emoji1LocalFiles(Emoji1SourceFileType SourceFileType, int Width, int Height,
			string ZipFileName, string ProgramDataFolder)
			: this(SourceFileType, Width, Height, string.Empty, ZipFileName, ProgramDataFolder)
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
		/// <param name="ZipFileName">Full path of the emoji1 zip file. It will be deleted after unpacking to <paramref name="ProgramDataFolder"/>.</param>
		/// <param name="ProgramDataFolder">Folder to unzip emojis to.</param>
		public Emoji1LocalFiles(Emoji1SourceFileType SourceFileType, int Width, int Height, string ImageURL,
			string ZipFileName, string ProgramDataFolder)
		{
			this.sourceFileType = SourceFileType;
			this.width = Width;
			this.height = Height;
			this.imageUrl = ImageURL;
			this.zipFileName = ZipFileName;
			this.programDataFolder = ProgramDataFolder;

			try
			{
				DateTime TP = File.GetLastWriteTime(this.zipFileName);

				if (File.Exists(this.zipFileName) && RuntimeSettings.Get(this.zipFileName, DateTime.MinValue) != TP)
				{
					if (!Directory.Exists(ProgramDataFolder))
						Directory.CreateDirectory(ProgramDataFolder);
					else
					{
						string Folder = Path.Combine(ProgramDataFolder, "Emoji1");

						if (Directory.Exists(Folder))
							Directory.Delete(Folder, true);
					}

					this.initialized = new ManualResetEvent(false);

					Task.Run(this.Unpack);
				}
				else
					this.initialized = new ManualResetEvent(true);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task Unpack()
		{
			try
			{
				Log.Informational("Starting unpacking file.",
					new KeyValuePair<string, object>("FileName", this.zipFileName),
					new KeyValuePair<string, object>("Destination", this.programDataFolder));

				ZipFile.ExtractToDirectory(this.zipFileName, this.programDataFolder);

				DateTime TP = File.GetLastWriteTime(this.zipFileName);
				await RuntimeSettings.SetAsync(this.zipFileName, TP);

				try
				{
					File.Delete(this.zipFileName);
					Log.Informational("File unpacked and deleted.", new KeyValuePair<string, object>("FileName", this.zipFileName));
				}
				catch (Exception)
				{
					Log.Informational("File unpacked.", new KeyValuePair<string, object>("FileName", this.zipFileName));
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				this.initialized.Set();
			}
		}

		/// <summary>
		/// Waits until initialization is completed.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>If initialization completed successfully.</returns>
		public bool WaitUntilInitialized(int TimeoutMilliseconds)
		{
			return this.initialized.WaitOne(TimeoutMilliseconds);
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
			string FileName = this.GetFileName(Emoji);
			return File.Exists(FileName);
		}

		/// <summary>
		/// Gets the local file name for a given emoji.
		/// </summary>
		/// <param name="Emoji">Emoji</param>
		/// <returns>Local file name.</returns>
		public string GetFileName(EmojiInfo Emoji)
		{
			switch (this.sourceFileType)
			{
				case Emoji1SourceFileType.Png64: return Path.Combine(this.programDataFolder, "Emoji1", "png", "64x64", Emoji.FileName);
				case Emoji1SourceFileType.Png128: return Path.Combine(this.programDataFolder, "Emoji1", "png", "128x128", Emoji.FileName);
				case Emoji1SourceFileType.Png512: return Path.Combine(this.programDataFolder, "Emoji1", "png", "512x512", Emoji.FileName);
				case Emoji1SourceFileType.Svg:
				default:
					string s = Emoji.FileName;
					if (s.EndsWith(".png"))
						s = s.Substring(0, s.Length - 3) + "svg";

					return Path.Combine(this.programDataFolder, "Emoji1", "svg", s);
			}
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
			if (s == null || s.IndexOfAny(specialCharacters) < 0)
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

				string FileName = this.GetFileName(Emoji);
				byte[] Data = File.ReadAllBytes(FileName);

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

		/// <summary>
		/// <see cref="IDisposable"/>
		/// </summary>
		public void Dispose()
		{
			if (this.initialized != null)
			{
				this.initialized.Dispose();
				this.initialized = null;
			}
		}
	}
}
