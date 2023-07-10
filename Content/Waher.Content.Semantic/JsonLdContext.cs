using System;
using System.Collections.Generic;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Maintains information about a JSON-LD context.
	/// </summary>
	internal class JsonLdContext
	{
		private readonly Dictionary<string, object> context;

		/// <summary>
		/// Maintains information about a JSON-LD context.
		/// </summary>
		public JsonLdContext()
			: this(null, null)
		{
		}

		/// <summary>
		/// Maintains information about a JSON-LD context.
		/// </summary>
		/// <param name="Context">Parsed JSON Context.</param>
		/// <param name="BaseUri">Base URI</param>
		public JsonLdContext(Dictionary<string, object> Context, Uri BaseUri)
		{
			this.context = new Dictionary<string, object>();

			if (!(Context is null))
			{
				foreach (KeyValuePair<string, object> P in Context)
				{
					string Name = this.GetFullyQualifiedName(P.Key);
					this.context[Name] = P.Value;
					this.SetProperty(Name, P.Value, BaseUri);
				}
			}
		}

		private void SetProperty(string Name, object Value, Uri BaseUri)
		{
			switch (Name)
			{
				case "@id":
					this.Id = this.ToUri(Value, BaseUri);
					break;

				case "@type":
					this.Type = this.ToUri(Value, BaseUri);
					break;

				case "@base":
					this.Base = this.ToUri(Value, BaseUri);
					break;

				case "@version":
					if (Value is double d)
						this.Version = d;
					break;

				case "@vocab":
					this.Vocabulary = this.ToUri(Value, BaseUri);
					break;
			}
		}

		private string GetFullyQualifiedName(string Name)
		{
			int i = Name.IndexOf(':');
			if (i >= 0)
			{
				string Prefix = Name.Substring(0, i);

				if (this.TryGetStringValue(Prefix, out string PrefixUrl))
					return PrefixUrl + Name.Substring(i + 1);
			}

			return Name;
		}

		private Uri ToUri(object Value, Uri BaseUri)
		{
			if (Value is string s)
			{
				s = this.GetFullyQualifiedName(s);
				BaseUri = this.Base ?? BaseUri;

				if (BaseUri is null)
				{
					if (Uri.TryCreate(s, UriKind.Absolute, out Uri ParsedUri))
						return ParsedUri;
				}
				else
				{
					if (Uri.TryCreate(BaseUri, s, out Uri ParsedUri))
						return ParsedUri;
				}
			}

			return null;
		}

		/// <summary>
		/// @id property of context.
		/// </summary>
		public Uri Id { get; private set; }

		/// <summary>
		/// @type property of context.
		/// </summary>
		public Uri Type { get; private set; }

		/// <summary>
		/// @base property of context.
		/// </summary>
		public Uri Base { get; private set; }

		/// <summary>
		/// @version property of context.
		/// </summary>
		public double Version { get; private set; }

		/// <summary>
		/// @vocab property of context.
		/// </summary>
		public Uri Vocabulary { get; private set; }

		/// <summary>
		/// Gives access to a property, given its name.
		/// </summary>
		/// <param name="Name">Name of property.</param>
		/// <returns>Property value.</returns>
		public object this[string Name]
		{
			get
			{
				if (this.context.TryGetValue(Name, out object Value))
					return Value;
				else
					return null;
			}
		}

		/// <summary>
		/// Tries to get a string value from the context.
		/// </summary>
		/// <param name="Name">Name of property.</param>
		/// <param name="StringValue">String Value, if found.</param>
		/// <returns>If a string value with the given name was found.</returns>
		public bool TryGetStringValue(string Name, out string StringValue)
		{
			if (this.context.TryGetValue(Name, out object Value) && Value is string s)
			{
				StringValue = s;
				return true;
			}
			else
			{
				StringValue = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to get an untyped value from the context.
		/// </summary>
		/// <param name="Name">Name of property.</param>
		/// <param name="Value">Untyped Value, if found.</param>
		/// <returns>If a value with the given name was found.</returns>
		public bool TryGetObjectValue(string Name, out object Value)
		{
			return this.context.TryGetValue(Name, out Value);
		}

		/// <summary>
		/// Appends a context object with the another.
		/// </summary>
		/// <param name="Context">Context object to append to this context.</param>
		/// <param name="BaseUri">Base URI</param>
		public void Append(JsonLdContext Context, Uri BaseUri)
		{
			foreach (KeyValuePair<string, object> P in Context.context)
			{
				string Name = this.GetFullyQualifiedName(P.Key);
				object Value = P.Value;

				if (this.context.TryGetValue(Name, out object Prev) &&
					Prev is string PrevString &&
					Uri.TryCreate(BaseUri, PrevString, out Uri PrevUri) &&
					Uri.TryCreate(PrevUri, P.Value?.ToString(), out Uri NewUri))
				{
					Value = NewUri.AbsoluteUri;
				}

				this.context[Name] = Value;
				this.SetProperty(Name, Value, BaseUri);
			}
		}
	}
}
