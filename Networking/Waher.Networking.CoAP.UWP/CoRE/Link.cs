using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP.CoRE
{
	/// <summary>
	/// CoRE link.
	/// </summary>
	public class Link
	{
		private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();
		private readonly Uri uri;
		private Uri anchor = null;
		private string[] relationTypes = null;
		private string[] rev = null;
		private string[] resourceTypes = null;
		private string[] interfaceDescriptions = null;
		private string language = null;
		private string media = null;
		private string title = null;
		private string type = null;
		private ulong? size = null;
		private bool observable = false;

		/// <summary>
		/// CoRE link.
		/// </summary>
		/// <param name="Uri">URI</param>
		internal Link(Uri Uri)
		{
			this.uri = Uri;
		}

		internal void AddParameter(string Name, string Value)
		{
			this.parameters[Name] = Value;
		}

		/// <summary>
		/// URI of resource.
		/// </summary>
		public Uri Uri => this.uri;

		/// <summary>
		/// Relation types, if available, null otherwise.
		/// </summary>
		public string[] RelationTypes
		{
			get => this.relationTypes;
			internal set => this.relationTypes = value;
		}

		/// <summary>
		/// Rev (?), if available, null otherwise.
		/// </summary>
		public string[] Rev
		{
			get => this.rev;
			internal set => this.rev = value;
		}

		/// <summary>
		/// Anchor, if available, null otherwise.
		/// </summary>
		public Uri Anchor
		{
			get => this.anchor;
			internal set => this.anchor = value;
		}

		/// <summary>
		/// Language, if available, null otherwise.
		/// </summary>
		public string Language
		{
			get => this.language;
			internal set => this.language = value;
		}

		/// <summary>
		/// Media, if available, null otherwise.
		/// </summary>
		public string Media
		{
			get => this.media;
			internal set => this.media = value;
		}

		/// <summary>
		/// Title, if available, null otherwise.
		/// </summary>
		public string Title
		{
			get => this.title;
			internal set => this.title = value;
		}

		/// <summary>
		/// Type, if available, null otherwise.
		/// </summary>
		public string Type
		{
			get => this.type;
			internal set => this.type = value;
		}

		/// <summary>
		/// Resource types, if available, null otherwise.
		/// </summary>
		public string[] ResourceTypes
		{
			get => this.resourceTypes;
			internal set => this.resourceTypes = value;
		}

		/// <summary>
		/// Interface descriptions, if available, null otherwise.
		/// </summary>
		public string[] InterfaceDescriptions
		{
			get => this.interfaceDescriptions;
			internal set => this.interfaceDescriptions = value;
		}

		/// <summary>
		/// Size estimate, if available, null otherwise.
		/// </summary>
		public ulong? Size
		{
			get => this.size;
			internal set => this.size = value;
		}

		/// <summary>
		/// If resource is reported to be observable.
		/// </summary>
		public bool Observable
		{
			get => this.observable;
			internal set => this.observable = value;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.uri.ToString());

			foreach (KeyValuePair<string, string> P in this.parameters)
			{
				sb.Append(';');
				sb.Append(P.Key);
				if (!string.IsNullOrEmpty(P.Value))
				{
					sb.Append('=');
					sb.Append(P.Value);
				}
			}

			return sb.ToString();
		}

	}
}
