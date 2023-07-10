using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Waher.Content.Getters;
using Waher.Content.Semantic.Model;
using Waher.Runtime.Cache;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Contains semantic information stored in a JSON-LD document.
	/// 
	/// Ref: https://www.w3.org/TR/json-ld/
	/// </summary>
	public class JsonLdDocument : InMemorySemanticCube, IWebServerMetaContent
	{
		private static readonly Cache<string, KeyValuePair<DateTimeOffset, Dictionary<string, object>>> contextObjects =
			new Cache<string, KeyValuePair<DateTimeOffset, Dictionary<string, object>>>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromHours(1));

		private readonly object parsedJson;
		private readonly BlankNodeIdMode blankNodeIdMode;
		private readonly string text;
		private readonly string blankNodeIdPrefix;
		private DateTimeOffset? date = null;
		private int blankNodeIndex = 0;

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		/// <param name="BlankNodeIdMode">How Blank Node IDs are generated</param>
		private JsonLdDocument(object Doc, string Text, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
		{
			this.parsedJson = Doc;
			this.text = Text;
			this.blankNodeIdMode = BlankNodeIdMode;
			this.blankNodeIdPrefix = BlankNodeIdPrefix;
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		public static Task<JsonLdDocument> CreateAsync(string Text)
		{
			return CreateAsync(JSON.Parse(Text), Text, null);
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		/// <param name="BaseUri">Base URI</param>
		public static Task<JsonLdDocument> CreateAsync(string Text, Uri BaseUri)
		{
			return CreateAsync(JSON.Parse(Text), Text, BaseUri, "n");
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		public static Task<JsonLdDocument> CreateAsync(string Text, Uri BaseUri, string BlankNodeIdPrefix)
		{
			return CreateAsync(JSON.Parse(Text), Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode.Sequential);
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		/// <param name="BlankNodeIdMode">How Blank Node IDs are generated</param>
		public static Task<JsonLdDocument> CreateAsync(string Text, Uri BaseUri, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
		{
			return CreateAsync(JSON.Parse(Text), Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode);
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		public static Task<JsonLdDocument> CreateAsync(object Doc, string Text)
		{
			return CreateAsync(Doc, Text, null);
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		/// <param name="BaseUri">Base URI</param>
		public static Task<JsonLdDocument> CreateAsync(object Doc, string Text, Uri BaseUri)
		{
			return CreateAsync(Doc, Text, BaseUri, "n");
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		public static Task<JsonLdDocument> CreateAsync(object Doc, string Text, Uri BaseUri, string BlankNodeIdPrefix)
		{
			return CreateAsync(Doc, Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode.Sequential);
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		/// <param name="BlankNodeIdMode">How Blank Node IDs are generated</param>
		public static async Task<JsonLdDocument> CreateAsync(object Doc, string Text, Uri BaseUri, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
		{
			JsonLdDocument Result = new JsonLdDocument(Doc, Text, BlankNodeIdPrefix, BlankNodeIdMode);
			await Result.Parse(Doc, null, BaseUri, Result.CreateBlankNode());
			return Result;
		}

		private BlankNode CreateBlankNode()
		{
			if (this.blankNodeIdMode == BlankNodeIdMode.Guid)
				return new BlankNode(this.blankNodeIdPrefix + Guid.NewGuid().ToString());
			else
				return new BlankNode(this.blankNodeIdPrefix + (++this.blankNodeIndex).ToString());
		}

		private UriNode CreateUriNode(string Reference, Uri BaseUri, JsonLdContext Context)
		{
			return new UriNode(this.CreateUri(Reference, BaseUri, Context), Reference);
		}

		private UriNode CreateUriNode(string Reference, Uri BaseUri, string ShortName, JsonLdContext Context)
		{
			return new UriNode(this.CreateUri(Reference, BaseUri, Context), ShortName);
		}

		private Uri CreateUri(string Reference, Uri BaseUri, JsonLdContext Context)
		{
			int i = Reference.IndexOf(':');
			if (i >= 0)
			{
				string Prefix = Reference.Substring(0, i);

				if (!(Context is null) && Context.TryGetStringValue(Prefix, out string PrefixUrl))
					Reference = PrefixUrl + Reference.Substring(i + 1);
			}

			BaseUri = Context?.Base ?? BaseUri;

			if (BaseUri is null)
			{
				if (Uri.TryCreate(Reference, UriKind.Absolute, out Uri URI))
					return URI;
				else
					throw this.ParsingException("Invalid URI: " + Reference);
			}
			else
			{
				if (string.IsNullOrEmpty(Reference))
					return BaseUri;
				else if (Uri.TryCreate(BaseUri, Reference, out Uri URI))
					return URI;
				else
					throw this.ParsingException("Invalid URI: " + Reference + " (base: " + BaseUri.ToString() + ")");
			}
		}

		private Exception ParsingException(string Message)
		{
			return new Exception(Message);
		}

		private async Task<ISemanticElement> Parse(object Doc, JsonLdContext Context, Uri BaseUri, ISemanticElement Subject)
		{
			ISemanticElement ReturnValue = null;

			if (Doc is Dictionary<string, object> DocObj)
			{
				string Name;
				object Value;
				ISemanticElement ParsedValue;
				Uri ParsedUri;
				string s;

				foreach (KeyValuePair<string, object> P in DocObj)
				{
					Name = P.Key;
					Value = P.Value;

					if (Name.StartsWith("@"))
					{
						switch (Name)
						{
							case "@context":
								Context = await this.GetContext(Value, BaseUri, Context);
								break;

							case "@id":
								s = Value as string
									?? throw this.ParsingException("Unsupported @id value: " + Value.ToString());

								Subject = this.CreateUriNode(s, BaseUri, Context);
								break;

							case "@type":
								this.AddType(Subject, Value, BaseUri, Context);
								break;

							case "@graph":
								await this.Parse(P.Value, Context, BaseUri, Subject);
								break;

							case "@version":
								break;

							case "@base":
								s = Value as string
									?? throw this.ParsingException("Unsupported @base value: " + Value.ToString());

								BaseUri = this.CreateUri(s, BaseUri, Context);
								break;

							case "@value":
								ReturnValue = await this.ParseValue(BaseUri, Value, Context);
								break;

							default:
								throw this.ParsingException("Unrecognized keyword: " + Name);
						}
					}
					else
					{
						ParsedUri = null;
						ParsedValue = null;

						if (!this.TryGetContextName(Name, Context, out object ContextValue))
						{
							if (Uri.TryCreate(Name, UriKind.Absolute, out Uri UriValue))    // Not relative to base URI.
							{
								ContextValue = Name;
								ParsedUri = UriValue;
							}
							else
								continue;
						}
						else if (ContextValue is null)
							continue;

						s = ContextValue as string;
						if (!(s is null))
							Name = s;
						else if (ContextValue is Dictionary<string, object> ContextValueObj)
						{
							foreach (KeyValuePair<string, object> P2 in ContextValueObj)
							{
								switch (P2.Key)
								{
									case "@id":
										s = P2.Value as string
											?? throw this.ParsingException("Unsupported @id value: " + P2.Value.ToString());

										Name = s;
										break;

									case "@type":
										s = P2.Value as string
											?? throw this.ParsingException("Unsupported @type value: " + P2.Value.ToString());

										switch (s)
										{
											case "@id":
												Value = this.CreateUri(Value?.ToString(), BaseUri, Context);
												break;

											default:
												Uri TypeUri = this.CreateUri(s, BaseUri, Context);
												ParsedValue = SemanticElements.Parse(Value?.ToString(), TypeUri.AbsoluteUri, string.Empty);
												break;
										}
										break;

									case "@vocab":
										s = P2.Value as string
											?? throw this.ParsingException("Unsupported @vocab value: " + P2.Value.ToString());

										ParsedUri = this.CreateUri(s, BaseUri, Context);
										break;

									case "@base":
										s = P2.Value as string
											?? throw this.ParsingException("Unsupported @base value: " + P2.Value.ToString());

										BaseUri = this.CreateUri(s, BaseUri, Context);
										break;

									default:
										throw this.ParsingException("Unhandled context keyword: " + P2.Key);
								}
							}

							if (ParsedUri is null && !(Context.Vocabulary is null))
								ParsedUri = this.CreateUri(Name, Context.Vocabulary, Context);
						}

						if (!(ParsedUri is null) || Uri.TryCreate(BaseUri, Name, out ParsedUri))
						{
							UriNode Predicate = new UriNode(ParsedUri, Name);

							if (ParsedValue is null)
							{
								if (Value is Array ValueArray)
								{
									foreach (object ValueItem in ValueArray)
									{
										ParsedValue = await this.ParseValue(BaseUri, ValueItem, Context);
										this.Add(new SemanticTriple(Subject, Predicate, ParsedValue));
									}
								}
								else
								{
									ParsedValue = await this.ParseValue(BaseUri, Value, Context);
									this.Add(new SemanticTriple(Subject, Predicate, ParsedValue));
								}
							}
							else
								this.Add(new SemanticTriple(Subject, Predicate, ParsedValue));
						}
					}
				}
			}
			else if (Doc is Array DocArray)
			{
				foreach (object DocItem in DocArray)
					await this.Parse(DocItem, Context, BaseUri, this.CreateBlankNode());
			}

			return ReturnValue;
		}

		private async Task<JsonLdContext> GetContext(object Value, Uri BaseUri, JsonLdContext Context)
		{
			if (Value is Dictionary<string, object> ContextObj)
				return new JsonLdContext(ContextObj, BaseUri);
			else if (Value is Array ContextArray)
			{
				JsonLdContext Result = new JsonLdContext();

				foreach (object ContextItem in ContextArray)
				{
					JsonLdContext Result2 = await this.GetContext(ContextItem, BaseUri, Context);
					Result.Append(Result2, BaseUri);
					Context = Result;
				}

				return Result;
			}
			else
			{
				Uri ContextUri;

				if (Value is string Url)
					ContextUri = this.CreateUri(Url, BaseUri, Context);
				else if (Value is Uri Uri2)
					ContextUri = Uri2;
				else
					throw this.ParsingException("Expected context URI.");

				if (contextObjects.TryGetValue(ContextUri.AbsolutePath, out KeyValuePair<DateTimeOffset, Dictionary<string, object>> P2))
				{
					try
					{
						object Obj = await InternetContent.GetAsync(ContextUri,
							new KeyValuePair<string, string>("Accept", JsonLdCodec.DefaultContentType),
							new KeyValuePair<string, string>("If-Modified-Since", CommonTypes.EncodeRfc822(P2.Key)));

						if (Obj is JsonLdDocument ContextDoc &&
							ContextDoc.parsedJson is Dictionary<string, object> ContextObj2 &&
							ContextObj2.TryGetValue("@context", out Obj) &&
							Obj is Dictionary<string, object> ContextObj3)
						{
							Context = new JsonLdContext(ContextObj3, BaseUri);
							contextObjects[ContextUri.AbsolutePath] = new KeyValuePair<DateTimeOffset, Dictionary<string, object>>(
								ContextDoc.Date ?? DateTimeOffset.Now, ContextObj3);
						}
						else
							Context = new JsonLdContext(P2.Value, BaseUri);
					}
					catch (Exception)
					{
						Context = new JsonLdContext(P2.Value, BaseUri);
					}
				}
				else
				{
					object Obj = await InternetContent.GetAsync(ContextUri,
						new KeyValuePair<string, string>("Accept", JsonLdCodec.DefaultContentType));

					if (Obj is JsonLdDocument ContextDoc &&
						ContextDoc.parsedJson is Dictionary<string, object> ContextObj2 &&
						ContextObj2.TryGetValue("@context", out Obj) &&
						Obj is Dictionary<string, object> ContextObj3)
					{
						Context = new JsonLdContext(ContextObj3, BaseUri);
						contextObjects[ContextUri.AbsolutePath] = new KeyValuePair<DateTimeOffset, Dictionary<string, object>>(
							ContextDoc.Date ?? DateTimeOffset.Now, ContextObj3);
					}
					else
						throw this.ParsingException("Context reference not a JSON-LD document. Expected Content-Type: " + JsonLdCodec.DefaultContentType);
				}

				return Context;
			}
		}

		private void AddType(ISemanticElement Subject, object Value, Uri BaseUri, JsonLdContext Context)
		{
			if (Value is string s)
			{
				if (this.TryGetContextName(s, Context, out object ContextValue))
				{
					s = ContextValue as string;
					if (string.IsNullOrEmpty(s))
						throw this.ParsingException("Invalid @type: " + ContextValue?.ToString());
				}

				this.Add(new SemanticTriple(Subject, RdfDocument.RdfType, this.CreateUriNode(s, BaseUri, Context)));
			}
			else if (Value is Array A)
			{
				foreach (object Item in A)
					this.AddType(Subject, Item, BaseUri, Context);
			}
			else
				throw this.ParsingException("Unsupported @type: " + Value?.ToString());
		}

		private bool TryGetContextName(string Name, JsonLdContext Context, out object Value)
		{
			if (Context is null)
			{
				Value = null;
				return false;
			}

			int i = Name.IndexOf(':');
			if (i >= 0)
			{
				string Prefix = Name.Substring(0, i);

				if (Context.TryGetStringValue(Prefix, out string PrefixUrl))
				{
					Value = PrefixUrl + Name.Substring(i + 1);
					return true;
				}
			}

			if (Context.TryGetObjectValue(Name, out Value))
				return true;

			if (!(Context.Vocabulary is null))
			{
				Value = Context.Vocabulary.AbsoluteUri + Name;
				return true;
			}

			Value = null;
			return false;
		}

		private async Task<ISemanticElement> ParseValue(Uri BaseUri, object Value, JsonLdContext Context)
		{
			if (Value is Dictionary<string, object> Obj)
			{
				ISemanticElement Object = null;

				foreach (KeyValuePair<string, object> P in Obj)
				{
					switch (P.Key)
					{
						case "@id":
							if (P.Value is string s)
								Object = this.CreateUriNode(s, BaseUri, Context);
							else
								throw this.ParsingException("Invalid @id: " + P.Value.ToString());
							break;

						case "@base":
							s = P.Value as string;
							if (!(s is null))
								BaseUri = this.CreateUri(s, BaseUri, Context);
							else
								throw this.ParsingException("Invalid @base: " + P.Value.ToString());
							break;

					}
				}

				if (Object is null)
					Object = this.CreateBlankNode();

				return await this.Parse(Obj, Context, BaseUri, Object) ?? Object;
			}
			else if (Value is Uri Uri)
				return new UriNode(Uri, Uri.ToString());
			else
				return SemanticElements.EncapsulateLiteral(Value);
		}

		/// <summary>
		/// Original content, as parsed JSON.
		/// </summary>
		public object ParsedJson => this.parsedJson;

		/// <summary>
		/// Text representation.
		/// </summary>
		public string Text => this.text;

		/// <summary>
		/// Server timestamp of document.
		/// </summary>
		public DateTimeOffset? Date => this.date;

		/// <summary>
		/// Decodes meta-information available in the HTTP Response.
		/// </summary>
		/// <param name="HttpResponse">HTTP Response.</param>
		public Task DecodeMetaInformation(HttpResponseMessage HttpResponse)
		{
			this.date = HttpResponse.Headers.Date;
			return Task.CompletedTask;
		}

	}
}
