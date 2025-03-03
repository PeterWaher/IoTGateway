﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Settings;

namespace Waher.Runtime.Language
{
	/// <summary>
	/// Basic access point for runtime language localization.
	/// </summary>
	public static class Translator
	{
		/// <summary>
		/// Resource name of embedded schema file.
		/// </summary>
		public const string SchemaResource = "Waher.Runtime.Language.Schema.Translation.xsd";

		/// <summary>
		/// Namespace of embedded schema file.
		/// </summary>
		public const string SchemaNamespace = "http://waher.se/Schema/Translation.xsd";

		/// <summary>
		/// Expected root in XML files.
		/// </summary>
		public const string SchemaRoot = "Translation";

		private readonly static SortedDictionary<string, Language> languagesByCode = new SortedDictionary<string, Language>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static SortedDictionary<string, Language> languagesByName = new SortedDictionary<string, Language>(StringComparer.CurrentCultureIgnoreCase);
		private readonly static object synchObject = new object();
		private static bool langaugesLoaded = false;

		/// <summary>
		/// Gets the languge object, given its language code, if available.
		/// </summary>
		/// <param name="Code">Language code.</param>
		/// <returns>Language object, if found, or null if not found.</returns>
		public static async Task<Language> GetLanguageAsync(string Code)
		{
			lock (synchObject)
			{
				if (languagesByCode.TryGetValue(Code, out Language Result))
					return Result;
			}

			Language Language = await Database.FindFirstIgnoreRest<Language>(
				new FilterFieldEqualTo("Code", Code));

			if (!(Language is null))
			{
				lock (synchObject)
				{
					if (languagesByCode.TryGetValue(Code, out Language Result))
						return Result;

					languagesByCode[Language.Code] = Language;
					languagesByName[Language.Name] = Language;
				}
			}

			return Language;
		}

		/// <summary>
		/// Gets available languages.
		/// </summary>
		/// <returns>Languages.</returns>
		public static async Task<Language[]> GetLanguagesAsync()
		{
			if (!langaugesLoaded)
			{
				IEnumerable<Language> Languages = await Database.Find<Language>();

				lock (synchObject)
				{
					foreach (Language Language in Languages)
					{
						if (!languagesByCode.ContainsKey(Language.Code))
						{
							languagesByCode[Language.Code] = Language;
							languagesByName[Language.Name] = Language;
						}
					}
				}

				langaugesLoaded = true;
			}

			lock (synchObject)
			{
				Language[] Result = new Language[languagesByCode.Count];
				languagesByCode.Values.CopyTo(Result, 0);
				return Result;
			}
		}

		/// <summary>
		/// Creates a new language, or updates an existing language, if one exist with the same code.
		/// </summary>
		/// <param name="Code">Language code.</param>
		/// <param name="Name">Name.</param>
		/// <param name="Flag">Flag.</param>
		/// <param name="FlagWidth">Width of flag.</param>
		/// <param name="FlagHeight">Height of flag.</param>
		/// <returns>Language object.</returns>
		public static async Task<Language> CreateLanguageAsync(string Code, string Name, byte[] Flag, int FlagWidth, int FlagHeight)
		{
			Language Result = await GetLanguageAsync(Code);
			if (!(Result is null))
			{
				bool Updated = false;

				if (Result.Name != Name)
				{
					lock (synchObject)
					{
						languagesByName.Remove(Result.Name);
						Result.Name = Name;
						languagesByName[Name] = Result;
					}

					Updated = true;
				}

				if (!(Flag is null))
				{
					Result.Flag = Flag;
					Result.FlagWidth = FlagWidth;
					Result.FlagHeight = FlagHeight;

					Updated = true;
				}

				if (Updated)
					await Database.Update(Result);

				return Result;
			}
			else
			{
				bool Insert = false;
				bool Update = false;

				lock (synchObject)
				{
					if (languagesByCode.TryGetValue(Code, out Result))
					{
						if (Result.Name != Name && !string.IsNullOrEmpty(Name))
						{
							languagesByName.Remove(Result.Name);
							Result.Name = Name;
							languagesByName[Name] = Result;
							Update = true;
						}

						if (!(Flag is null))
						{
							Result.Flag = Flag;
							Result.FlagWidth = FlagWidth;
							Result.FlagHeight = FlagHeight;
							Update = true;
						}
					}
					else
					{
						Result = new Language()
						{
							Code = Code,
							Name = Name,
							Flag = Flag,
							FlagWidth = FlagWidth,
							FlagHeight = FlagHeight
						};

						languagesByCode[Code] = Result;
						languagesByName[Name] = Result;

						Insert = true;
					}
				}

				if (Insert)
					await Database.Insert(Result);
				else if (Update)
					await Database.Update(Result);

				return Result;
			}
		}

		/// <summary>
		/// Default language code.
		/// </summary>
		public static string DefaultLanguageCode
		{
			get
			{
				if (defaultLanguageCode is null)
					defaultLanguageCode = RuntimeSettings.Get("DefaultLanguage", "en");

				return defaultLanguageCode;
			}

			set
			{
				if (defaultLanguageCode != value)
				{
					defaultLanguageCode = value;
					RuntimeSettings.Set("DefaultLanguage", value);
				}
			}
		}

		private static string defaultLanguageCode = null;

		/// <summary>
		/// Gets the default language.
		/// </summary>
		public static async Task<Language> GetDefaultLanguageAsync()
		{
			string Code = DefaultLanguageCode;
			Language Result = await GetLanguageAsync(Code)
				?? await CreateLanguageAsync(Code, Code == "en" ? "English" : Code, null, 0, 0);

			return Result;
		}

		/// <summary>
		/// Imports language strings into the language database.
		/// </summary>
		/// <param name="Xml">XML containing language information.</param>
		public static async Task ImportAsync(XmlReader Xml)
		{
			Language Language = null;
			Namespace Namespace = null;
			string Code;
			string Name;
			string Value = null;
			byte[] Flag;
			int? FlagWidth;
			int? FlagHeight;
			string Id = null;
			TranslationLevel Level = TranslationLevel.HumanTranslated;
			bool InString = false;

			while (Xml.Read())
			{
				switch (Xml.NodeType)
				{
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Whitespace:
						if (Value is null)
							Value = Xml.Value;
						else
							Value += Xml.Value;
						break;

					case XmlNodeType.Element:
						switch (Xml.Name)
						{
							case "Translation":
								break;

							case "Language":
								Code = Name = null;
								Flag = null;
								FlagWidth = FlagHeight = null;

								if (Xml.MoveToFirstAttribute())
								{
									do
									{
										switch (Xml.Name)
										{
											case "code":
												Code = Xml.Value;
												break;

											case "name":
												Name = Xml.Value;
												break;

											case "flag":
												Flag = Convert.FromBase64String(Xml.Value);
												break;

											case "flagWidth":
												FlagWidth = int.Parse(Xml.Value);
												break;

											case "flagHeight":
												FlagHeight = int.Parse(Xml.Value);
												break;
										}
									}
									while (Xml.MoveToNextAttribute());
								}

								if (!(Flag is null) && FlagWidth.HasValue && FlagHeight.HasValue)
									Language = await CreateLanguageAsync(Code, Name, Flag, FlagWidth.Value, FlagHeight.Value);
								else
									Language = await CreateLanguageAsync(Code, Name, null, 0, 0);

								Namespace = null;
								break;

							case "Namespace":
								Name = null;

								if (Xml.MoveToFirstAttribute())
								{
									do
									{
										switch (Xml.Name)
										{
											case "name":
												Name = Xml.Value;
												break;
										}
									}
									while (Xml.MoveToNextAttribute());
								}

								Namespace = await Language.CreateNamespaceAsync(Name);
								break;

							case "String":
								Id = null;
								Level = TranslationLevel.HumanTranslated;
								InString = true;
								Value = null;

								if (Xml.MoveToFirstAttribute())
								{
									do
									{
										switch (Xml.Name)
										{
											case "id":
												Id = Xml.Value;
												break;

											case "level":
												Level = (TranslationLevel)Enum.Parse(typeof(TranslationLevel), Xml.Value);
												break;

											case "untranslated":
												Level = (Xml.Value == "true") ? TranslationLevel.Untranslated : TranslationLevel.HumanTranslated;
												break;
										}
									}
									while (Xml.MoveToNextAttribute());
								}
								break;
						}
						break;

					case XmlNodeType.EndElement:
						if (InString)
						{
							if (!string.IsNullOrEmpty(Id) && !(Value is null))
							{
								if (string.IsNullOrEmpty(Value))
									await Namespace.DeleteStringAsync(Id);
								else
									await Namespace.CreateStringAsync(Id, Value, Level);
							}

							InString = false;
						}

						Value = null;
						break;

					case XmlNodeType.Entity:
					case XmlNodeType.EntityReference:

					case XmlNodeType.Comment:
					case XmlNodeType.Document:
					case XmlNodeType.DocumentFragment:
					case XmlNodeType.DocumentType:
					case XmlNodeType.EndEntity:
					case XmlNodeType.None:
					case XmlNodeType.Notation:
					case XmlNodeType.ProcessingInstruction:
					case XmlNodeType.XmlDeclaration:
						break;
				}
			}
		}

		/// <summary>
		/// Imports language strings into the language database.
		/// </summary>
		/// <param name="FileName">Language file.</param>
		public static async Task ImportAsync(string FileName)
		{
			using (StreamReader r = System.IO.File.OpenText(FileName))
			{
				using (XmlReader Xml = XmlReader.Create(r))
				{
					await ImportAsync(Xml);
				}
			}
		}

	}
}
