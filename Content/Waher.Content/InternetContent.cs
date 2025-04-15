using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Binary;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;

namespace Waher.Content
{
	/// <summary>
	/// Static class managing encoding and decoding of internet content.
	/// </summary>
	public static class InternetContent
	{
		/// <summary>
		/// ISO-8859-1 character encoding.
		/// </summary>
		public static readonly Encoding ISO_8859_1 = Encoding.GetEncoding("ISO-8859-1");

		private static string[] canEncodeContentTypes = null;
		private static string[] canEncodeFileExtensions = null;
		private static string[] canDecodeContentTypes = null;
		private static string[] canDecodeFileExtensions = null;
		private static string[] canGetUriSchemes = null;
		private static string[] canPostToUriSchemes = null;
		private static string[] canPutToUriSchemes = null;
		private static string[] canDeleteToUriSchemes = null;
		private static string[] canHeadUriSchemes = null;
		private static IContentEncoder[] encoders = null;
		private static IContentDecoder[] decoders = null;
		private static IContentConverter[] converters = null;
		private static IContentGetter[] getters = null;
		private static IContentPoster[] posters = null;
		private static IContentPutter[] putters = null;
		private static IContentDeleter[] deleters = null;
		private static IContentHeader[] headers = null;
		private readonly static Dictionary<string, KeyValuePair<Grade, IContentDecoder>> decoderByContentType =
			new Dictionary<string, KeyValuePair<Grade, IContentDecoder>>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, KeyValuePair<Grade, IContentEncoder>> encodersByType =
			new Dictionary<string, KeyValuePair<Grade, IContentEncoder>>();
		private readonly static Dictionary<string, string> contentTypeByFileExtensions = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, string> fileExtensionsByContentType = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, IContentConverter> convertersByStep = new Dictionary<string, IContentConverter>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, ChunkedList<IContentConverter>> convertersByFrom = new Dictionary<string, ChunkedList<IContentConverter>>();
		private readonly static Dictionary<string, IContentGetter[]> gettersByScheme = new Dictionary<string, IContentGetter[]>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, IContentPoster[]> postersByScheme = new Dictionary<string, IContentPoster[]>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, IContentPutter[]> puttersByScheme = new Dictionary<string, IContentPutter[]>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, IContentDeleter[]> deletersByScheme = new Dictionary<string, IContentDeleter[]>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, IContentHeader[]> headersByScheme = new Dictionary<string, IContentHeader[]>(StringComparer.CurrentCultureIgnoreCase);

		static InternetContent()
		{
			Types.OnInvalidated += Types_OnInvalidated;
		}

		private static void Types_OnInvalidated(object Sender, EventArgs e)
		{
			canEncodeContentTypes = null;
			canEncodeFileExtensions = null;
			canDecodeContentTypes = null;
			canDecodeFileExtensions = null;
			canGetUriSchemes = null;
			encoders = null;
			decoders = null;
			converters = null;
			getters = null;
			headers = null;
			posters = null;
			putters = null;
			deleters = null;

			lock (decoderByContentType)
			{
				decoderByContentType.Clear();
			}

			lock (encodersByType)
			{
				encodersByType.Clear();
			}

			lock (contentTypeByFileExtensions)
			{
				contentTypeByFileExtensions.Clear();
			}

			lock (fileExtensionsByContentType)
			{
				fileExtensionsByContentType.Clear();
			}

			lock (convertersByStep)
			{
				convertersByStep.Clear();
				convertersByFrom.Clear();
			}

			lock (gettersByScheme)
			{
				gettersByScheme.Clear();
			}

			lock (headersByScheme)
			{
				headersByScheme.Clear();
			}

			lock (postersByScheme)
			{
				postersByScheme.Clear();
			}

			lock (puttersByScheme)
			{
				puttersByScheme.Clear();
			}

			lock (deletersByScheme)
			{
				deletersByScheme.Clear();
			}
		}

		#region Encoding

		/// <summary>
		/// Internet content types that can be encoded.
		/// </summary>
		public static string[] CanEncodeContentTypes
		{
			get
			{
				if (canEncodeContentTypes is null)
				{
					SortedDictionary<string, bool> ContentTypes = new SortedDictionary<string, bool>();

					foreach (IContentEncoder Encoder in Encoders)
					{
						foreach (string ContentType in Encoder.ContentTypes)
							ContentTypes[ContentType] = true;
					}

					string[] Types = new string[ContentTypes.Count];
					ContentTypes.Keys.CopyTo(Types, 0);

					canEncodeContentTypes = Types;
				}

				return canEncodeContentTypes;
			}
		}

		/// <summary>
		/// File extensions that can be encoded.
		/// </summary>
		public static string[] CanEncodeFileExtensions
		{
			get
			{
				if (canEncodeFileExtensions is null)
				{
					SortedDictionary<string, bool> FileExtensions = new SortedDictionary<string, bool>();

					foreach (IContentEncoder Encoder in Encoders)
					{
						foreach (string FileExtension in Encoder.FileExtensions)
							FileExtensions[FileExtension] = true;
					}

					string[] Types = new string[FileExtensions.Count];
					FileExtensions.Keys.CopyTo(Types, 0);

					canEncodeFileExtensions = Types;
				}

				return canEncodeFileExtensions;
			}
		}

		/// <summary>
		/// Available Internet Content Encoders.
		/// </summary>
		public static IContentEncoder[] Encoders
		{
			get
			{
				if (encoders is null)
				{
					ChunkedList<IContentEncoder> Encoders = new ChunkedList<IContentEncoder>();
					IContentEncoder Encoder;
					Type[] EncoderTypes = Types.GetTypesImplementingInterface(typeof(IContentEncoder));

					foreach (Type T in EncoderTypes)
					{
						ConstructorInfo CI = Types.GetDefaultConstructor(T);
						if (CI is null)
							continue;

						try
						{
							Encoder = (IContentEncoder)CI.Invoke(Types.NoParameters);
						}
						catch (Exception)
						{
							continue;
						}

						Encoders.Add(Encoder);
					}

					encoders = Encoders.ToArray();
				}

				return encoders;
			}
		}

		/// <summary>
		/// If a given object can be encoded.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Grade">How well the object can be encoded.</param>
		/// <param name="Encoder">Best encoder for the object.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>If the object can be encoded.</returns>
		public static bool Encodes(object Object, out Grade Grade, out IContentEncoder Encoder, params string[] AcceptedContentTypes)
		{
			string Key;

			if (AcceptedContentTypes is null || AcceptedContentTypes.Length == 0)
				Key = Object.GetType().FullName;
			else
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(Object.GetType().FullName);

				foreach (string Accepted in AcceptedContentTypes)
				{
					sb.Append('|');
					sb.Append(Accepted);
				}

				Key = sb.ToString();
			}

			lock (encodersByType)
			{
				if (encodersByType.TryGetValue(Key, out KeyValuePair<Grade, IContentEncoder> P))
				{
					Grade = P.Key;
					Encoder = P.Value;

					if (!(Encoder is null))
						return true;

					if (Object is CustomEncoding)
					{
						Encoder = new CustomEncoder();
						return true;
					}

					return false;
				}
			}

			Grade = Grade.NotAtAll;
			Encoder = null;

			foreach (IContentEncoder Encoder2 in Encoders)
			{
				if (Encoder2.Encodes(Object, out Grade Grade2, AcceptedContentTypes) && Grade2 > Grade)
				{
					Grade = Grade2;
					Encoder = Encoder2;
				}
			}

			lock (encodersByType)
			{
				encodersByType[Key] = new KeyValuePair<Grade, IContentEncoder>(Grade, Encoder);
			}

			if (!(Encoder is null))
				return true;

			if (Object is CustomEncoding)
			{
				Encoder = new CustomEncoder();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Encodes an object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Encoding">Desired encoding of text. Can be null if no desired encoding is speified.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object.</returns>
		[Obsolete("Use the EncodeAsync method for more efficient processing of content including asynchronous processing elements in parallel environments.")]
		public static ContentResponse Encode(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			return EncodeAsync(Object, Encoding, AcceptedContentTypes).Result;
		}

		/// <summary>
		/// Encodes an object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Encoding">Desired encoding of text. Can be null if no desired encoding is speified.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object, and Content Type of encoding. Includes information about any text encodings used.</returns>
		public static Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			return EncodeAsync(Object, Encoding, null, AcceptedContentTypes);
		}

		/// <summary>
		/// Encodes an object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Encoding">Desired encoding of text. Can be null if no desired encoding is speified.</param>
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object, and Content Type of encoding. Includes information about any text encodings used.</returns>
		public static Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding,
			ICodecProgress Progress, params string[] AcceptedContentTypes)
		{
			if (!Encodes(Object, out Grade _, out IContentEncoder Encoder, AcceptedContentTypes))
				return Task.FromResult(new ContentResponse(new ArgumentException("No encoder found to encode objects of type " + (Object?.GetType()?.FullName) + ".", nameof(Object))));

			return Encoder.EncodeAsync(Object, Encoding, Progress);
		}

		/// <summary>
		/// Checks if a given content type is acceptable.
		/// </summary>
		/// <param name="ContentType">Content type.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		public static bool IsAccepted(string ContentType, params string[] AcceptedContentTypes)
		{
			if (AcceptedContentTypes.Length == 0)
				return true;
			else
				return Array.IndexOf(AcceptedContentTypes, ContentType) >= 0;
		}

		/// <summary>
		/// Checks if at least one content type in a set of content types is acceptable.
		/// </summary>
		/// <param name="ContentTypes">Content types.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		public static bool IsAccepted(string[] ContentTypes, params string[] AcceptedContentTypes)
		{
			return IsAccepted(ContentTypes, out string _, AcceptedContentTypes);
		}

		/// <summary>
		/// Checks if at least one content type in a set of content types is acceptable.
		/// </summary>
		/// <param name="ContentTypes">Content types.</param>
		/// <param name="ContentType">Content type selected as the first acceptable content type.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		public static bool IsAccepted(string[] ContentTypes, out string ContentType, params string[] AcceptedContentTypes)
		{
			if (ContentTypes.Length == 0)
			{
				ContentType = null;
				return false;
			}

			if ((AcceptedContentTypes?.Length ?? 0) == 0)
			{
				ContentType = ContentTypes[0];
				return true;
			}

			foreach (string ContentType2 in ContentTypes)
			{
				if (IsAccepted(ContentType2, AcceptedContentTypes))
				{
					ContentType = ContentType2;
					return true;
				}
			}

			ContentType = null;
			return false;
		}

		#endregion

		#region Decoding

		/// <summary>
		/// Internet content types that can be decoded.
		/// </summary>
		public static string[] CanDecodeContentTypes
		{
			get
			{
				if (canDecodeContentTypes is null)
				{
					SortedDictionary<string, bool> ContentTypes = new SortedDictionary<string, bool>();

					foreach (IContentDecoder Decoder in Decoders)
					{
						foreach (string ContentType in Decoder.ContentTypes)
							ContentTypes[ContentType] = true;
					}

					string[] Types = new string[ContentTypes.Count];
					ContentTypes.Keys.CopyTo(Types, 0);

					canDecodeContentTypes = Types;
				}

				return canDecodeContentTypes;
			}
		}

		/// <summary>
		/// File extensions that can be decoded.
		/// </summary>
		public static string[] CanDecodeFileExtensions
		{
			get
			{
				if (canDecodeFileExtensions is null)
				{
					SortedDictionary<string, bool> FileExtensions = new SortedDictionary<string, bool>();

					foreach (IContentDecoder Decoder in Decoders)
					{
						foreach (string FileExtension in Decoder.FileExtensions)
							FileExtensions[FileExtension] = true;
					}

					string[] Types = new string[FileExtensions.Count];
					FileExtensions.Keys.CopyTo(Types, 0);

					canDecodeFileExtensions = Types;
				}

				return canDecodeFileExtensions;
			}
		}

		/// <summary>
		/// Available Internet Content Decoders.
		/// </summary>
		public static IContentDecoder[] Decoders
		{
			get
			{
				if (decoders is null)
				{
					ChunkedList<IContentDecoder> Decoders = new ChunkedList<IContentDecoder>();
					IContentDecoder Decoder;
					Type[] DecoderTypes = Types.GetTypesImplementingInterface(typeof(IContentDecoder));

					foreach (Type T in DecoderTypes)
					{
						ConstructorInfo CI = Types.GetDefaultConstructor(T);
						if (CI is null)
							continue;

						try
						{
							Decoder = (IContentDecoder)CI.Invoke(Types.NoParameters);
						}
						catch (Exception)
						{
							continue;
						}

						Decoders.Add(Decoder);
					}

					decoders = Decoders.ToArray();
				}

				return decoders;
			}
		}

		/// <summary>
		/// If an object with a given content type can be decoded.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <param name="Decoder">Best decoder for the object.</param>
		/// <returns>If an object with the given type can be decoded.</returns>
		public static bool Decodes(string ContentType, out Grade Grade, out IContentDecoder Decoder)
		{
			lock (decoderByContentType)
			{
				if (decoderByContentType.TryGetValue(ContentType, out KeyValuePair<Grade, IContentDecoder> P))
				{
					Grade = P.Key;
					Decoder = P.Value;

					return !(Decoder is null);
				}
			}

			Grade = Grade.NotAtAll;
			Decoder = null;

			foreach (IContentDecoder Decoder2 in Decoders)
			{
				if (Decoder2.Decodes(ContentType, out Grade Grade2) && Grade2 > Grade)
				{
					Grade = Grade2;
					Decoder = Decoder2;
				}
			}

			lock (decoderByContentType)
			{
				decoderByContentType[ContentType] = new KeyValuePair<Grade, IContentDecoder>(Grade, Decoder);
			}

			return !(Decoder is null);
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
		[Obsolete("Use the DecoceAsync method for more efficient processing of content including asynchronous processing elements in parallel environments.")]
		public static ContentResponse Decode(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			return DecodeAsync(ContentType, Data, Encoding, Fields, BaseUri).Result;
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
		public static Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			return DecodeAsync(ContentType, Data, Encoding, Fields, BaseUri, null);
		}

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		/// <param name="Encoding">Any encoding specified. Can be null if no encoding specified.</param>
		/// <param name="Fields">Any content-type related fields and their corresponding values.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <returns>Decoded object.</returns>
		public static Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, 
			KeyValuePair<string, string>[] Fields, Uri BaseUri, ICodecProgress Progress)
		{
			if (!Decodes(ContentType, out Grade _, out IContentDecoder Decoder))
				return Task.FromResult(new ContentResponse(new ArgumentException("No decoder found to decode objects of type " + ContentType + ".", nameof(ContentType))));

			return Decoder.DecodeAsync(ContentType, Data, Encoding, Fields, BaseUri, Progress);
		}

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		[Obsolete("Use the DecoceAsync method for more efficient processing of content including asynchronous processing elements in parallel environments.")]
		public static ContentResponse Decode(string ContentType, byte[] Data, Uri BaseUri)
		{
			return DecodeAsync(ContentType, Data, BaseUri).Result;
		}

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		public static Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Uri BaseUri)
		{
			return DecodeAsync(ContentType, Data, BaseUri, null);
		}

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <returns>Decoded object.</returns>
		public static Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Uri BaseUri,
			ICodecProgress Progress)
		{
			Encoding Encoding = null;
			KeyValuePair<string, string>[] Fields;
			int i;

			i = ContentType.IndexOf(';');
			if (i > 0)
			{
				Fields = CommonTypes.ParseFieldValues(ContentType.Substring(i + 1).TrimStart());
				ContentType = ContentType.Substring(0, i).TrimEnd();

				foreach (KeyValuePair<string, string> Field in Fields)
				{
					if (string.Compare(Field.Key, "CHARSET", true) == 0)
						Encoding = GetEncoding(Field.Value);
				}
			}
			else
				Fields = Array.Empty<KeyValuePair<string, string>>();

			return DecodeAsync(ContentType, Data, Encoding, Fields, BaseUri, Progress);
		}

		/// <summary>
		/// Gets a character encoding from its name.
		/// </summary>
		/// <param name="CharacterSet">Name of character set.</param>
		/// <returns>Encoding.</returns>
		public static Encoding GetEncoding(string CharacterSet)
		{
			if (string.IsNullOrEmpty(CharacterSet))
				return null;

			// Reference: http://www.iana.org/assignments/character-sets/character-sets.xhtml
			switch (CharacterSet.ToUpper())
			{
				case "ASCII":
				case "US-ASCII":
					return Encoding.ASCII;

				case "UTF-16LE":
				case "UTF-16":
					return Encoding.Unicode;

				case "UTF-16BE":
					return Encoding.BigEndianUnicode;

				case "UTF-32":
				case "UTF-32LE":
					return Encoding.UTF32;

				case "UNICODE-1-1-UTF-7":
				case "UTF-7":
					return Encoding.UTF7;

				case "UTF-8":
					return Encoding.UTF8;

				default:
					return Encoding.GetEncoding(CharacterSet);
			}
		}

		#endregion

		#region File extensions

		/// <summary>
		/// Gets the content type of an item, given its file extension. It uses the <see cref="TryGetContentType"/> to see if any of the
		/// content encoders/decoders support content with the corresponding file type. If no such encoder/decoder is found, the generic
		/// application/octet-stream type is returned.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <returns>Content type.</returns>
		public static string GetContentType(string FileExtension)
		{
			if (TryGetContentType(FileExtension, out string ContentType))
				return ContentType;
			else
				return BinaryCodec.DefaultContentType;
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public static bool TryGetContentType(string FileExtension, out string ContentType)
		{
			FileExtension = FileExtension.ToLower();
			if (FileExtension.StartsWith("."))
				FileExtension = FileExtension.Substring(1);

			lock (contentTypeByFileExtensions)
			{
				if (contentTypeByFileExtensions.TryGetValue(FileExtension, out ContentType))
					return true;
			}

			foreach (IContentDecoder Decoder in Decoders)
			{
				if (Decoder.TryGetContentType(FileExtension, out ContentType))
				{
					lock (contentTypeByFileExtensions)
					{
						contentTypeByFileExtensions[FileExtension] = ContentType;
					}

					return true;
				}
			}

			foreach (IContentEncoder Encoder in Encoders)
			{
				if (Encoder.TryGetContentType(FileExtension, out ContentType))
				{
					lock (contentTypeByFileExtensions)
					{
						contentTypeByFileExtensions[FileExtension] = ContentType;
					}

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the file extension of an item, given its content type. It uses the <see cref="TryGetFileExtension"/> to see if any of the
		/// content encoders/decoders support content with the corresponding content type. If no such encoder/decoder is found, the generic
		/// bin extension is returned.
		/// </summary>
		/// <param name="ContentType">File Content-Type.</param>
		/// <returns>File extension.</returns>
		public static string GetFileExtension(string ContentType)
		{
			if (TryGetFileExtension(ContentType, out string FileExtension))
				return FileExtension;
			else
				return "bin";
		}

		/// <summary>
		/// Tries to get the file extension of an item, given its content type.
		/// </summary>
		/// <param name="ContentType">Content type.</param>
		/// <param name="FileExtension">File extension.</param>
		/// <returns>If the Content-Type was recognized.</returns>
		public static bool TryGetFileExtension(string ContentType, out string FileExtension)
		{
			ContentType = ContentType.ToLower();

			lock (fileExtensionsByContentType)
			{
				if (fileExtensionsByContentType.TryGetValue(ContentType, out FileExtension))
					return true;
			}

			foreach (IContentDecoder Decoder in Decoders)
			{
				if (Decoder.TryGetFileExtension(ContentType, out FileExtension))
				{
					lock (fileExtensionsByContentType)
					{
						fileExtensionsByContentType[ContentType] = FileExtension;
					}

					return true;
				}
			}

			foreach (IContentEncoder Encoder in Encoders)
			{
				if (Encoder.TryGetFileExtension(ContentType, out FileExtension))
				{
					lock (fileExtensionsByContentType)
					{
						fileExtensionsByContentType[ContentType] = FileExtension;
					}

					return true;
				}
			}

			return false;
		}

		#endregion

		#region Content Conversion


		/// <summary>
		/// Available Internet Content Converters.
		/// </summary>
		public static IContentConverter[] Converters
		{
			get
			{
				if (converters is null)
					FindConverters();

				return converters;
			}
		}

		/// <summary>
		/// Checks if it is possible to convert content from one type to another.
		/// 
		/// A shortest path algorithm maximizing conversion quality and shortening conversion distance is used to find sequences of converters, 
		/// if a direct conversion is not possible.
		/// </summary>
		/// <param name="FromContentType">Existing content type.</param>
		/// <param name="ToContentType">Desired content type.</param>
		/// <param name="Converter">Converter that transforms content from type <paramref name="FromContentType"/> to type
		/// <paramref name="ToContentType"/>.</param>
		/// <returns>If a converter was found.</returns>
		public static bool CanConvert(string FromContentType, string ToContentType, out IContentConverter Converter)
		{
			lock (convertersByStep)
			{
				if (converters is null)
					FindConverters();

				string PathKey = FromContentType + " -> " + ToContentType;
				if (convertersByStep.TryGetValue(PathKey, out Converter))
					return !(Converter is null);

				if (!convertersByFrom.TryGetValue(FromContentType, out ChunkedList<IContentConverter> Converters))
					return false;

				ChunkedList<ConversionStep> Queue = new ChunkedList<ConversionStep>();
				ConversionStep Step;

				foreach (IContentConverter C in Converters)
				{
					Step = new ConversionStep()
					{
						From = FromContentType,
						Converter = C,
						TotalGrade = C.ConversionGrade,
						Prev = null,
						Distance = 1
					};

					Queue.Add(Step);
				}

				Dictionary<string, ConversionStep> Possibilities = new Dictionary<string, ConversionStep>();
				ConversionStep Best = null;
				Grade BestGrade = Grade.NotAtAll;
				int BestDistance = int.MaxValue;
				Grade StepGrade;
				int StepDistance;
				bool First;

				while (Queue.HasFirstItem)
				{
					Step = Queue.RemoveFirst();

					StepDistance = Step.Distance + 1;
					StepGrade = Step.Converter.ConversionGrade;
					if (Step.TotalGrade < StepGrade)
						StepGrade = Step.TotalGrade;

					foreach (string To in Step.Converter.ToContentTypes)
					{
						if (string.Compare(To, ToContentType, true) == 0 || To == "*")
						{
							if (StepGrade > BestGrade || StepGrade == BestGrade && StepDistance < BestDistance)
							{
								Best = Step;
								BestGrade = StepGrade;
								BestDistance = StepDistance;
							}
						}
						else
						{
							if (Possibilities.TryGetValue(To, out ConversionStep NextStep) && NextStep.TotalGrade >= StepGrade && NextStep.Distance <= StepDistance)
								continue;

							if (!convertersByFrom.TryGetValue(To, out Converters))
								continue;

							First = true;
							foreach (IContentConverter C in Converters)
							{
								NextStep = new ConversionStep()
								{
									From = To,
									Converter = C,
									TotalGrade = StepGrade,
									Prev = Step,
									Distance = StepDistance
								};

								if (First)
								{
									Possibilities[To] = NextStep;
									First = false;
								}

								Queue.Add(NextStep);
							}
						}
					}
				}

				if (!(Best is null))
				{
					ChunkedList<KeyValuePair<string, IContentConverter>> List = new ChunkedList<KeyValuePair<string, IContentConverter>>();

					while (!(Best is null))
					{
						List.Insert(0, new KeyValuePair<string, IContentConverter>(Best.From, Best.Converter));
						Best = Best.Prev;
					}

					Converter = new ConversionSequence(FromContentType, ToContentType, BestGrade, List.ToArray());
					convertersByStep[PathKey] = Converter;
					return true;
				}
				else
				{
					convertersByStep[PathKey] = null;
					Converter = null;
					return false;
				}
			}
		}

		private class ConversionStep
		{
			public IContentConverter Converter;
			public Grade TotalGrade;
			public ConversionStep Prev;
			public string From;
			public int Distance;
		}

		private static void FindConverters()
		{
			ChunkedList<IContentConverter> Converters = new ChunkedList<IContentConverter>();
			IContentConverter Converter;
			Type[] ConverterTypes = Types.GetTypesImplementingInterface(typeof(IContentConverter));

			convertersByStep.Clear();
			convertersByFrom.Clear();

			lock (convertersByStep)
			{
				foreach (Type T in ConverterTypes)
				{
					ConstructorInfo CI = Types.GetDefaultConstructor(T);
					if (CI is null)
						continue;

					try
					{
						Converter = (IContentConverter)CI.Invoke(Types.NoParameters);
					}
					catch (Exception)
					{
						continue;
					}

					Converters.Add(Converter);

					foreach (string From in Converter.FromContentTypes)
					{
						if (!convertersByFrom.TryGetValue(From, out ChunkedList<IContentConverter> List))
						{
							List = new ChunkedList<IContentConverter>();
							convertersByFrom[From] = List;
						}

						List.Add(Converter);

						foreach (string To in Converter.ToContentTypes)
						{
							if (To != "*")
								convertersByStep[From + " -> " + To] = Converter;
						}
					}
				}
			}

			converters = Converters.ToArray();
		}

		/// <summary>
		/// Gets available converters that can convert content from a given type.
		/// </summary>
		/// <param name="FromContentType">From which content type converters have to convert.</param>
		/// <returns>Available converters, or null if there are none.</returns>
		public static IContentConverter[] GetConverters(string FromContentType)
		{
			lock (convertersByStep)
			{
				if (converters is null)
					FindConverters();

				if (!convertersByFrom.TryGetValue(FromContentType, out ChunkedList<IContentConverter> Converters))
					return null;

				return Converters.ToArray();
			}
		}

		#endregion

		#region Getting resources

		/// <summary>
		/// Internet URI Schemes that can be gotten.
		/// </summary>
		public static string[] CanGetUriSchemes
		{
			get
			{
				if (canGetUriSchemes is null)
				{
					SortedDictionary<string, bool> UriSchemes = new SortedDictionary<string, bool>();

					foreach (IContentGetter Getter in Getters)
					{
						foreach (string Scheme in Getter.UriSchemes)
							UriSchemes[Scheme] = true;
					}

					string[] Schemes = new string[UriSchemes.Count];
					UriSchemes.Keys.CopyTo(Schemes, 0);

					canGetUriSchemes = Schemes;
				}

				return canGetUriSchemes;
			}
		}

		/// <summary>
		/// Available Internet Content Getters.
		/// </summary>
		public static IContentGetter[] Getters
		{
			get
			{
				if (getters is null)
					BuildGetters();

				return getters;
			}
		}

		private static void BuildGetters()
		{
			ChunkedList<IContentGetter> Getters = new ChunkedList<IContentGetter>();
			Type[] GetterTypes = Types.GetTypesImplementingInterface(typeof(IContentGetter));
			Dictionary<string, ChunkedList<IContentGetter>> ByScheme = new Dictionary<string, ChunkedList<IContentGetter>>();
			IContentGetter Getter;

			foreach (Type T in GetterTypes)
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					Getter = (IContentGetter)CI.Invoke(Types.NoParameters);
				}
				catch (Exception)
				{
					continue;
				}

				Getters.Add(Getter);

				foreach (string Schema in Getter.UriSchemes)
				{
					if (!ByScheme.TryGetValue(Schema, out ChunkedList<IContentGetter> List))
					{
						List = new ChunkedList<IContentGetter>();
						ByScheme[Schema] = List;
					}

					List.Add(Getter);
				}
			}

			lock (gettersByScheme)
			{
				foreach (KeyValuePair<string, ChunkedList<IContentGetter>> P in ByScheme)
					gettersByScheme[P.Key] = P.Value.ToArray();
			}

			getters = Getters.ToArray();
		}

		/// <summary>
		/// If a resource can be gotten, given its URI.
		/// </summary>
		/// <param name="Uri">URI of resource.</param>
		/// <param name="Grade">How well the getter can get the resource.</param>
		/// <param name="Getter">Best getter for the URI.</param>
		/// <returns>If a resource with the given URI can be gotten.</returns>
		public static bool CanGet(Uri Uri, out Grade Grade, out IContentGetter Getter)
		{
			if (Uri is null)
			{
				Grade = Grade.NotAtAll;
				Getter = null;
				return false;
			}

			if (getters is null)
				BuildGetters();

			IContentGetter[] Getters;

			lock (gettersByScheme)
			{
				if (Uri is null || !gettersByScheme.TryGetValue(Uri.Scheme, out Getters))
				{
					Getter = null;
					Grade = Grade.NotAtAll;
					return false;
				}
			}

			Grade = Grade.NotAtAll;
			Getter = null;

			foreach (IContentGetter Getter2 in Getters)
			{
				if (Getter2.CanGet(Uri, out Grade Grade2) && Grade2 > Grade)
				{
					Grade = Grade2;
					Getter = Getter2;
				}
			}

			return !(Getter is null);
		}

		/// <summary>
		/// Gets a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> GetAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			return GetAsync(Uri, null, null, Headers);
		}

		/// <summary>
		/// Gets a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> GetAsync(Uri Uri, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return GetAsync(Uri, Certificate, null, Headers);
		}

		/// <summary>
		/// Gets a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> GetAsync(Uri Uri, X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, 
			params KeyValuePair<string, string>[] Headers)
		{
			if (!CanGet(Uri, out Grade _, out IContentGetter Getter))
				return Task.FromResult(new ContentResponse(new ArgumentException("URI Scheme not recognized (GET): " + Uri.Scheme, nameof(Uri))));

			return Getter.GetAsync(Uri, Certificate, RemoteCertificateValidator, Headers);
		}

		/// <summary>
		/// Gets a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> GetAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return GetAsync(Uri, null, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Gets a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> GetAsync(Uri Uri, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return GetAsync(Uri, Certificate, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Gets a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> GetAsync(Uri Uri, X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, 
			int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanGet(Uri, out Grade _, out IContentGetter Getter))
				return Task.FromResult(new ContentResponse(new ArgumentException("URI Scheme not recognized (GET): " + Uri.Scheme, nameof(Uri))));

			return Getter.GetAsync(Uri, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			return GetTempStreamAsync(Uri, null, null, null, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Destination">Optional destination. Content will be output to this stream. If not provided, a new temporary stream will be created.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, TemporaryStream Destination, params KeyValuePair<string, string>[] Headers)
		{
			return GetTempStreamAsync(Uri, null, null, Destination, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return GetTempStreamAsync(Uri, Certificate, null, null, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Destination">Optional destination. Content will be output to this stream. If not provided, a new temporary stream will be created.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate, TemporaryStream Destination, params KeyValuePair<string, string>[] Headers)
		{
			return GetTempStreamAsync(Uri, Certificate, null, Destination, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanGet(Uri, out Grade _, out IContentGetter Getter))
				return Task.FromResult(new ContentStreamResponse(new ArgumentException("URI Scheme not recognized (GET): " + Uri.Scheme, nameof(Uri))));

			return Getter.GetTempStreamAsync(Uri, Certificate, RemoteCertificateValidator, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Destination">Optional destination. Content will be output to this stream. If not provided, a new temporary stream will be created.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, TemporaryStream Destination, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanGet(Uri, out Grade _, out IContentGetter Getter))
				return Task.FromResult(new ContentStreamResponse(new ArgumentException("URI Scheme not recognized (GET): " + Uri.Scheme, nameof(Uri))));

			return Getter.GetTempStreamAsync(Uri, Certificate, RemoteCertificateValidator, Destination, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return GetTempStreamAsync(Uri, null, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Destination">Optional destination. Content will be output to this stream. If not provided, a new temporary stream will be created.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, int TimeoutMs, TemporaryStream Destination, params KeyValuePair<string, string>[] Headers)
		{
			return GetTempStreamAsync(Uri, null, null, TimeoutMs, Destination, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return GetTempStreamAsync(Uri, Certificate, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Destination">Optional destination. Content will be output to this stream. If not provided, a new temporary stream will be created.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate, int TimeoutMs,
			TemporaryStream Destination, params KeyValuePair<string, string>[] Headers)
		{
			return GetTempStreamAsync(Uri, Certificate, null, TimeoutMs, Destination, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanGet(Uri, out Grade _, out IContentGetter Getter))
				return Task.FromResult(new ContentStreamResponse(new ArgumentException("URI Scheme not recognized (GET): " + Uri.Scheme, nameof(Uri))));

			return Getter.GetTempStreamAsync(Uri, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Destination">Optional destination. Content will be output to this stream. If not provided, a new temporary stream will be created.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, TemporaryStream Destination, 
			params KeyValuePair<string, string>[] Headers)
		{
			if (!CanGet(Uri, out Grade _, out IContentGetter Getter))
				return Task.FromResult(new ContentStreamResponse(new ArgumentException("URI Scheme not recognized (GET): " + Uri.Scheme, nameof(Uri))));

			return Getter.GetTempStreamAsync(Uri, Certificate, RemoteCertificateValidator, TimeoutMs, Destination, Headers);
		}

		#endregion

		#region Posting to resources

		/// <summary>
		/// Internet URI Schemes that can be posted to.
		/// </summary>
		public static string[] CanPostToUriSchemes
		{
			get
			{
				if (canPostToUriSchemes is null)
				{
					SortedDictionary<string, bool> UriSchemes = new SortedDictionary<string, bool>();

					foreach (IContentPoster Poster in Posters)
					{
						foreach (string Scheme in Poster.UriSchemes)
							UriSchemes[Scheme] = true;
					}

					string[] Schemes = new string[UriSchemes.Count];
					UriSchemes.Keys.CopyTo(Schemes, 0);

					canPostToUriSchemes = Schemes;
				}

				return canPostToUriSchemes;
			}
		}

		/// <summary>
		/// Available Internet Content Posters.
		/// </summary>
		public static IContentPoster[] Posters
		{
			get
			{
				if (posters is null)
					BuildPosters();

				return posters;
			}
		}

		private static void BuildPosters()
		{
			ChunkedList<IContentPoster> Posters = new ChunkedList<IContentPoster>();
			Type[] PosterTypes = Types.GetTypesImplementingInterface(typeof(IContentPoster));
			Dictionary<string, ChunkedList<IContentPoster>> ByScheme = new Dictionary<string, ChunkedList<IContentPoster>>();
			IContentPoster Poster;

			foreach (Type T in PosterTypes)
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					Poster = (IContentPoster)CI.Invoke(Types.NoParameters);
				}
				catch (Exception)
				{
					continue;
				}

				Posters.Add(Poster);

				foreach (string Schema in Poster.UriSchemes)
				{
					if (!ByScheme.TryGetValue(Schema, out ChunkedList<IContentPoster> List))
					{
						List = new ChunkedList<IContentPoster>();
						ByScheme[Schema] = List;
					}

					List.Add(Poster);
				}
			}

			lock (postersByScheme)
			{
				foreach (KeyValuePair<string, ChunkedList<IContentPoster>> P in ByScheme)
					postersByScheme[P.Key] = P.Value.ToArray();
			}

			posters = Posters.ToArray();
		}

		/// <summary>
		/// If a resource can be posted to, given its URI.
		/// </summary>
		/// <param name="Uri">URI of resource.</param>
		/// <param name="Grade">How well the posted can post to the resource.</param>
		/// <param name="Poster">Best poster for the URI.</param>
		/// <returns>If a resource with the given URI can be posted to.</returns>
		public static bool CanPost(Uri Uri, out Grade Grade, out IContentPoster Poster)
		{
			if (Uri is null)
			{
				Grade = Grade.NotAtAll;
				Poster = null;
				return false;
			}

			if (posters is null)
				BuildPosters();

			IContentPoster[] Posters;

			lock (postersByScheme)
			{
				if (Uri is null || !postersByScheme.TryGetValue(Uri.Scheme, out Posters))
				{
					Poster = null;
					Grade = Grade.NotAtAll;
					return false;
				}
			}

			Grade = Grade.NotAtAll;
			Poster = null;

			foreach (IContentPoster Poster2 in Posters)
			{
				if (Poster2.CanPost(Uri, out Grade Grade2) && Grade2 > Grade)
				{
					Grade = Grade2;
					Poster = Poster2;
				}
			}

			return !(Poster is null);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PostAsync(Uri Uri, object Data, params KeyValuePair<string, string>[] Headers)
		{
			return PostAsync(Uri, Data, null, null, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PostAsync(Uri Uri, object Data, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return PostAsync(Uri, Data, Certificate, null, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PostAsync(Uri Uri, object Data, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanPost(Uri, out Grade _, out IContentPoster Poster))
				return Task.FromResult(new ContentResponse(new ArgumentException("URI Scheme not recognized (POST): " + Uri.Scheme, nameof(Uri))));

			return Poster.PostAsync(Uri, Data, Certificate, RemoteCertificateValidator, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PostAsync(Uri Uri, object Data, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return PostAsync(Uri, Data, null, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PostAsync(Uri Uri, object Data, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return PostAsync(Uri, Data, Certificate, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PostAsync(Uri Uri, object Data, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanPost(Uri, out Grade _, out IContentPoster Poster))
				return Task.FromResult(new ContentResponse(new ArgumentException("URI Scheme not recognized (POST): " + Uri.Scheme, nameof(Uri))));

			return Poster.PostAsync(Uri, Data, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, params KeyValuePair<string, string>[] Headers)
		{
			return PostAsync(Uri, EncodedData, ContentType, null, null, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PostAsync(Uri Uri, byte[] EncodedData, string ContentType,
			X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return PostAsync(Uri, EncodedData, ContentType, Certificate, null, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PostAsync(Uri Uri, byte[] EncodedData, string ContentType,
			X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanPost(Uri, out Grade _, out IContentPoster Poster))
				return Task.FromResult(new ContentBinaryResponse(new ArgumentException("URI Scheme not recognized (POST): " + Uri.Scheme, nameof(Uri))));

			return Poster.PostAsync(Uri, EncodedData, ContentType, Certificate, RemoteCertificateValidator, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return PostAsync(Uri, EncodedData, ContentType, null, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PostAsync(Uri Uri, byte[] EncodedData, string ContentType,
			X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return PostAsync(Uri, EncodedData, ContentType, Certificate, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, 
			X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, 
			params KeyValuePair<string, string>[] Headers)
		{
			if (!CanPost(Uri, out Grade _, out IContentPoster Poster))
				return Task.FromResult(new ContentBinaryResponse(new ArgumentException("URI Scheme not recognized (POST): " + Uri.Scheme, nameof(Uri))));

			return Poster.PostAsync(Uri, EncodedData, ContentType, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);
		}

		#endregion

		#region Putting to resources

		/// <summary>
		/// Internet URI Schemes that can be putted to.
		/// </summary>
		public static string[] CanPutToUriSchemes
		{
			get
			{
				if (canPutToUriSchemes is null)
				{
					SortedDictionary<string, bool> UriSchemes = new SortedDictionary<string, bool>();

					foreach (IContentPutter Putter in Putters)
					{
						foreach (string Scheme in Putter.UriSchemes)
							UriSchemes[Scheme] = true;
					}

					string[] Schemes = new string[UriSchemes.Count];
					UriSchemes.Keys.CopyTo(Schemes, 0);

					canPutToUriSchemes = Schemes;
				}

				return canPutToUriSchemes;
			}
		}

		/// <summary>
		/// Available Internet Content Putters.
		/// </summary>
		public static IContentPutter[] Putters
		{
			get
			{
				if (putters is null)
					BuildPutters();

				return putters;
			}
		}

		private static void BuildPutters()
		{
			ChunkedList<IContentPutter> Putters = new ChunkedList<IContentPutter>();
			Type[] PutterTypes = Types.GetTypesImplementingInterface(typeof(IContentPutter));
			Dictionary<string, ChunkedList<IContentPutter>> ByScheme = new Dictionary<string, ChunkedList<IContentPutter>>();
			IContentPutter Putter;

			foreach (Type T in PutterTypes)
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					Putter = (IContentPutter)CI.Invoke(Types.NoParameters);
				}
				catch (Exception)
				{
					continue;
				}

				Putters.Add(Putter);

				foreach (string Schema in Putter.UriSchemes)
				{
					if (!ByScheme.TryGetValue(Schema, out ChunkedList<IContentPutter> List))
					{
						List = new ChunkedList<IContentPutter>();
						ByScheme[Schema] = List;
					}

					List.Add(Putter);
				}
			}

			lock (puttersByScheme)
			{
				foreach (KeyValuePair<string, ChunkedList<IContentPutter>> P in ByScheme)
					puttersByScheme[P.Key] = P.Value.ToArray();
			}

			putters = Putters.ToArray();
		}

		/// <summary>
		/// If a resource can be putted to, given its URI.
		/// </summary>
		/// <param name="Uri">URI of resource.</param>
		/// <param name="Grade">How well the putted can put to the resource.</param>
		/// <param name="Putter">Best putter for the URI.</param>
		/// <returns>If a resource with the given URI can be putted to.</returns>
		public static bool CanPut(Uri Uri, out Grade Grade, out IContentPutter Putter)
		{
			if (Uri is null)
			{
				Grade = Grade.NotAtAll;
				Putter = null;
				return false;
			}

			if (putters is null)
				BuildPutters();

			IContentPutter[] Putters;

			lock (puttersByScheme)
			{
				if (Uri is null || !puttersByScheme.TryGetValue(Uri.Scheme, out Putters))
				{
					Putter = null;
					Grade = Grade.NotAtAll;
					return false;
				}
			}

			Grade = Grade.NotAtAll;
			Putter = null;

			foreach (IContentPutter Putter2 in Putters)
			{
				if (Putter2.CanPut(Uri, out Grade Grade2) && Grade2 > Grade)
				{
					Grade = Grade2;
					Putter = Putter2;
				}
			}

			return !(Putter is null);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to put.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PutAsync(Uri Uri, object Data, params KeyValuePair<string, string>[] Headers)
		{
			return PutAsync(Uri, Data, null, null, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to put.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PutAsync(Uri Uri, object Data, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return PutAsync(Uri, Data, Certificate, null, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to put.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PutAsync(Uri Uri, object Data, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanPut(Uri, out Grade _, out IContentPutter Putter))
				return Task.FromResult(new ContentResponse(new ArgumentException("URI Scheme not recognized (PUT): " + Uri.Scheme, nameof(Uri))));

			return Putter.PutAsync(Uri, Data, Certificate, RemoteCertificateValidator, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to put.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PutAsync(Uri Uri, object Data, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return PutAsync(Uri, Data, null, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to put.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PutAsync(Uri Uri, object Data, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return PutAsync(Uri, Data, Certificate, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to put.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> PutAsync(Uri Uri, object Data, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanPut(Uri, out Grade _, out IContentPutter Putter))
				return Task.FromResult(new ContentResponse(new ArgumentException("URI Scheme not recognized (PUT): " + Uri.Scheme, nameof(Uri))));

			return Putter.PutAsync(Uri, Data, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be putted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, params KeyValuePair<string, string>[] Headers)
		{
			return PutAsync(Uri, EncodedData, ContentType, null, null, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be putted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return PutAsync(Uri, EncodedData, ContentType, Certificate, null, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be putted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, 
			X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanPut(Uri, out Grade _, out IContentPutter Putter))
				return Task.FromResult(new ContentBinaryResponse(new ArgumentException("URI Scheme not recognized (PUT): " + Uri.Scheme, nameof(Uri))));

			return Putter.PutAsync(Uri, EncodedData, ContentType, Certificate, RemoteCertificateValidator, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be putted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return PutAsync(Uri, EncodedData, ContentType, null, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be putted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return PutAsync(Uri, EncodedData, ContentType, Certificate, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be putted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public static Task<ContentBinaryResponse> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, 
			X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanPut(Uri, out Grade _, out IContentPutter Putter))
				return Task.FromResult(new ContentBinaryResponse(new ArgumentException("URI Scheme not recognized (PUT): " + Uri.Scheme, nameof(Uri))));

			return Putter.PutAsync(Uri, EncodedData, ContentType, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);
		}

		#endregion

		#region Deleting to resources

		/// <summary>
		/// Internet URI Schemes that can be deleted to.
		/// </summary>
		public static string[] CanDeleteToUriSchemes
		{
			get
			{
				if (canDeleteToUriSchemes is null)
				{
					SortedDictionary<string, bool> UriSchemes = new SortedDictionary<string, bool>();

					foreach (IContentDeleter Deleter in Deleters)
					{
						foreach (string Scheme in Deleter.UriSchemes)
							UriSchemes[Scheme] = true;
					}

					string[] Schemes = new string[UriSchemes.Count];
					UriSchemes.Keys.CopyTo(Schemes, 0);

					canDeleteToUriSchemes = Schemes;
				}

				return canDeleteToUriSchemes;
			}
		}

		/// <summary>
		/// Available Internet Content Deleters.
		/// </summary>
		public static IContentDeleter[] Deleters
		{
			get
			{
				if (deleters is null)
					BuildDeleters();

				return deleters;
			}
		}

		private static void BuildDeleters()
		{
			ChunkedList<IContentDeleter> Deleters = new ChunkedList<IContentDeleter>();
			Type[] DeleterTypes = Types.GetTypesImplementingInterface(typeof(IContentDeleter));
			Dictionary<string, ChunkedList<IContentDeleter>> ByScheme = new Dictionary<string, ChunkedList<IContentDeleter>>();
			IContentDeleter Deleter;

			foreach (Type T in DeleterTypes)
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					Deleter = (IContentDeleter)CI.Invoke(Types.NoParameters);
				}
				catch (Exception)
				{
					continue;
				}

				Deleters.Add(Deleter);

				foreach (string Schema in Deleter.UriSchemes)
				{
					if (!ByScheme.TryGetValue(Schema, out ChunkedList<IContentDeleter> List))
					{
						List = new ChunkedList<IContentDeleter>();
						ByScheme[Schema] = List;
					}

					List.Add(Deleter);
				}
			}

			lock (deletersByScheme)
			{
				foreach (KeyValuePair<string, ChunkedList<IContentDeleter>> P in ByScheme)
					deletersByScheme[P.Key] = P.Value.ToArray();
			}

			deleters = Deleters.ToArray();
		}

		/// <summary>
		/// If a resource can be deleted to, given its URI.
		/// </summary>
		/// <param name="Uri">URI of resource.</param>
		/// <param name="Grade">How well the deleted can delete to the resource.</param>
		/// <param name="Deleter">Best deleter for the URI.</param>
		/// <returns>If a resource with the given URI can be deleted to.</returns>
		public static bool CanDelete(Uri Uri, out Grade Grade, out IContentDeleter Deleter)
		{
			if (Uri is null)
			{
				Grade = Grade.NotAtAll;
				Deleter = null;
				return false;
			}

			if (deleters is null)
				BuildDeleters();

			IContentDeleter[] Deleters;

			lock (deletersByScheme)
			{
				if (Uri is null || !deletersByScheme.TryGetValue(Uri.Scheme, out Deleters))
				{
					Deleter = null;
					Grade = Grade.NotAtAll;
					return false;
				}
			}

			Grade = Grade.NotAtAll;
			Deleter = null;

			foreach (IContentDeleter Deleter2 in Deleters)
			{
				if (Deleter2.CanDelete(Uri, out Grade Grade2) && Grade2 > Grade)
				{
					Grade = Grade2;
					Deleter = Deleter2;
				}
			}

			return !(Deleter is null);
		}

		/// <summary>
		/// Deletes a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> DeleteAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			return DeleteAsync(Uri, null, null, Headers);
		}

		/// <summary>
		/// Deletes a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> DeleteAsync(Uri Uri, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return DeleteAsync(Uri, Certificate, null, Headers);
		}

		/// <summary>
		/// Deletes a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> DeleteAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanDelete(Uri, out Grade _, out IContentDeleter Deleter))
				return Task.FromResult(new ContentResponse(new ArgumentException("URI Scheme not recognized (DELETE): " + Uri.Scheme, nameof(Uri))));

			return Deleter.DeleteAsync(Uri, Certificate, RemoteCertificateValidator, Headers);
		}

		/// <summary>
		/// Deletes a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> DeleteAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return DeleteAsync(Uri, null, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Deletes a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> DeleteAsync(Uri Uri, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return DeleteAsync(Uri, Certificate, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Deletes a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public static Task<ContentResponse> DeleteAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs,
			params KeyValuePair<string, string>[] Headers)
		{
			if (!CanDelete(Uri, out Grade _, out IContentDeleter Deleter))
				return Task.FromResult(new ContentResponse(new ArgumentException("URI Scheme not recognized (DELETE): " + Uri.Scheme, nameof(Uri))));

			return Deleter.DeleteAsync(Uri, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);
		}

		#endregion

		#region Getting headers of resources

		/// <summary>
		/// Internet URI Schemes where it is possible to get headers.
		/// </summary>
		public static string[] CanHeadUriSchemes
		{
			get
			{
				if (canHeadUriSchemes is null)
				{
					SortedDictionary<string, bool> UriSchemes = new SortedDictionary<string, bool>();

					foreach (IContentHeader Header in Headers)
					{
						foreach (string Scheme in Header.UriSchemes)
							UriSchemes[Scheme] = true;
					}

					string[] Schemes = new string[UriSchemes.Count];
					UriSchemes.Keys.CopyTo(Schemes, 0);

					canHeadUriSchemes = Schemes;
				}

				return canHeadUriSchemes;
			}
		}

		/// <summary>
		/// Available Internet Content Header-retrievers.
		/// </summary>
		public static IContentHeader[] Headers
		{
			get
			{
				if (headers is null)
					BuildHeaders();

				return headers;
			}
		}

		private static void BuildHeaders()
		{
			ChunkedList<IContentHeader> Headers = new ChunkedList<IContentHeader>();
			Type[] HeaderTypes = Types.GetTypesImplementingInterface(typeof(IContentHeader));
			Dictionary<string, ChunkedList<IContentHeader>> ByScheme = new Dictionary<string, ChunkedList<IContentHeader>>();
			IContentHeader Header;

			foreach (Type T in HeaderTypes)
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					Header = (IContentHeader)CI.Invoke(Types.NoParameters);
				}
				catch (Exception)
				{
					continue;
				}

				Headers.Add(Header);

				foreach (string Schema in Header.UriSchemes)
				{
					if (!ByScheme.TryGetValue(Schema, out ChunkedList<IContentHeader> List))
					{
						List = new ChunkedList<IContentHeader>();
						ByScheme[Schema] = List;
					}

					List.Add(Header);
				}
			}

			lock (headersByScheme)
			{
				foreach (KeyValuePair<string, ChunkedList<IContentHeader>> P in ByScheme)
					headersByScheme[P.Key] = P.Value.ToArray();
			}

			headers = Headers.ToArray();
		}

		/// <summary>
		/// If the headers of a resource can be gotten, given its URI.
		/// </summary>
		/// <param name="Uri">URI of resource.</param>
		/// <param name="Grade">How well the headers of the resource can be retrieved.</param>
		/// <param name="Header">Best header-retriever for the URI.</param>
		/// <returns>If the headers of a resource with the given URI can be retrieved.</returns>
		public static bool CanHead(Uri Uri, out Grade Grade, out IContentHeader Header)
		{
			if (Uri is null)
			{
				Grade = Grade.NotAtAll;
				Header = null;
				return false;
			}

			if (headers is null)
				BuildHeaders();

			IContentHeader[] Headers;

			lock (headersByScheme)
			{
				if (Uri is null || !headersByScheme.TryGetValue(Uri.Scheme, out Headers))
				{
					Header = null;
					Grade = Grade.NotAtAll;
					return false;
				}
			}

			Grade = Grade.NotAtAll;
			Header = null;

			foreach (IContentHeader Header2 in Headers)
			{
				if (Header2.CanHead(Uri, out Grade Grade2) && Grade2 > Grade)
				{
					Grade = Grade2;
					Header = Header2;
				}
			}

			return !(Header is null);
		}

		/// <summary>
		/// Heads a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> HeadAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			return HeadAsync(Uri, null, null, Headers);
		}

		/// <summary>
		/// Heads a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> HeadAsync(Uri Uri, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{ 
			return HeadAsync(Uri, Certificate, null, Headers);
		}

		/// <summary>
		/// Heads a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> HeadAsync(Uri Uri, X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, 
			params KeyValuePair<string, string>[] Headers)
		{
			if (!CanHead(Uri, out Grade _, out IContentHeader Header))
				return Task.FromResult(new ContentResponse(new ArgumentException("URI Scheme not recognized (HEAD): " + Uri.Scheme, nameof(Uri))));

			return Header.HeadAsync(Uri, Certificate, RemoteCertificateValidator, Headers);
		}

		/// <summary>
		/// Heads a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> HeadAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return HeadAsync(Uri, null, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Heads a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> HeadAsync(Uri Uri, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return HeadAsync(Uri, Certificate, null, TimeoutMs, Headers);
		}

		/// <summary>
		/// Heads a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<ContentResponse> HeadAsync(Uri Uri, X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, 
			int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanHead(Uri, out Grade _, out IContentHeader Header))
				return Task.FromResult(new ContentResponse(new ArgumentException("URI Scheme not recognized (HEAD): " + Uri.Scheme, nameof(Uri))));

			return Header.HeadAsync(Uri, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);
		}

		#endregion
	}
}
