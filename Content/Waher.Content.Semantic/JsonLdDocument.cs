using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Waher.Content.Getters;
using Waher.Content.Json;
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
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		/// <param name="BlankNodeIdMode">How Blank Node IDs are generated</param>
		private JsonLdDocument(Dictionary<string, object> Doc, string Text, Uri BaseUri, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
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
			JsonLdDocument Result = new JsonLdDocument(Doc, Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode);
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

			foreach (KeyValuePair<string, object> P in Doc)
			{
				string Name = P.Key;
				ISemanticElement Value;

				if (Name.StartsWith("@"))
				{
					switch (Name)
					{
						case "@context":
							if (P.Value is Dictionary<string, object> ContextObj)
								Context = ContextObj;
							else
							{
								Uri ContextUri;

								if (P.Value is string Url)
									ContextUri = this.CreateUri(Url, BaseUri);
								else
								{
									Value = this.ParseValue(BaseUri, P.Value);
									if (Value is UriNode UriNode)
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

										if (Obj is JsonLdDocument ContextDoc)
										{
											Context = ContextDoc.json;
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

									if (Obj is JsonLdDocument ContextDoc)
									{
										Context = ContextDoc.json;
										contextObjects[ContextUri.AbsolutePath] = new KeyValuePair<DateTimeOffset, Dictionary<string, object>>(
											ContextDoc.Date ?? DateTimeOffset.Now, Context);
									}
									else
										throw this.ParsingException("Context reference not a JSON-LD document. Expected Content-Type: " + JsonLdCodec.DefaultContentType);
								}
							}
							break;

						default:
							throw this.ParsingException("Unrecognized keyword: " + Name);
					}
				}
				else
				{
					Value = this.ParseValue(BaseUri, P.Value);

					if (Uri.TryCreate(BaseUri, Name, out Uri NameUri))
						this.Add(new SemanticTriple(Subject, new UriNode(NameUri, Name), Value));
				}
			}
		}

		private ISemanticElement ParseValue(Uri BaseUri, object Value)
		{
			if (Value is Dictionary<string, object> Obj)
			{
				if (Obj.TryGetValue("@id", out Value) && Value is string s)
					return this.CreateUriNode(s, BaseUri);
				else
					throw new NotImplementedException();	// TODO
            }
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
