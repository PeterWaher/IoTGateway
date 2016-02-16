using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Waher.Script;
using Waher.Events;

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
		private static IContentEncoder[] encoders = null;
		private static IContentDecoder[] decoders = null;
		private static IContentConverter[] converters = null;
		private static Dictionary<string, KeyValuePair<Grade, IContentDecoder>> decoderByContentType = 
			new Dictionary<string, KeyValuePair<Grade, IContentDecoder>>(Types.CaseInsensitiveComparer);
		private static Dictionary<string, string> contentTypeByFileExtensions = new Dictionary<string, string>(Types.CaseInsensitiveComparer);
		private static Dictionary<string, IContentConverter> convertersByStep = new Dictionary<string, IContentConverter>(Types.CaseInsensitiveComparer);
		private static Dictionary<string, LinkedList<IContentConverter>> convertersByFrom = new Dictionary<string, LinkedList<IContentConverter>>();

		static InternetContent()
		{
			Types.OnInvalidated += new EventHandler(Types_OnInvalidated);
		}

		private static void Types_OnInvalidated(object sender, EventArgs e)
		{
			canEncodeContentTypes = null;
			canEncodeFileExtensions = null;
			canDecodeContentTypes = null;
			canDecodeFileExtensions = null;
			encoders = null;
			decoders = null;
			converters = null;

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
		}

		#region Encoding

		/// <summary>
		/// Internet content types that can be encoded.
		/// </summary>
		public static string[] CanEncodeContentTypes
		{
			get
			{
				if (canEncodeContentTypes == null)
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
				if (canEncodeFileExtensions == null)
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
				if (encoders == null)
				{
					List<IContentEncoder> Encoders = new List<IContentEncoder>();
					ConstructorInfo CI;
					IContentEncoder Encoder;
					Type[] EncoderTypes = Types.GetTypesImplementingInterface(typeof(IContentEncoder));

					foreach (Type T in EncoderTypes)
					{
						if (T.IsAbstract)
							continue;

						CI = T.GetConstructor(Types.NoTypes);
						if (CI == null)
							continue;

						try
						{
							Encoder = (IContentEncoder)CI.Invoke(Types.NoParameters);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
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
		/// <returns>If the object can be encoded.</returns>
		public static bool Encodes(object Object, out Grade Grade, out IContentEncoder Encoder)
		{
			Grade Grade2;

			Grade = Grade.NotAtAll;
			Encoder = null;

			foreach (IContentEncoder Encoder2 in Encoders)
			{
				if (Encoder2.Encodes(Object, out Grade2) && Grade2 > Grade)
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
		/// <returns>Encoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
		public static byte[] Encode(object Object, Encoding Encoding, out string ContentType)
		{
			IContentEncoder Encoder;
			Grade Grade;

			if (!Encodes(Object, out Grade, out Encoder))
				throw new ArgumentException("No encoder found to encode the object", "Object");

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
			if (AcceptedContentTypes.Length == 0)
				return true;

			foreach (string ContentType in ContentTypes)
			{
				if (IsAccepted(ContentType, AcceptedContentTypes))
					return true;
			}

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
				if (canDecodeContentTypes == null)
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
				if (canDecodeFileExtensions == null)
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
				if (decoders == null)
				{
					List<IContentDecoder> Decoders = new List<IContentDecoder>();
					ConstructorInfo CI;
					IContentDecoder Decoder;
					Type[] DecoderTypes = Types.GetTypesImplementingInterface(typeof(IContentDecoder));

					foreach (Type T in DecoderTypes)
					{
						if (T.IsAbstract)
							continue;

						CI = T.GetConstructor(Types.NoTypes);
						if (CI == null)
							continue;

						try
						{
							Decoder = (IContentDecoder)CI.Invoke(Types.NoParameters);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
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
				KeyValuePair<Grade, IContentDecoder> P;

				if (decoderByContentType.TryGetValue(ContentType, out P))
				{
					Grade = P.Key;
					Decoder = P.Value;

					return Decoder != null;
				}
			}

			Grade Grade2;

			Grade = Grade.NotAtAll;
			Decoder = null;

			foreach (IContentDecoder Decoder2 in Decoders)
			{
				if (Decoder2.Decodes(ContentType, out Grade2) && Grade2 > Grade)
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
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public static object Decode(string ContentType, byte[] Data, Encoding Encoding)
		{
			IContentDecoder Decoder;
			Grade Grade;

			if (!Decodes(ContentType, out Grade, out Decoder))
				throw new ArgumentException("No decoder found to decode objects of type " + ContentType + ".", "ContentType");

			return Decoder.Decode(ContentType, Data, Encoding);
		}

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public static object Decode(string ContentType, byte[] Data)
		{
			Encoding Encoding = null;
			string[] Parts;
			string s, Key, Value;
			int i;

			i = ContentType.IndexOf(';');
			if (i > 0)
			{
				Parts = ContentType.Substring(i + 1).TrimStart().Split(';');
				ContentType = ContentType.Substring(0, i).TrimEnd();

				foreach (string Part in Parts)
				{
					s = Part.ToUpper().Trim();
					i = s.IndexOf('=');
					if (i < 0)
						continue;

					Key = s.Substring(0, i).TrimEnd();
					Value = s.Substring(i + 1).TrimStart();

					if (Key == "CHARSET")
					{
						if ((Value.StartsWith("\"") && Value.EndsWith("\"")) || (Value.StartsWith("'") && Value.EndsWith("'")))
							Value = Value.Substring(1, Value.Length - 2);

						// Reference: http://www.iana.org/assignments/character-sets/character-sets.xhtml
						switch (Value)
						{
							case "ASCII":
							case "US-ASCII":
								Encoding = Encoding.ASCII;
								break;

							case "UTF-16LE":
							case "UTF-16":
								Encoding = Encoding.Unicode;
								break;

							case "UTF-16BE":
								Encoding = Encoding.BigEndianUnicode;
								break;

							case "UTF-32":
							case "UTF-32LE":
								Encoding = Encoding.UTF32;
								break;

							case "UNICODE-1-1-UTF-7":
							case "UTF-7":
								Encoding = Encoding.UTF7;
								break;

							case "UTF-8":
								Encoding = Encoding.UTF8;
								break;

							default:
								Encoding = Encoding.GetEncoding(Value);
								break;
						}
					}
				}
			}

			return Decode(ContentType, Data, Encoding);
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
			string ContentType;

			if (TryGetContentType(FileExtension, out ContentType))
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
				if (converters == null)
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
				if (converters == null)
					FindConverters();

				string PathKey = FromContentType + " -> " + ToContentType;
				if (convertersByStep.TryGetValue(PathKey, out Converter))
					return Converter != null;

				LinkedList<IContentConverter> Converters;

				if (!convertersByFrom.TryGetValue(FromContentType, out Converters))
					return false;

				LinkedList<ConversionStep> Queue = new LinkedList<ConversionStep>();
				ConversionStep Step;

				foreach (IContentConverter C in Converters)
				{
					Step = new ConversionStep();
					Step.From = FromContentType;
					Step.Converter = C;
					Step.TotalGrade = C.ConversionGrade;
					Step.Prev = null;
					Step.Distance = 1;
					Queue.AddLast(Step);
				}

				Dictionary<string, ConversionStep> Possibilities = new Dictionary<string, ConversionStep>();
				ConversionStep NextStep;
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
							if (Possibilities.TryGetValue(To, out NextStep) && NextStep.TotalGrade >= StepGrade && NextStep.Distance <= StepDistance)
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

								NextStep = new ConversionStep();
								NextStep.From = To;
								NextStep.Converter = C;
								NextStep.TotalGrade = StepGrade;
								NextStep.Prev = Step;
								NextStep.Distance = StepDistance;
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
			LinkedList<IContentConverter> List;
			ConstructorInfo CI;
			IContentConverter Converter;
			Type[] ConverterTypes = Types.GetTypesImplementingInterface(typeof(IContentConverter));

			convertersByStep.Clear();
			convertersByFrom.Clear();

			lock (convertersByStep)
			{
				foreach (Type T in ConverterTypes)
				{
					if (T.IsAbstract)
						continue;

					CI = T.GetConstructor(Types.NoTypes);
					if (CI == null)
						continue;

					try
					{
						Converter = (IContentConverter)CI.Invoke(Types.NoParameters);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						continue;
					}

					Converters.Add(Converter);

					foreach (string From in Converter.FromContentTypes)
					{
						if (!convertersByFrom.TryGetValue(From, out List))
						{
							List = new LinkedList<IContentConverter>();
							convertersByFrom[From] = List;
						}

						List.AddLast(Converter);

						foreach (string To in Converter.ToContentTypes)
						{
							convertersByStep[From + " -> " + To] = Converter;
						}
					}
				}
			}

			converters = Converters.ToArray();
		}

		#endregion

	}
}
