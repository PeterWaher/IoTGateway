using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Dsn
{
	/// <summary>
	/// Decodes Delivery Status information, as defined in RFC 3464:
	/// 
	/// https://tools.ietf.org/html/rfc3464
	/// </summary>
	public class DeliveryStatusDecoder : IContentDecoder
	{
		/// <summary>
		/// message/delivery-status
		/// </summary>
		public const string ContentType = "message/delivery-status";

		/// <summary>
		/// Decodes Delivery Status information, as defined in RFC 3464:
		/// 
		/// https://tools.ietf.org/html/rfc3464
		/// </summary>
		public DeliveryStatusDecoder()
		{
		}

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => new string[] { ContentType };

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => new string[] { "dsn" };

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == DeliveryStatusDecoder.ContentType)
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
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (FileExtension.ToLower() == "dsn")
			{
				ContentType = DeliveryStatusDecoder.ContentType;
				return true;
			}
			else
			{
				ContentType = string.Empty;
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
			string Dsn = CommonTypes.GetString(Data, Encoding ?? Encoding.ASCII);
			List<string[]> Sections = new List<string[]>();
			List<string> Section = new List<string>();

			foreach (string Row in Dsn.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
			{
				if (string.IsNullOrEmpty(Row))
				{
					if (Section.Count > 0)
					{
						Sections.Add(Section.ToArray());
						Section.Clear();
					}
				}
				else
				{
					if (Row[0] <= ' ' && Section.Count > 0)
						Section[Section.Count - 1] += Row;
					else
						Section.Add(Row);
				}
			}

			if (Section.Count > 0)
			{
				Sections.Add(Section.ToArray());
				Section.Clear();
			}

			PerMessageFields PerMessage;
			PerRecipientFields[] PerRecipients;
			int i, c = Sections.Count;

			if (c == 0)
			{
				PerMessage = new PerMessageFields(new string[0]);
				PerRecipients = new PerRecipientFields[0];
			}
			else
			{
				PerMessage = new PerMessageFields(Sections[0]);
				PerRecipients = new PerRecipientFields[c - 1];

				for (i = 1; i < c; i++)
					PerRecipients[i - 1] = new PerRecipientFields(Sections[i]);
			}

			return new DeliveryStatus(Dsn, PerMessage, PerRecipients);
		}
	}
}
