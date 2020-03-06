using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

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
		private static IContentEncoder[] encoders = null;
		private static IContentDecoder[] decoders = null;
		private static IContentConverter[] converters = null;
		private static IContentGetter[] getters = null;
		private readonly static Dictionary<string, KeyValuePair<Grade, IContentDecoder>> decoderByContentType =
			new Dictionary<string, KeyValuePair<Grade, IContentDecoder>>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, string> contentTypeByFileExtensions = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, IContentConverter> convertersByStep = new Dictionary<string, IContentConverter>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static Dictionary<string, List<IContentConverter>> convertersByFrom = new Dictionary<string, List<IContentConverter>>();
		private readonly static Dictionary<string, IContentGetter[]> gettersByScheme = new Dictionary<string, IContentGetter[]>(StringComparer.CurrentCultureIgnoreCase);

		static InternetContent()
		{
			Types.OnInvalidated += Types_OnInvalidated;
		}

		private static void Types_OnInvalidated(object sender, EventArgs e)
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

			lock (decoderByContentType)
			{
				decoderByContentType.Clear();
			}

			lock (contentTypeByFileExtensions)
			{
				contentTypeByFileExtensions.Clear();
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
					List<IContentEncoder> Encoders = new List<IContentEncoder>();
					IContentEncoder Encoder;
					Type[] EncoderTypes = Types.GetTypesImplementingInterface(typeof(IContentEncoder));
					TypeInfo TI;

					foreach (Type T in EncoderTypes)
					{
						TI = T.GetTypeInfo();
						if (TI.IsAbstract || TI.IsGenericTypeDefinition)
							continue;

						try
						{
							Encoder = (IContentEncoder)Activator.CreateInstance(T);
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

			return Encoder != null;
		}

		/// <summary>
		/// Encodes an object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Encoding">Desired encoding of text. Can be null if no desired encoding is speified.</param>
		/// <param name="ContentType">Content Type of encoding. Includes information about any text encodings used.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
		public static byte[] Encode(object Object, Encoding Encoding, out string ContentType, params string[] AcceptedContentTypes)
		{
			if (!Encodes(Object, out Grade _, out IContentEncoder Encoder, AcceptedContentTypes))
				throw new ArgumentException("No encoder found to encode objects of type " + (Object?.GetType()?.FullName) + ".", nameof(Object));

			return Encoder.Encode(Object, Encoding, out ContentType);
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
				return Array.IndexOf<string>(AcceptedContentTypes, ContentType) >= 0;
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
				throw new ArgumentException("Empty list of content types not permitted.", nameof(ContentTypes));

			if (AcceptedContentTypes.Length == 0)
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
					List<IContentDecoder> Decoders = new List<IContentDecoder>();
					IContentDecoder Decoder;
					Type[] DecoderTypes = Types.GetTypesImplementingInterface(typeof(IContentDecoder));
					TypeInfo TI;

					foreach (Type T in DecoderTypes)
					{
						TI = T.GetTypeInfo();
						if (TI.IsAbstract || TI.IsGenericTypeDefinition)
							continue;

						try
						{
							Decoder = (IContentDecoder)Activator.CreateInstance(T);
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

					return Decoder != null;
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

			return Decoder != null;
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
		public static object Decode(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			if (!Decodes(ContentType, out Grade _, out IContentDecoder Decoder))
				throw new ArgumentException("No decoder found to decode objects of type " + ContentType + ".", nameof(ContentType));

			return Decoder.Decode(ContentType, Data, Encoding, Fields, BaseUri);
		}

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public static object Decode(string ContentType, byte[] Data, Uri BaseUri)
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
					if (Field.Key.ToUpper() == "CHARSET")
						Encoding = GetEncoding(Field.Value);
				}
			}
			else
				Fields = new KeyValuePair<string, string>[0];

			return Decode(ContentType, Data, Encoding, Fields, BaseUri);
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
				return "application/octet-stream";
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
					return Converter != null;

				if (!convertersByFrom.TryGetValue(FromContentType, out List<IContentConverter> Converters))
					return false;

				LinkedList<ConversionStep> Queue = new LinkedList<ConversionStep>();
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

					Queue.AddLast(Step);
				}

				Dictionary<string, ConversionStep> Possibilities = new Dictionary<string, ConversionStep>();
				ConversionStep Best = null;
				Grade BestGrade = Grade.NotAtAll;
				int BestDistance = int.MaxValue;
				Grade StepGrade;
				int StepDistance;
				bool First;

				while (Queue.First != null)
				{
					Step = Queue.First.Value;
					Queue.RemoveFirst();

					StepDistance = Step.Distance + 1;
					StepGrade = Step.Converter.ConversionGrade;
					if (Step.TotalGrade < StepGrade)
						StepGrade = Step.TotalGrade;

					foreach (string To in Step.Converter.ToContentTypes)
					{
						if (string.Compare(To, ToContentType, true) == 0)
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
								if (First)
								{
									Possibilities[To] = NextStep;
									First = false;
								}

								NextStep = new ConversionStep()
								{
									From = To,
									Converter = C,
									TotalGrade = StepGrade,
									Prev = Step,
									Distance = StepDistance
								};

								Queue.AddLast(NextStep);
							}
						}
					}
				}

				if (Best != null)
				{
					List<KeyValuePair<string, IContentConverter>> List = new List<KeyValuePair<string, IContentConverter>>();

					while (Best != null)
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
			List<IContentConverter> Converters = new List<IContentConverter>();
			IContentConverter Converter;
			Type[] ConverterTypes = Types.GetTypesImplementingInterface(typeof(IContentConverter));
			TypeInfo TI;

			convertersByStep.Clear();
			convertersByFrom.Clear();

			lock (convertersByStep)
			{
				foreach (Type T in ConverterTypes)
				{
					TI = T.GetTypeInfo();
					if (TI.IsAbstract || TI.IsGenericTypeDefinition)
						continue;

					try
					{
						Converter = (IContentConverter)Activator.CreateInstance(T);
					}
					catch (Exception)
					{
						continue;
					}

					Converters.Add(Converter);

					foreach (string From in Converter.FromContentTypes)
					{
						if (!convertersByFrom.TryGetValue(From, out List<IContentConverter> List))
						{
							List = new List<IContentConverter>();
							convertersByFrom[From] = List;
						}

						List.Add(Converter);

						foreach (string To in Converter.ToContentTypes)
						{
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

				if (!convertersByFrom.TryGetValue(FromContentType, out List<IContentConverter> Converters))
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
			List<IContentGetter> Getters = new List<IContentGetter>();
			Type[] GetterTypes = Types.GetTypesImplementingInterface(typeof(IContentGetter));
			Dictionary<string, List<IContentGetter>> ByScheme = new Dictionary<string, List<IContentGetter>>();
			IContentGetter Getter;
			TypeInfo TI;

			foreach (Type T in GetterTypes)
			{
				TI = T.GetTypeInfo();
				if (TI.IsAbstract || TI.IsGenericTypeDefinition)
					continue;

				try
				{
					Getter = (IContentGetter)Activator.CreateInstance(T);
				}
				catch (Exception)
				{
					continue;
				}

				Getters.Add(Getter);

				foreach (string Schema in Getter.UriSchemes)
				{
					if (!ByScheme.TryGetValue(Schema, out List<IContentGetter> List))
					{
						List = new List<IContentGetter>();
						ByScheme[Schema] = List;
					}

					List.Add(Getter);
				}
			}

			lock (gettersByScheme)
			{
				foreach (KeyValuePair<string, List<IContentGetter>> P in ByScheme)
					gettersByScheme[P.Key] = P.Value.ToArray();
			}

			getters = Getters.ToArray();
		}

		/// <summary>
		/// If a resource can be gotten, given its URI.
		/// </summary>
		/// <param name="Uri">URI of resource.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <param name="Getter">Best getter for the URI.</param>
		/// <returns>If a resource with the given URI can be gotten.</returns>
		public static bool CanGet(Uri Uri, out Grade Grade, out IContentGetter Getter)
		{
			if (Uri is null)
				throw new ArgumentNullException("URI cannot be null.", nameof(Uri));

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

			return Getter != null;
		}

		/// <summary>
		/// Gets a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static Task<object> GetAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanGet(Uri, out Grade _, out IContentGetter Getter))
				throw new ArgumentException("URI Scheme not recognized: " + Uri.Scheme, nameof(Uri));

			return Getter.GetAsync(Uri, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<KeyValuePair<string, TemporaryFile>> GetTempFileAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			if (!CanGet(Uri, out Grade _, out IContentGetter Getter))
				throw new ArgumentException("URI Scheme not recognized: " + Uri.Scheme, nameof(Uri));

			return Getter.GetTempFileAsync(Uri, TimeoutMs, Headers);
		}

		#endregion
	}
}
