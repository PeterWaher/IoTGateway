using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Content;
using Waher.Script;

namespace Waher.Content.Multipart
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
			Dictionary<string, object> Form = new Dictionary<string, object>();

			Decode(Data, Fields, Form, null);

			return Form;
		}

		internal static void Decode(byte[] Data, KeyValuePair<string, string>[] Fields, Dictionary<string, object> Form, List<object> List)
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
			int j, k;

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

						string Header = System.Text.Encoding.ASCII.GetString(Data, Start, j - Start);
						string Key, Value;
						byte[] Data2 = new byte[i - j - 4 - k];
						string ContentType2 = "text/plain";
						string ContentDisposition = string.Empty;
						string ContentTransferEncoding = string.Empty;
						string Name = string.Empty;
						string FileName = string.Empty;
						object Obj;

						Array.Copy(Data, j + 4, Data2, 0, i - j - 4 - k);

						foreach (string Row in Header.Split(CommonTypes.CRLF))
						{
							j = Row.IndexOf(':');
							if (j < 0)
								continue;

							Key = Row.Substring(0, j).Trim();
							Value = Row.Substring(j + 1).Trim();

							switch (Key.ToUpper())
							{
								case "CONTENT-TYPE":
									ContentType2 = Value;
									break;

								case "CONTENT-DISPOSITION":

									j = Value.IndexOf(';');
									if (j < 0)
										ContentDisposition = Value;
									else
									{
										ContentDisposition = Value.Substring(0, j).Trim();

										foreach (KeyValuePair<string, string> Field in CommonTypes.ParseFieldValues(Value.Substring(j + 1).Trim()))
										{
											switch (Field.Key.ToUpper())
											{
												case "NAME":
													Name = Field.Value;
													break;

												case "FILENAME":
													FileName = Field.Value;
													break;
											}
										}
									}
									break;

								case "CONTENT-TRANSFER-ENCODING":
									ContentTransferEncoding = Value;
									break;
							}
						}

						if (!string.IsNullOrEmpty(ContentTransferEncoding))
						{
							switch (ContentTransferEncoding.ToUpper())
							{
								case "7BIT":
								case "8BIT":
								case "BINARY":
									break;

								case "BASE64":
									string s = System.Text.Encoding.ASCII.GetString(Data2);
									Data2 = Convert.FromBase64String(s);
									break;

								case "QUOTED-PRINTABLE":
									MemoryStream ms = new MemoryStream();
									byte b;
									char ch;
									for (j = 0, k = Data2.Length; j < k; j++)
									{
										b = Data2[j];

										if (b == (byte)'=' && j + 2 < k)
										{
											b = 0;
											ch = (char)Data2[++j];

											if (ch >= '0' && ch <= '9')
												b = (byte)(ch - '0');
											else if (ch >= 'a' && ch <= 'f')
												b = (byte)(ch - 'a' + 10);
											else if (ch >= 'A' && ch <= 'F')
												b = (byte)(ch - 'A' + 10);

											b <<= 4;

											ch = (char)Data2[++j];

											if (ch >= '0' && ch <= '9')
												b |= (byte)(ch - '0');
											else if (ch >= 'a' && ch <= 'f')
												b |= (byte)(ch - 'a' + 10);
											else if (ch >= 'A' && ch <= 'F')
												b |= (byte)(ch - 'A' + 10);
										}

										ms.WriteByte(b);
									}

									ms.Capacity = (int)ms.Position;
									Data2 = ms.ToArray();
									ms.Dispose();
									break;

								default:
									throw new Exception("Unrecognized Content-Transfer-Encoding: " + ContentTransferEncoding);
							}
						}

						Obj = InternetContent.Decode(ContentType2, Data2);

						if (Form != null)
						{
							Form[Name] = Obj;

							if (!(Obj is byte[]))
								Form[Name + "_Binary"] = Data2;

							if (!string.IsNullOrEmpty(ContentType2))
								Form[Name + "_ContentType"] = ContentType2;

							if (!string.IsNullOrEmpty(FileName))
								Form[Name + "_FileName"] = FileName;
						}

						if (List != null)
							List.Add(Obj);
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
