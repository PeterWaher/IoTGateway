using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Script;

namespace Waher.Networking.HTTP.Multipart
{
	/// <summary>
	/// Decoder of form data.
	/// 
	/// https://tools.ietf.org/html/rfc7578
	/// </summary>
	public class FormDataDecoder : IContentDecoder
	{
		public const string ContentType = "multipart/form-data";

		/// <summary>
		/// Decoder of form data.
		/// 
		/// https://tools.ietf.org/html/rfc7578
		/// </summary>
		public FormDataDecoder()
		{
		}

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get
			{
				return new string[]
				{
					ContentType
				};
			}
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get
			{
				return new string[]
				{
					"formdata"
				};
			}
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == FormDataDecoder.ContentType)
			{
				Grade = Grade.Excellent;
				return true;
			}
			else
			{
				Grade = Grade.NotAtAll;
				return false;
			}
		}

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		/// <param name="Encoding">Any encoding specified. Can be null if no encoding specified.</param>
		/// <param name="Fields">Any content-type related fields and their corresponding values.</param>
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public object Decode(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields)
		{
			string Boundary = null;

			foreach (KeyValuePair<string, string> P in Fields)
			{
				if (P.Key.ToUpper() == "BOUNDARY")
				{
					Boundary = P.Value;
					break;
				}
			}

			if (string.IsNullOrEmpty(Boundary))
				throw new Exception("No boundary defined.");

			byte[] BoundaryBin = System.Text.Encoding.ASCII.GetBytes(Boundary);
			int Start = 0;
			int i = 0;
			int c = Data.Length;
			int d = BoundaryBin.Length;
			int j;

			while (i < c)
			{
				for (j = 0; j < d; j++)
				{
					if (Data[i + j] != BoundaryBin[j])
						break;
				}

				if (j == d)
				{
					for (j = Start; j < i; j++)
					{
						if (Data[j] == '\r' && Data[j + 1] == '\n' && Data[j + 2] == '\r' && Data[j + 3] == '\n')
							break;
					}

					if (j < i)
					{
						string Header = System.Text.Encoding.ASCII.GetString(Data, Start, j - Start);
						string Key, Value;
						byte[] Data2 = new byte[i - j - 4];

						Array.Copy(Data, j + 4, Data2, 0, i - j - 4);

						foreach (string Row in Header.Split(CommonTypes.CRLF))
						{
							j = Row.IndexOf(':');
							if (j < 0)
								continue;

							Key = Row.Substring(0, i).Trim();
							Value = Row.Substring(i + 1).Trim();


						}
					}

					i += d;
					while (i < c && Data[i] <= 32)
						i++;

					Start = i;
				}
				else
					i++;
			}

			return Data;
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (FileExtension.ToLower() == "formdata")
			{
				ContentType = FormDataDecoder.ContentType;
				return true;
			}
			else
			{
				ContentType = string.Empty;
				return false;
			}
		}
	}
}
