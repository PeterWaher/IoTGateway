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

		private UriNode CreateUriNode(string Reference, Uri BaseUri, Dictionary<string, object> Context)
		{
			return new UriNode(this.CreateUri(Reference, BaseUri, Context), Reference);
		}

		private UriNode CreateUriNode(string Reference, Uri BaseUri, string ShortName, Dictionary<string, object> Context)
		{
			return new UriNode(this.CreateUri(Reference, BaseUri, Context), ShortName);
		}

		private Uri CreateUri(string Reference, Uri BaseUri, Dictionary<string, object> Context)
		{
			int i = Reference.IndexOf(':');
			if (i >= 0)
			{
				string Prefix = Reference.Substring(0, i);

				if (!(Context is null) && Context.TryGetValue(Prefix, out object PrefixValue))
				{
					if (!(PrefixValue is string PrefixUrl))
						throw this.ParsingException("Unrecognized prefix value: " + PrefixValue?.ToString());

					Reference = PrefixUrl + Reference.Substring(i + 1);
				}
			}

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

		private async Task Parse(object Doc, Dictionary<string, object> Context, Uri BaseUri, ISemanticElement Subject)
		{
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

							default:
								throw this.ParsingException("Unrecognized keyword: " + Name);
						}
					}
					else
					{
						ParsedUri = null;
						ParsedValue = null;

						if (!this.TryGetContextValue(Name, Context, out object ContextValue))
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

									default:
										throw this.ParsingException("Unhandled context keyword: " + P2.Key);
								}
							}

							if (ParsedUri is null &&
								Context.TryGetValue("@vocab", out ContextValue) &&
								!((s = ContextValue as string) is null))
							{
								Uri Vocabulary = this.CreateUri(s, BaseUri, Context);
								ParsedUri = this.CreateUri(Name, Vocabulary, Context);
							}
						}

						if (ParsedValue is null)
							ParsedValue = await this.ParseValue(BaseUri, Value, Context);

						if (!(ParsedUri is null) || Uri.TryCreate(BaseUri, Name, out ParsedUri))
							this.Add(new SemanticTriple(Subject, new UriNode(ParsedUri, Name), ParsedValue));
					}
				}
			}
			else if (Doc is Array DocArray)
			{
				foreach (object DocItem in DocArray)
					await this.Parse(DocItem, Context, BaseUri, this.CreateBlankNode());
			}
		}

		private async Task<Dictionary<string, object>> GetContext(object Value, Uri BaseUri, Dictionary<string, object> Context)
		{
			if (Value is Dictionary<string, object> ContextObj)
				return ContextObj;
			else if (Value is Array ContextArray)
			{
				Dictionary<string, object> Result = new Dictionary<string, object>();

				foreach (object ContextItem in ContextArray)
				{
					Dictionary<string, object> Result2 = await this.GetContext(ContextItem, BaseUri, Context);

					foreach (KeyValuePair<string, object> P in Result2)
					{
						if (!(Context is null) &&
							Context.TryGetValue(P.Key, out object Prev) &&
							Prev is string PrevString &&
							Uri.TryCreate(BaseUri, PrevString, out Uri PrevUri) &&
							Uri.TryCreate(PrevUri, P.Value?.ToString(),out Uri NewUri))
						{
							Result[P.Key] = NewUri.AbsoluteUri;
						}
						else
							Result[P.Key] = P.Value;
					}

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
							Context = ContextObj3;
							contextObjects[ContextUri.AbsolutePath] = new KeyValuePair<DateTimeOffset, Dictionary<string, object>>(
								ContextDoc.Date ?? DateTimeOffset.Now, Context);
						}
						else
							Context = P2.Value;
					}
					catch (Exception)
					{
						return P2.Value;
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
						Context = ContextObj3;
						contextObjects[ContextUri.AbsolutePath] = new KeyValuePair<DateTimeOffset, Dictionary<string, object>>(
							ContextDoc.Date ?? DateTimeOffset.Now, Context);
					}
					else
						throw this.ParsingException("Context reference not a JSON-LD document. Expected Content-Type: " + JsonLdCodec.DefaultContentType);
				}

				return Context;
			}
		}

		private void AddType(ISemanticElement Subject, object Value, Uri BaseUri, Dictionary<string, object> Context)
		{
			if (Value is string s)
			{
				if (this.TryGetContextValue(s, Context, out object ContextValue))
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

		private bool TryGetContextValue(string Name, Dictionary<string, object> Context, out object Value)
		{
			if (Context is null)
			{
				Value = null;
				return false;
			}
			else if (Context.TryGetValue(Name, out Value))
				return true;
			else if (Context.TryGetValue("@vocab", out Value))
			{
				if (Value is string s)
					Value = s + Name;
				else
					throw this.ParsingException("Unable to use @vocab: " + Value?.ToString());

				return true;
			}
			else
			{
				Value = null;
				return false;
			}
		}

		private async Task<ISemanticElement> ParseValue(Uri BaseUri, object Value, Dictionary<string, object> Context)
		{
			if (Value is Dictionary<string, object> Obj)
			{
				ISemanticElement Object;

				if (Obj.TryGetValue("@id", out Value) && Value is string s)
					Object = this.CreateUriNode(s, BaseUri, Context);
				else
					Object = this.CreateBlankNode();

				await this.Parse(Obj, Context, BaseUri, Object);

				return Object;
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
