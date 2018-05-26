using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Waher.Content;
using Waher.Runtime.Inventory;

namespace Waher.Security.ACME.Decoders
{
	/// <summary>
	/// Decodes certificates encoded using the application/pem-certificate-chain content type.
	/// </summary>
	public class PemDecoder : IContentDecoder
	{
		/// <summary>
		/// application/pem-certificate-chain
		/// </summary>
		public const string ContentType = "application/pem-certificate-chain";

		/// <summary>
		/// Decodes certificates encoded using the application/pem-certificate-chain content type.
		/// </summary>
		public PemDecoder()
		{
		}

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => new string[] { ContentType };

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => new string[] { "pem" };

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == PemDecoder.ContentType)
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
			List<X509Certificate2> Certificates = new List<X509Certificate2>();
			string s = Encoding.ASCII.GetString(Data);
			int i;

			while ((i = s.IndexOf("-----BEGIN CERTIFICATE-----")) >= 0)
			{
				s = s.Substring(i + 27).TrimStart();

				i = s.IndexOf("-----END CERTIFICATE-----");
				if (i > 0)
				{
					byte[] Bin = Convert.FromBase64String(s.Substring(0, i).TrimEnd());
					s = s.Substring(i + 25).TrimStart();

					X509Certificate2 Certificate = new X509Certificate2(Bin);

					Certificates.Add(Certificate);
				}
			}

			return Certificates.ToArray();
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (FileExtension.ToLower() == "pem")
			{
				ContentType = PemDecoder.ContentType;
				return true;
			}
			else
			{
				ContentType = null;
				return false;
			}
		}
	}
}
