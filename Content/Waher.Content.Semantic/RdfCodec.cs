using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Encoder and Decoder of semantic information in RDF Documents.
	/// </summary>
	public class RdfCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// Encoder and Decoder of semantic information in RDF Documents.
		/// </summary>
		public RdfCodec()
		{
		}

		/// <summary>
		/// Supported Internet Content Types.
		/// </summary>
		public string[] ContentTypes => RdfContentTypes;

		private static readonly string[] RdfContentTypes = new string[]
		{
			"application/rdf+xml"
		};

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => RdfFileExtensions;

		private static readonly string[] RdfFileExtensions = new string[]
		{
			"rdf"
		};

		/// <summary>
		/// If the decoder decodes content of a given Internet Content Type.
		/// </summary>
		/// <param name="ContentType">Content Type</param>
		/// <param name="Grade">How well the decoder supports the given content type.</param>
		/// <returns>If the decoder decodes the given content type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf(RdfContentTypes, ContentType) >= 0)
			{
				Grade = Grade.Ok;
				return true;
			}
			else
			{
				Grade = Grade.NotAtAll;
				return false;
			}
		}

		/// <summary>
		/// Decodes an object
		/// </summary>
		/// <param name="ContentType">Content Type</param>
		/// <param name="Data">Binary representation of object.</param>
		/// <param name="Encoding">Encoding</param>
		/// <param name="Fields">Additional fields</param>
		/// <param name="BaseUri">Base URI</param>
		/// <returns>Decoded object.</returns>
		public Task<object> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			string s = CommonTypes.GetString(Data, Encoding);
			XmlDocument Xml = new XmlDocument();
			Xml.LoadXml(s);

			RdfDocument Parsed = new RdfDocument(Xml, BaseUri, "n", BlankNodeIdMode.Guid);
			return Task.FromResult<object>(Parsed);
		}

		/// <summary>
		/// If the encoder encodes a specific object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Grade">How well the encoder supports the given object.</param>
		/// <param name="AcceptedContentTypes">Accepted content types.</param>
		/// <returns>If the encoder encodes the given object.</returns>
		public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
		{
			if (Object is RdfDocument &&
				InternetContent.IsAccepted(RdfContentTypes, AcceptedContentTypes))
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
		/// Encodes an object
		/// </summary>
		/// <param name="Object">Object to encode</param>
		/// <param name="Encoding">Encoding</param>
		/// <param name="AcceptedContentTypes">Accepted content types.</param>
		/// <returns></returns>
		public Task<KeyValuePair<byte[], string>> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			if (Encoding is null)
				Encoding = Encoding.UTF8;

			string Text;

			if (Object is RdfDocument Doc)
				Text = Doc.Xml.OuterXml;
			else
				throw new ArgumentException("Unable to encode object.", nameof(Object));

			byte[] Bin = Encoding.GetBytes(Text);
			string ContentType = RdfContentTypes[0] + "; charset=" + Encoding.WebName;

			return Task.FromResult(new KeyValuePair<byte[], string>(Bin, ContentType));
		}

		/// <summary>
		/// Tries to get the content type of content of a given file extension.
		/// </summary>
		/// <param name="FileExtension">File Extension</param>
		/// <param name="ContentType">Content Type, if recognized.</param>
		/// <returns>If File Extension was recognized and Content Type found.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (FileExtension.ToLower() == RdfFileExtensions[0])
			{
				ContentType = RdfContentTypes[0];
				return true;
			}
			else
			{
				ContentType = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to get the file extension of content of a given content type.
		/// </summary>
		/// <param name="ContentType">Content Type</param>
		/// <param name="FileExtension">File Extension, if recognized.</param>
		/// <returns>If Content Type was recognized and File Extension found.</returns>
		public bool TryGetFileExtension(string ContentType, out string FileExtension)
		{
			if (Array.IndexOf(RdfContentTypes, ContentType) >= 0)
			{
				FileExtension = RdfFileExtensions[0];
				return true;
			}
			else
			{
				FileExtension = null;
				return false;
			}
		}
	}
}
