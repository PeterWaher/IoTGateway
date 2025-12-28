using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;

namespace Waher.Content.Zip
{
	/// <summary>
	/// Converts an object to a ZIP File.
	/// </summary>
	public class ObjectToZipConverter : IContentConverter
	{
		private readonly static Dictionary<string, bool> protectedContentTypes = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		private static string[] canConvertFrom = null;

		/// <summary>
		/// Protects a content type, so that it cannot be included in generated zip files by external parties through any content conversion procedures.
		/// </summary>
		/// <param name="ContentType">Content type to protect.</param>
		public static void ProtectContentType(string ContentType)
		{
			lock (protectedContentTypes)
			{
				protectedContentTypes[ContentType] = true;
				canConvertFrom = null;
			}
		}

		/// <summary>
		/// Checks if a Content-Type is protected.
		/// </summary>
		/// <param name="ContentType">Content-Type to check.</param>
		/// <returns>if the content type is protected.</returns>
		public static bool IsProtected(string ContentType)
		{
			lock (protectedContentTypes)
			{
				return protectedContentTypes.TryGetValue(ContentType, out bool Protected) && Protected;
			}
		}

		/// <summary>
		/// Converts an object to a ZIP File.
		/// </summary>
		public ObjectToZipConverter()
		{
		}

		/// <summary>
		/// Converts content from these content types.
		/// </summary>
		public string[] FromContentTypes
		{
			get
			{
				string[] Result = canConvertFrom;
				if (!(Result is null))
					return Result;

				ChunkedList<string> Supported = new ChunkedList<string>();
				string[] ZipTypes = ZipCodec.ZipFileContentTypes;

				foreach (string ContentType in InternetContent.CanEncodeContentTypes)
				{
					if (!IsProtected(ContentType) && Array.IndexOf(ZipTypes, ContentType) < 0)
						Supported.Add(ContentType);
				}

				canConvertFrom = Result = Supported.ToArray();

				return Result;
			}
		}

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		public string[] ToContentTypes => ZipCodec.ZipFileContentTypes;

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		public virtual Grade ConversionGrade => Grade.Ok;

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="State">State of the current conversion.</param>
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		public async Task<bool> ConvertAsync(ConversionState State)
		{
			if (State is null)
				return true;

			string SourceFileName = State.FromFileName;

			if (string.IsNullOrEmpty(SourceFileName))
				SourceFileName = "File." + InternetContent.GetFileExtension(State.FromContentType);

			byte[] Data;

			using (MemoryStream Output = new MemoryStream())
			{
				await Zip.CreateZipFile(SourceFileName, State.From, DateTime.Now, Output);
				Data = Output.ToArray();
			}

			await State.To.WriteAsync(Data, 0, Data.Length);

			return false;
		}
	}
}
