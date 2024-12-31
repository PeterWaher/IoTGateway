using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Waher.Content.Emoji;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model.Multimedia
{
	/// <summary>
	/// Image content.
	/// </summary>
	public class ImageContent : MultimediaContent
	{
		/// <summary>
		/// Image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Item">Multimedia item.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(MultimediaItem Item)
		{
			if (Item.ContentType.StartsWith("image/"))
				return Grade.Ok;
			else
				return Grade.Barely;
		}

		/// <summary>
		/// If the link provided should be embedded in a multi-media construct automatically.
		/// </summary>
		/// <param name="Url">Inline link.</param>
		public override bool EmbedInlineLink(string Url)
		{
			string Extension = Path.GetExtension(Url);
			string ContentType = InternetContent.GetContentType(Extension);

			return ContentType.StartsWith("image/");
		}

		/// <summary>
		/// Checks a Data URI image, that it contains a decodable image.
		/// </summary>
		/// <param name="Source">Image source.</param>
		public static async Task<IImageSource> CheckDataUri(IImageSource Source)
		{
			string Url = Source.Url;
			int i;

			if (Url.StartsWith("data:", StringComparison.CurrentCultureIgnoreCase) && (i = Url.IndexOf("base64,")) > 0)
			{
				int? Width = Source.Width;
				int? Height = Source.Height;
				byte[] Data = Convert.FromBase64String(Url.Substring(i + 7));
				using (SKBitmap Bitmap = SKBitmap.Decode(Data))
				{
					Width = Bitmap.Width;
					Height = Bitmap.Height;
				}

				Url = await GetTemporaryFile(Data);

				return new ImageSource()
				{
					Url = Url,
					Width = Width,
					Height = Height
				};
			}
			else
				return Source;
		}

		/// <summary>
		/// Stores an image in binary form as a temporary file. Files will be deleted when application closes.
		/// </summary>
		/// <param name="BinaryImage">Binary image.</param>
		/// <returns>Temporary file name.</returns>
		public static Task<string> GetTemporaryFile(byte[] BinaryImage)
		{
			return GetTemporaryFile(BinaryImage, "tmp");
		}

		/// <summary>
		/// Stores an image in binary form as a temporary file. Files will be deleted when application closes.
		/// </summary>
		/// <param name="BinaryImage">Binary image.</param>
		/// <param name="FileExtension">File extension.</param>
		/// <returns>Temporary file name.</returns>
		public static async Task<string> GetTemporaryFile(byte[] BinaryImage, string FileExtension)
		{
			string FileName;

			using (SHA256 H = SHA256.Create())
			{
				byte[] Digest = H.ComputeHash(BinaryImage);
				FileName = Path.Combine(Path.GetTempPath(), "tmp" + Base64Url.Encode(Digest) + "." + FileExtension);
			}

			if (!File.Exists(FileName))
			{
				await Resources.WriteAllBytesAsync(FileName, BinaryImage);

				lock (synchObject)
				{
					if (temporaryFiles is null)
					{
						temporaryFiles = new Dictionary<string, bool>();
						Log.Terminating += CurrentDomain_ProcessExit;
					}

					temporaryFiles[FileName] = true;
				}
			}

			return FileName;
		}

		private static Dictionary<string, bool> temporaryFiles = null;
		private readonly static object synchObject = new object();

		private static Task CurrentDomain_ProcessExit(object Sender, EventArgs e)
		{
			lock (synchObject)
			{
				if (!(temporaryFiles is null))
				{
					foreach (string FileName in temporaryFiles.Keys)
					{
						try
						{
							File.Delete(FileName);
						}
						catch (Exception)
						{
							// Ignore
						}
					}

					temporaryFiles.Clear();
				}
			}

			return Task.CompletedTask;
		}
	}
}
