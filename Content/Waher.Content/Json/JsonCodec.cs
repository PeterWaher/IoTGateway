using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Json
{
    /// <summary>
    /// JSON codec.
    /// </summary>
    public class JsonCodec : IContentDecoder, IContentEncoder
    {
		/// <summary>
		/// application/json
		/// </summary>
		public const string DefaultContentType = "application/json";

        /// <summary>
        /// json
        /// </summary>
        public const string DefaultFileExtension = "json";

        /// <summary>
        /// JSON codec.
        /// </summary>
        public JsonCodec()
        {
        }

        /// <summary>
        /// JSON content types.
        /// </summary>
        public static readonly string[] JsonContentTypes = new string[]
        {
            DefaultContentType,
            "text/x-json"
        };

        /// <summary>
        /// JSON file extensions.
        /// </summary>
        public static readonly string[] JsonFileExtensions = new string[]
        {
            DefaultFileExtension
        };

        /// <summary>
        /// Supported content types.
        /// </summary>
        public string[] ContentTypes => JsonContentTypes;

        /// <summary>
        /// Supported file extensions.
        /// </summary>
        public string[] FileExtensions => JsonFileExtensions;

        /// <summary>
        /// If the decoder decodes an object with a given content type.
        /// </summary>
        /// <param name="ContentType">Content type to decode.</param>
        /// <param name="Grade">How well the decoder decodes the object.</param>
        /// <returns>If the decoder can decode an object with the given type.</returns>
        public bool Decodes(string ContentType, out Grade Grade)
        {
            if (ContentType == DefaultContentType || ContentType == "text/x-json")
            {
                Grade = Grade.Excellent;
                return true;
            }
            else if (ContentType.StartsWith("application/") && ContentType.EndsWith("+json"))
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
        /// Decodes an object.
        /// </summary>
        /// <param name="ContentType">Internet Content Type.</param>
        /// <param name="Data">Encoded object.</param>
        /// <param name="Encoding">Any encoding specified. Can be null if no encoding specified.</param>
        /// <param name="Fields">Any content-type related fields and their corresponding values.</param>
        ///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
        /// <returns>Decoded object.</returns>
        /// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
        public Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
        {
            string s = CommonTypes.GetString(Data, Encoding ?? Encoding.UTF8);
            return Task.FromResult(new ContentResponse(ContentType, JSON.Parse(s), Data));
        }

        /// <summary>
        /// Tries to get the content type of an item, given its file extension.
        /// </summary>
        /// <param name="FileExtension">File extension.</param>
        /// <param name="ContentType">Content type.</param>
        /// <returns>If the extension was recognized.</returns>
        public bool TryGetContentType(string FileExtension, out string ContentType)
        {
            if (string.Compare(FileExtension, "json", true) == 0)
            {
                ContentType = DefaultContentType;
                return true;
            }
            else
            {
                ContentType = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the file extension of an item, given its Content-Type.
        /// </summary>
        /// <param name="ContentType">Content type.</param>
        /// <param name="FileExtension">File extension.</param>
        /// <returns>If the Content-Type was recognized.</returns>
        public bool TryGetFileExtension(string ContentType, out string FileExtension)
        {
            ContentType = ContentType.ToLower();

            if (Array.IndexOf(JsonContentTypes, ContentType) >= 0)
            {
                FileExtension = "json";
                return true;
            }
            else if (ContentType.StartsWith("application/") && ContentType.EndsWith("+json"))
            {
                FileExtension = ContentType.Substring(12, ContentType.Length - 5 - 12);
                return true;
            }
            else
            {
                FileExtension = string.Empty;
                return false;
            }
        }

        /// <summary>
        /// If the encoder encodes a given object.
        /// </summary>
        /// <param name="Object">Object to encode.</param>
        /// <param name="Grade">How well the encoder encodes the object.</param>
        /// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
        /// <returns>If the encoder can encode the given object.</returns>
        public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
        {
            if (InternetContent.IsAccepted(JsonContentTypes, AcceptedContentTypes))
            {
                if (Object is IEnumerable<KeyValuePair<string, object>>)
                {
                    Grade = Grade.Ok;
                    return true;
                }
                else if (Object is IJsonEncodingHint Hint)
                {
                    Grade = Hint.CanEncodeJson;
                    return Grade != Grade.NotAtAll;
                }
                else if (Object is null ||
                    Object is IEnumerable ||
                    Object is IVector ||
                    Object is string ||
                    Object is bool ||
                    Object is decimal ||
                    Object is double ||
                    Object is float ||
                    Object is int ||
                    Object is long ||
                    Object is short ||
                    Object is byte ||
                    Object is uint ||
                    Object is ulong ||
                    Object is ushort ||
                    Object is sbyte ||
                    Object is char)
                {
                    Grade = Grade.Barely;
                    return true;
                }
            }

            Grade = Grade.NotAtAll;
            return false;
        }

        /// <summary>
        /// Encodes an object.
        /// </summary>
        /// <param name="Object">Object to encode.</param>
        /// <param name="Encoding">Desired encoding of text. Can be null if no desired encoding is speified.</param>
        /// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
        /// <returns>Encoded object, as well as Content Type of encoding. Includes information about any text encodings used.</returns>
        /// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
        public Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
        {
            string Json = JSON.Encode(Object, false);
            string ContentType = DefaultContentType;

            if (Encoding is null)
            {
                Encoding = Encoding.UTF8;
                ContentType += "; charset=utf-8";
            }
            else
                ContentType += "; charset=" + Encoding.WebName;

            return Task.FromResult(new ContentResponse(ContentType, Object, Encoding.GetBytes(Json)));
        }
    }
}