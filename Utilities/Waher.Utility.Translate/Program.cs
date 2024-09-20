using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Runtime.Settings;

namespace Waher.Utility.Translate
{
	class Program
	{
		/// <summary>
		/// Translates resource strings from one language to another. It uses an internal
		/// database to check for updates, and performs translations only of new or
		/// updated strings accordingly.
		/// 
		/// Command line switches:
		/// 
		/// -d APP_DATA_FOLDER    Points to the application data folder where intermediate
		///                       information about translations are kept.
		/// -e                    If encryption is used by the database.
		/// -bs BLOCK_SIZE        Block size, in bytes. Default=8192.
		/// -bbs BLOB_BLOCK_SIZE  BLOB block size, in bytes. Default=8192.
		/// -enc ENCODING         Text encoding. Default=UTF-8
		/// -rf REFERENCE_FILE    Reference resource file, containing the reference
		///                       language strings
		/// -rl REF_LANGUAGE      Reference language code.
		/// -tl TO_LANGUAGE       Language code to translate to.
		/// -tf TO_FILE           Resource file of translation. If it exists, will be used
		///                       to update the reference database of manual translations.
		/// -df DIFF_FILE         Resource file containing translated strings only.
		/// -ns NAMESPACE         Optional namespace. If not provided, the language to
		///                       translate to will be used.
		/// -tk TRANSLATION_KEY   Microsoft Translator Key. Will be stored in the database
		///                       if provided, for reference. If not provided, value in
		///                       database will be used.
		/// </summary>
		static int Main(string[] args)
		{
			try
			{
				Encoding Encoding = Encoding.UTF8;
				string ProgramDataFolder = null;
				string ReferenceFileName = null;
				string ToFileName = null;
				string DiffFileName = null;
				string ReferenceLanguage = null;
				string TranslationLanguage = null;
				string TranslationKey = null;
				string Namespace = null;
				string s;
				int BlockSize = 8192;
				int BlobBlockSize = 8192;
				int i = 0;
				int c = args.Length;
				bool Help = false;
				bool Encryption = false;

				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-d":
							if (i >= c)
								throw new Exception("Missing program data folder.");

							if (string.IsNullOrEmpty(ProgramDataFolder))
								ProgramDataFolder = args[i++];
							else
								throw new Exception("Only one program data folder allowed.");
							break;

						case "-bs":
							if (i >= c)
								throw new Exception("Block size missing.");

							if (!int.TryParse(args[i++], out BlockSize))
								throw new Exception("Invalid block size");

							break;

						case "-bbs":
							if (i >= c)
								throw new Exception("Blob Block size missing.");

							if (!int.TryParse(args[i++], out BlobBlockSize))
								throw new Exception("Invalid blob block size");

							break;

						case "-enc":
							if (i >= c)
								throw new Exception("Text encoding missing.");

							Encoding = Encoding.GetEncoding(args[i++]);
							break;

						case "-e":
							Encryption = true;
							break;

						case "-rf":
							if (i >= c)
								throw new Exception("Missing reference file name.");

							if (string.IsNullOrEmpty(ReferenceFileName))
								ReferenceFileName = args[i++];
							else
								throw new Exception("Only one reference file name allowed.");
							break;

						case "-tf":
							if (i >= c)
								throw new Exception("Missing to file name.");

							if (string.IsNullOrEmpty(ToFileName))
								ToFileName = args[i++];
							else
								throw new Exception("Only one to file name allowed.");
							break;

						case "-rl":
							if (i >= c)
								throw new Exception("Missing reference language.");

							if (string.IsNullOrEmpty(ReferenceLanguage))
								ReferenceLanguage = args[i++];
							else
								throw new Exception("Only one reference language allowed.");
							break;

						case "-tl":
							if (i >= c)
								throw new Exception("Missing translation language.");

							if (string.IsNullOrEmpty(TranslationLanguage))
								TranslationLanguage = args[i++];
							else
								throw new Exception("Only one translation language allowed.");
							break;

						case "-df":
							if (i >= c)
								throw new Exception("Missing diff file name.");

							if (string.IsNullOrEmpty(DiffFileName))
								DiffFileName = args[i++];
							else
								throw new Exception("Only one diff file name allowed.");
							break;

						case "-ns":
							if (i >= c)
								throw new Exception("Missing namespace.");

							if (string.IsNullOrEmpty(Namespace))
								Namespace = args[i++];
							else
								throw new Exception("Only one namespace allowed.");
							break;

						case "-tk":
							if (i >= c)
								throw new Exception("Missing translation key.");

							if (string.IsNullOrEmpty(TranslationKey))
								TranslationKey = args[i++];
							else
								throw new Exception("Only one translation key allowed.");
							break;

						case "-?":
							Help = true;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (Help || c == 0)
				{
					ConsoleOut.WriteLine("Translates resource strings from one language to another. It uses an internal");
					ConsoleOut.WriteLine("database to check for updates, and performs translations only of new or");
					ConsoleOut.WriteLine("updated strings accordingly.");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("Command line switches:");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("-d APP_DATA_FOLDER    Points to the application data folder where intermediate");
					ConsoleOut.WriteLine("                      information about translations are kept.");
					ConsoleOut.WriteLine("-e                    If encryption is used by the database.");
					ConsoleOut.WriteLine("-bs BLOCK_SIZE        Block size, in bytes. Default=8192.");
					ConsoleOut.WriteLine("-bbs BLOB_BLOCK_SIZE  BLOB block size, in bytes. Default=8192.");
					ConsoleOut.WriteLine("-enc ENCODING         Text encoding. Default=UTF-8");
					ConsoleOut.WriteLine("-rf REFERENCE_FILE    Reference resource file, containing the reference");
					ConsoleOut.WriteLine("                      language strings");
					ConsoleOut.WriteLine("-rl REF_LANGUAGE      Reference language code.");
					ConsoleOut.WriteLine("-tl TO_LANGUAGE       Language code to translate to.");
					ConsoleOut.WriteLine("-tf TO_FILE           Resource file of translation. If it exists, will be used");
					ConsoleOut.WriteLine("                      to update the reference database of manual translations.");
					ConsoleOut.WriteLine("-df DIFF_FILE         Resource file containing translated strings only.");
					ConsoleOut.WriteLine("-ns NAMESPACE         Optional namespace. If not provided, the language to");
					ConsoleOut.WriteLine("                      translate to will be used.");
					ConsoleOut.WriteLine("-tk TRANSLATION_KEY   Microsoft Translator Key. Will be stored in the database");
					ConsoleOut.WriteLine("                      if provided, for reference. If not provided, value in");
					ConsoleOut.WriteLine("                      database will be used.");
					return 0;
				}

				if (string.IsNullOrEmpty(ProgramDataFolder))
					throw new Exception("No program data folder set");

				if (string.IsNullOrEmpty(ReferenceFileName))
					throw new Exception("No reference file name specified.");

				if (string.IsNullOrEmpty(ReferenceLanguage))
					throw new Exception("No reference language specified.");

				if (string.IsNullOrEmpty(ToFileName))
					throw new Exception("No to file name specified.");

				if (string.IsNullOrEmpty(TranslationLanguage))
					throw new Exception("No translation language specified.");

				if (string.IsNullOrEmpty(Namespace))
					Namespace = TranslationLanguage;

				Types.Initialize(
					typeof(Database).Assembly,
					typeof(FilesProvider).Assembly,
					typeof(RuntimeSettings).Assembly,
					typeof(Translator).Assembly,
					typeof(InternetContent).Assembly,
					typeof(ObjectSerializer).Assembly);

				using FilesProvider FilesProvider = FilesProvider.CreateAsync(ProgramDataFolder, "Default", BlockSize, 10000, BlobBlockSize, Encoding, 3600000, Encryption, false).Result;

				Database.Register(FilesProvider);

				if (string.IsNullOrEmpty(TranslationKey))
				{
					TranslationKey = RuntimeSettings.Get("Translation.Key", string.Empty);
					if (string.IsNullOrEmpty(TranslationKey))
						throw new Exception("No translation key provided, and no translation key found in database.");
				}
				else
					RuntimeSettings.Set("Translation.Key", TranslationKey);

				XmlDocument From = new()
				{
					PreserveWhitespace = true
				};
				From.Load(ReferenceFileName);

				if (From.DocumentElement is null || From.DocumentElement.LocalName != "root")
					throw new Exception("Reference file does not point to a resource file.");

				Language FromLanguage = Translator.GetLanguageAsync(ReferenceLanguage).Result
					?? Translator.CreateLanguageAsync(ReferenceLanguage, ReferenceLanguage, null, 0, 0).Result;

				Language ToLanguage = Translator.GetLanguageAsync(TranslationLanguage).Result
					?? Translator.CreateLanguageAsync(TranslationLanguage, TranslationLanguage, null, 0, 0).Result;

				Namespace FromNamespace = FromLanguage.GetNamespaceAsync(Namespace).Result
					?? FromLanguage.CreateNamespaceAsync(Namespace).Result;

				Namespace ToNamespace = ToLanguage.GetNamespaceAsync(Namespace).Result
					?? ToLanguage.CreateNamespaceAsync(Namespace).Result;

				List<TranslationItem> ToTranslate = new();
				List<string> StringIds = new();
				XmlComment FromComment = null;
				XmlElement FromSchema = null;
				List<KeyValuePair<string, string>> Headers = new();

				foreach (XmlNode N in From.DocumentElement.ChildNodes)
				{
					if (N is XmlComment Comment)
						FromComment = Comment;
					else if (N is XmlElement E)
					{
						switch (E.LocalName)
						{
							case "schema":
								FromSchema = E;
								break;

							case "resheader":
								string Name = E.GetAttribute("name");
								string Value = E["value"]?.InnerText ?? string.Empty;

								Headers.Add(new KeyValuePair<string, string>(Name, Value));
								break;

							case "data":
								Name = E.GetAttribute("name");
								Value = E["value"]?.InnerText ?? string.Empty;
								bool NewString = false;

								StringIds.Add(Name);

								LanguageString FromString = FromNamespace.GetStringAsync(Name).Result;
								if (FromString is null || FromString.Value != Value)
								{
									NewString = true;
									FromString = FromNamespace.CreateStringAsync(Name, Value, TranslationLevel.HumanTranslated).Result;
								}

								LanguageString ToString = ToNamespace.GetStringAsync(Name).Result;
								if (ToString is not null && ToString.Level > TranslationLevel.Untranslated && !NewString)
									continue;

								ToTranslate.Add(new TranslationItem()
								{
									From = FromString,
									To = ToString
								});
								break;
						}
					}
				}

				c = ToTranslate.Count;
				if (c == 0)
					ConsoleOut.WriteLine("Strings up-to-date. No translation necessary.");
				else
				{
					ConsoleOut.WriteLine("Translating " + c.ToString() + " strings.");

					List<Dictionary<string, object>> Req = new();

					foreach (TranslationItem Item in ToTranslate)
						Req.Add(new Dictionary<string, object>() { { "Text", Item.From.Value } });

					StringBuilder Url = new();

					Url.Append("https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from=");
					Url.Append(ReferenceLanguage);
					Url.Append("&to=");
					Url.Append(TranslationLanguage);

					object Resp = InternetContent.PostAsync(new Uri(Url.ToString()), Req.ToArray(),
						new KeyValuePair<string, string>("Ocp-Apim-Subscription-Key", TranslationKey)).Result;

					if (Resp is not Array A)
						throw new Exception("Unexpected reponse returned.");

					List<string> Response = new();

					foreach (object Item in A)
					{
						if (Item is not Dictionary<string, object> TypedItem ||
							!TypedItem.TryGetValue("translations", out object Obj) ||
							Obj is not Array A2 ||
							A2.Length != 1 ||
							A2.GetValue(0) is not Dictionary<string, object> Translation ||
							!Translation.TryGetValue("text", out object Obj2) ||
							Obj2 is not string TranslatedText)
						{
							throw new Exception("Unexpected reponse returned.");
						}

						Response.Add(TranslatedText);
					}

					if (Response.Count != c)
						throw new Exception("Unexpected number of translated strings returned.");

					ConsoleOut.WriteLine(c.ToString() + " strings translated.");

					for (i = 0; i < c; i++)
					{
						TranslationItem Item = ToTranslate[i];

						if (Item.To is null)
							Item.To = ToNamespace.CreateStringAsync(Item.From.Id, Response[i], TranslationLevel.MachineTranslated).Result;
						else
						{
							Item.To.Value = Response[i];
							Item.To.Level = TranslationLevel.MachineTranslated;

							Database.Update(Item.To).Wait();
						}
					}
				}

				XmlWriterSettings Settings = new()
				{
					CloseOutput = true,
					ConformanceLevel = ConformanceLevel.Document,
					Encoding = Encoding.UTF8,
					Indent = true,
					IndentChars = "  ",
					NewLineChars = Environment.NewLine,
					NewLineHandling = NewLineHandling.Replace
				};

				using (XmlWriter Output = XmlWriter.Create(ToFileName, Settings))
				{
					Output.WriteStartDocument();
					Output.WriteStartElement("root");

					FromComment?.WriteTo(Output);
					FromSchema?.WriteTo(Output);

					foreach (KeyValuePair<string, string> P in Headers)
					{
						Output.WriteStartElement("resheader");
						Output.WriteAttributeString("name", P.Key);
						Output.WriteElementString("value", P.Value);
						Output.WriteEndElement();
					}

					foreach (string StringId in StringIds)
					{
						LanguageString String = ToNamespace.GetStringAsync(StringId).Result;

						Output.WriteStartElement("data");
						Output.WriteAttributeString("name", StringId);
						Output.WriteAttributeString("xml", "space", string.Empty, "preserve");
						Output.WriteElementString("value", String.Value ?? string.Empty);
						Output.WriteEndElement();
					}

					Output.WriteEndElement();
					Output.WriteEndDocument();
				}

				if (!string.IsNullOrEmpty(DiffFileName))
				{
					using XmlWriter Output = XmlWriter.Create(DiffFileName, Settings);

					Output.WriteStartDocument();
					Output.WriteStartElement("root");

					foreach (TranslationItem Item in ToTranslate)
					{
						Output.WriteStartElement("data");
						Output.WriteAttributeString("name", Item.To.Id);
						Output.WriteAttributeString("xml", "space", string.Empty, "preserve");
						Output.WriteElementString("value", Item.To.Value);
						Output.WriteEndElement();
					}

					Output.WriteEndElement();
					Output.WriteEndDocument();
				}

				return 0;
			}
			catch (Exception ex)
			{
				ConsoleOut.WriteLine(ex.Message);
				return -1;
			}
			finally
			{
				ConsoleOut.Flush(true);
			}
		}

		private class TranslationItem
		{
			public LanguageString From;
			public LanguageString To;
		}
	}
}
