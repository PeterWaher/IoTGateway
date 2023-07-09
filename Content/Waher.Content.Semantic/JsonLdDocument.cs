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

		private readonly Dictionary<string, object> json;
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
		private JsonLdDocument(Dictionary<string, object> Doc, string Text, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
		{
			this.json = Doc;
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
			return CreateAsync(ParseJson(Text), Text, null);
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		/// <param name="BaseUri">Base URI</param>
		public static Task<JsonLdDocument> CreateAsync(string Text, Uri BaseUri)
		{
			return CreateAsync(ParseJson(Text), Text, BaseUri, "n");
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		public static Task<JsonLdDocument> CreateAsync(string Text, Uri BaseUri, string BlankNodeIdPrefix)
		{
			return CreateAsync(ParseJson(Text), Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode.Sequential);
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
			return CreateAsync(ParseJson(Text), Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode);
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		public static Task<JsonLdDocument> CreateAsync(Dictionary<string, object> Doc, string Text)
		{
			return CreateAsync(Doc, Text, null);
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of JSON-LD document.</param>
		/// <param name="BaseUri">Base URI</param>
		public static Task<JsonLdDocument> CreateAsync(Dictionary<string, object> Doc, string Text, Uri BaseUri)
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
		public static Task<JsonLdDocument> CreateAsync(Dictionary<string, object> Doc, string Text, Uri BaseUri, string BlankNodeIdPrefix)
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
		public static async Task<JsonLdDocument> CreateAsync(Dictionary<string, object> Doc, string Text, Uri BaseUri, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
		{
			JsonLdDocument Result = new JsonLdDocument(Doc, Text, BlankNodeIdPrefix, BlankNodeIdMode);
			await Result.Parse(Doc, null, BaseUri, Result.CreateBlankNode());
			return Result;
		}

		internal static Dictionary<string, object> ParseJson(string Text)
		{
			object Parsed = JSON.Parse(Text);

			if (Parsed is Dictionary<string, object> Obj)
				return Obj;
			else
				throw new ArgumentException("Invalid JSON-LD document.", nameof(Text));
		}

		private BlankNode CreateBlankNode()
		{
			if (this.blankNodeIdMode == BlankNodeIdMode.Guid)
				return new BlankNode(this.blankNodeIdPrefix + Guid.NewGuid().ToString());
			else
				return new BlankNode(this.blankNodeIdPrefix + (++this.blankNodeIndex).ToString());
		}

		private UriNode CreateUriNode(string Reference, Uri BaseUri)
		{
			return new UriNode(this.CreateUri(Reference, BaseUri), Reference);
		}

		private UriNode CreateUriNode(string Reference, Uri BaseUri, string ShortName)
		{
			return new UriNode(this.CreateUri(Reference, BaseUri), ShortName);
		}

		private Uri CreateUri(string Reference, Uri BaseUri)
		{
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

		private async Task Parse(Dictionary<string, object> Doc, Dictionary<string, object> Context, Uri BaseUri, ISemanticElement Subject)
		{
			if (Doc is null)
				return;

			string Name;
			object Value;
			ISemanticElement ParsedValue;
			Uri ParsedUri;
			string s;

			foreach (KeyValuePair<string, object> P in Doc)
			{
				Name = P.Key;
				Value = P.Value;

				if (Name.StartsWith("@"))
				{
					switch (Name)
					{
						case "@context":
							if (Value is Dictionary<string, object> ContextObj)
								Context = ContextObj;
							else
							{
								Uri ContextUri;

								if (Value is string Url)
									ContextUri = this.CreateUri(Url, BaseUri);
								else
								{
									ParsedValue = this.ParseValue(BaseUri, Value);
									if (ParsedValue is UriNode UriNode)
										ContextUri = UriNode.Uri;
									else
										throw this.ParsingException("Expected context URI.");
								}

								if (contextObjects.TryGetValue(ContextUri.AbsolutePath, out KeyValuePair<DateTimeOffset, Dictionary<string, object>> P2))
								{
									try
									{
										object Obj = await InternetContent.GetAsync(ContextUri,
											new KeyValuePair<string, string>("Accept", JsonLdCodec.DefaultContentType),
											new KeyValuePair<string, string>("If-Modified-Since", CommonTypes.EncodeRfc822(P2.Key)));

										if (Obj is JsonLdDocument ContextDoc &&
											ContextDoc.json is Dictionary<string, object> ContextObj2 &&
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
										Context = P2.Value;
									}
								}
								else
								{
									object Obj = await InternetContent.GetAsync(ContextUri,
										new KeyValuePair<string, string>("Accept", JsonLdCodec.DefaultContentType));

									if (Obj is JsonLdDocument ContextDoc &&
										ContextDoc.json is Dictionary<string, object> ContextObj2 &&
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
							}
							break;

						case "@id":
							s = Value as string
								?? throw this.ParsingException("Unsupported @id value: " + Value.ToString());

							Subject = this.CreateUriNode(s, BaseUri);
							break;

						case "@type":
							this.AddType(Subject, Value, BaseUri, Context);
							break;

						default:
							throw this.ParsingException("Unrecognized keyword: " + Name);
					}
				}
				else
				{
					ParsedUri = null;

					if (Context is null || !Context.TryGetValue(Name, out object ContextValue))
					{
						if (Uri.TryCreate(Name, UriKind.Absolute, out Uri UriValue))    // Not relative to base URI.
						{
							ContextValue = Name;
							ParsedUri = UriValue;
						}
						else
							continue;
					}

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
											Value = this.CreateUri(Value?.ToString(), BaseUri);
											break;

										default:
											throw this.ParsingException("Unsupported @type value: " + s);
									}
									break;
							}
						}
					}

					ParsedValue = this.ParseValue(BaseUri, Value);

					if (!(ParsedUri is null) || Uri.TryCreate(BaseUri, Name, out ParsedUri))
						this.Add(new SemanticTriple(Subject, new UriNode(ParsedUri, Name), ParsedValue));
				}
			}
		}

		private void AddType(ISemanticElement Subject, object Value, Uri BaseUri, Dictionary<string, object> Context)
		{
			if (Value is string s)
			{
				if (!(Context is null) &&
					Context.TryGetValue(s, out object ContextValue))
				{
					s = ContextValue as string;
					if (string.IsNullOrEmpty(s))
						throw this.ParsingException("Invalid @type: " + ContextValue?.ToString());
				}

				this.Add(new SemanticTriple(Subject, RdfDocument.RdfType, this.CreateUriNode(s, BaseUri)));
			}
			else if (Value is Array A)
			{
				foreach (object Item in A)
					this.AddType(Subject, Item, BaseUri, Context);
			}
			else
				throw this.ParsingException("Unsupported @type: " + Value?.ToString());
		}

		private ISemanticElement ParseValue(Uri BaseUri, object Value)
		{
			if (Value is Dictionary<string, object> Obj)
			{
				if (Obj.TryGetValue("@id", out Value) && Value is string s)
					return this.CreateUriNode(s, BaseUri);
				else
					throw new NotImplementedException();    // TODO
			}
			else if (Value is Uri Uri)
				return new UriNode(Uri, Uri.ToString());
			else
				return SemanticElements.EncapsulateLiteral(Value);
		}

		/// <summary>
		/// Original XML of document.
		/// </summary>
		public Dictionary<string, object> JsonObject => this.json;

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
