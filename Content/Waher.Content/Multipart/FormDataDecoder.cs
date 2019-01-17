using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Content;
using Waher.Runtime.Inventory;

namespace Waher.Content.Multipart
{
	/// <summary>
	/// Decoder of form data.
	/// 
	/// https://tools.ietf.org/html/rfc7578
	/// </summary>
	public class FormDataDecoder : IContentDecoder
	{
		/// <summary>
		/// multipart/form-data
		/// </summary>
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
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public object Decode(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			Dictionary<string, object> Form = new Dictionary<string, object>();

			Decode(Data, Fields, Form, null, BaseUri);

			return Form;
		}

		internal static void Decode(byte[] Data, KeyValuePair<string, string>[] Fields, Dictionary<string, object> Form,
			List<EmbeddedContent> List, Uri BaseUri)
		{
			string Boundary = null;

			if (Fields != null)
			{
				foreach (KeyValuePair<string, string> P in Fields)
				{
					if (P.Key.ToUpper() == "BOUNDARY")
					{
						Boundary = P.Value;
						break;
					}
				}
			}

			if (string.IsNullOrEmpty(Boundary))
				throw new Exception("No boundary defined.");

			byte[] BoundaryBin = Encoding.ASCII.GetBytes(Boundary);
			int Start = 0;
			int i = 0;
			int c = Data.Length;
			int d = BoundaryBin.Length;
			int j, k, l, m;

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
						k = 0;
						if (Data[i - 1] == '-' && Data[i - 2] == '-')
							k = 2;

						if (Data[i - 1 - k] == '\n' && Data[i - 2 - k] == '\r')
							k += 2;

						string Header = Encoding.ASCII.GetString(Data, Start, j - Start);
						string Key, Value;
						byte[] Data2 = new byte[i - j - 4 - k];
						EmbeddedContent EmbeddedContent = new EmbeddedContent()
						{
							ContentType = "text/plain",
							Raw = Data2
						};

						Array.Copy(Data, j + 4, Data2, 0, i - j - 4 - k);

						string[] Rows = Header.Split(CommonTypes.CRLF);
						l = Rows.Length;
						m = -1;

						for (j = 0; j < l; j++)
						{
							Key = Rows[j];
							if (!string.IsNullOrEmpty(Key))
							{
								if (char.IsWhiteSpace(Key[0]) && m >= 0)
								{
									Rows[m] += Key;
									Rows[j] = string.Empty;
								}
								else
									m = j;
							}
						}

						foreach (string Row in Rows)
						{
							j = Row.IndexOf(':');
							if (j < 0)
								continue;

							Key = Row.Substring(0, j).Trim();
							Value = Row.Substring(j + 1).Trim();

							switch (Key.ToUpper())
							{
								case "CONTENT-TYPE":
									EmbeddedContent.ContentType = Value;
									j = Value.IndexOf(';');
									if (j >= 0)
									{
										ParseContentFields(Value.Substring(j + 1).Trim(), EmbeddedContent);
										Value = Value.Substring(0, j).Trim();
									}
									break;

								case "CONTENT-DISPOSITION":
									j = Value.IndexOf(';');
									if (j >= 0)
									{
										ParseContentFields(Value.Substring(j + 1).Trim(), EmbeddedContent);
										Value = Value.Substring(0, j).Trim();
									}

									switch (Value.ToUpper())
									{
										case "INLINE":
											EmbeddedContent.Disposition = ContentDisposition.Inline;
											break;

										case "ATTACHMENT":
											EmbeddedContent.Disposition = ContentDisposition.Attachment;
											break;
									}
									break;

								case "CONTENT-TRANSFER-ENCODING":
									EmbeddedContent.TransferEncoding = Value;
									break;

								case "CONTENT-ID":
									EmbeddedContent.ID = Value;
									break;

								case "CONTENT-DESCRIPTION":
									EmbeddedContent.Description = Value;
									break;
							}
						}

						if (!string.IsNullOrEmpty(EmbeddedContent.TransferEncoding))
						{
							if (TryTransferDecode(Data2, EmbeddedContent.TransferEncoding, out Data2))
								EmbeddedContent.TransferDecoded = Data2;
							else
								throw new Exception("Unrecognized Content-Transfer-Encoding: " + EmbeddedContent.TransferEncoding);
						}

						EmbeddedContent.Decoded = InternetContent.Decode(EmbeddedContent.ContentType, Data2, BaseUri);

						if (Form != null)
						{
							Form[EmbeddedContent.Name] = EmbeddedContent.Decoded;

							if (!(EmbeddedContent.Decoded is byte[]))
								Form[EmbeddedContent.Name + "_Binary"] = Data2;

							if (!string.IsNullOrEmpty(EmbeddedContent.ContentType))
								Form[EmbeddedContent.Name + "_ContentType"] = EmbeddedContent.ContentType;

							if (!string.IsNullOrEmpty(EmbeddedContent.FileName))
								Form[EmbeddedContent.Name + "_FileName"] = EmbeddedContent.FileName;
						}

						if (List != null)
							List.Add(EmbeddedContent);
					}

					i += d;
					while (i < c && Data[i] <= 32)
						i++;

					Start = i;
				}
				else
					i++;
			}
		}

		private static void ParseContentFields(string s, EmbeddedContent EmbeddedContent)
		{
			foreach (KeyValuePair<string, string> Field in CommonTypes.ParseFieldValues(s))
			{
				switch (Field.Key.ToUpper())
				{
					case "NAME":
						EmbeddedContent.Name = Field.Value;
						break;

					case "FILENAME":
						EmbeddedContent.FileName = Field.Value;
						break;
				}
			}
		}

		/// <summary>
		/// Tries to decode transfer-encoded binary data.
		/// </summary>
		/// <param name="Encoded">Transfer-encoded binary data.</param>
		/// <param name="TransferEncoding">Transfer-encoding used.</param>
		/// <param name="Decoded">Decoded binary data.</param>
		/// <returns>If decoding was successful.</returns>
		public static bool TryTransferDecode(byte[] Encoded, string TransferEncoding, out byte[] Decoded)
		{
			switch (TransferEncoding.ToUpper())
			{
				case "7BIT":
				case "8BIT":
				case "BINARY":
				case "":
				case null:
					Decoded = Encoded;
					return true;

				case "BASE64":
					string s = Encoding.ASCII.GetString(Encoded);
					Decoded = Convert.FromBase64String(s);
					return true;

				case "QUOTED-PRINTABLE":
					MemoryStream ms = new MemoryStream();
					byte b;
					char ch;
					int j, k;

					for (j = 0, k = Encoded.Length; j < k; j++)
					{
						b = Encoded[j];

						if (b == (byte)'=' && j + 2 < k)
						{
							b = 0;
							ch = (char)Encoded[++j];

							if (ch >= '0' && ch <= '9')
								b = (byte)(ch - '0');
							else if (ch >= 'a' && ch <= 'f')
								b = (byte)(ch - 'a' + 10);
							else if (ch >= 'A' && ch <= 'F')
								b = (byte)(ch - 'A' + 10);
							else if (ch == '\r')
							{
								if (Encoded[j + 1] == (byte)'\n')
									j++;

								continue;
							}
							else
							{
								ms.WriteByte((byte)'=');
								ms.WriteByte((byte)ch);
								continue;
							}

							b <<= 4;

							ch = (char)Encoded[++j];

							if (ch >= '0' && ch <= '9')
								b |= (byte)(ch - '0');
							else if (ch >= 'a' && ch <= 'f')
								b |= (byte)(ch - 'a' + 10);
							else if (ch >= 'A' && ch <= 'F')
								b |= (byte)(ch - 'A' + 10);
						}

						ms.WriteByte(b);
					}

					Decoded = ms.ToArray();
					ms.Dispose();
					return true;

				default:
					Decoded = null;
					return false;
			}
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

		/// <summary>
		/// Encodes multi-part content
		/// </summary>
		/// <param name="Content">Multi-part content.</param>
		/// <param name="Boundary">Boundary to use.</param>
		/// <returns>Encoded multi-part content.</returns>
		public static byte[] Encode(IEnumerable<EmbeddedContent> Content, string Boundary)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				StringBuilder Header = new StringBuilder();

				foreach (EmbeddedContent Alternative in Content)
				{
					Alternative.AssertEncoded();

					Header.Clear();
					Header.Append("\r\n--");
					Header.Append(Boundary);
					Header.Append("\r\nContent-Transfer-Encoding: ");
					Header.Append(Alternative.TransferEncoding);
					Header.Append("\r\nContent-Type: ");
					Header.Append(Alternative.ContentType);

					if (!string.IsNullOrEmpty(Alternative.Name))
					{
						Header.Append("; name=\"");
						Header.Append(Alternative.Name.Replace("\"", "\\\""));
						Header.Append("\"");
					}

					if (Alternative.Disposition != ContentDisposition.Unknown ||
						!string.IsNullOrEmpty(Alternative.FileName))
					{
						Header.Append("\r\nContent-Disposition: ");

						switch (Alternative.Disposition)
						{
							case ContentDisposition.Inline:
								Header.Append("inline");
								break;

							case ContentDisposition.Attachment:
							default:
								Header.Append("attachment");
								break;
						}

						if (!string.IsNullOrEmpty(Alternative.FileName))
						{
							Header.Append("; filename=\"");
							Header.Append(Alternative.FileName.Replace("\"", "\\\""));
							Header.Append("\"");
						}
					}

					if (!string.IsNullOrEmpty(Alternative.ID))
					{
						Header.Append("\r\nContent-ID: ");
						Header.Append(Alternative.ID);
					}

					if (!string.IsNullOrEmpty(Alternative.Description))
					{
						Header.Append("\r\nContent-Description: ");
						Header.Append(Alternative.Description);
					}

					Header.Append("\r\n\r\n");

					byte[] HeaderBin = Encoding.ASCII.GetBytes(Header.ToString());

					ms.Write(HeaderBin, 0, HeaderBin.Length);
					ms.Write(Alternative.Raw, 0, Alternative.Raw.Length);
				}

				return ms.ToArray();
			}
		}
	}
}
