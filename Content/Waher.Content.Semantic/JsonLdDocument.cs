using System;
using System.Collections.Generic;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Contains semantic information stored in a JSON-LD document.
	/// 
	/// Ref: https://www.w3.org/TR/json-ld/
	/// </summary>
	public class JsonLdDocument : InMemorySemanticCube
	{
		private readonly Dictionary<string, object> json;
		private readonly string text;

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of RDF document.</param>
		public JsonLdDocument(string Text)
			: this(ParseJson(Text), Text, null)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		public JsonLdDocument(string Text, Uri BaseUri)
			: this(ParseJson(Text), Text, BaseUri, "n")
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		public JsonLdDocument(string Text, Uri BaseUri, string BlankNodeIdPrefix)
			: this(ParseJson(Text), Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode.Sequential)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		/// <param name="BlankNodeIdMode">How Blank Node IDs are generated</param>
		public JsonLdDocument(string Text, Uri BaseUri, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
			: this(ParseJson(Text), Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode)
		{
		}

		internal static Dictionary<string, object> ParseJson(string Text)
		{
			object Parsed = JSON.Parse(Text);

			if (Parsed is Dictionary<string, object> Obj)
				return Obj;
			else
				throw new ArgumentException("Invalid JSON-LD document.", nameof(Text));
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of RDF document.</param>
		public JsonLdDocument(Dictionary<string, object> Doc, string Text)
			: this(Doc, Text, null)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		public JsonLdDocument(Dictionary<string, object> Doc, string Text, Uri BaseUri)
			: this(Doc, Text, BaseUri, "n")
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		public JsonLdDocument(Dictionary<string, object> Doc, string Text, Uri BaseUri, string BlankNodeIdPrefix)
			: this(Doc, Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode.Sequential)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Doc">Parsed JSON object.</param>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		/// <param name="BlankNodeIdMode">How Blank Node IDs are generated</param>
		public JsonLdDocument(Dictionary<string, object> Doc, string Text, Uri BaseUri, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
		{
			this.json = Doc;
			this.text = Text;
		}

		/// <summary>
		/// Original XML of document.
		/// </summary>
		public Dictionary<string, object> JsonObject => this.json;

		/// <summary>
		/// Text representation.
		/// </summary>
		public string Text => this.text;
	}
}
