using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Abstract base class for all search operators.
	/// </summary>
	public abstract class SearchOperator
	{
		private readonly string name;
		private readonly string nameWildcard;
		private readonly bool hasNameWildcard;
		/*
		/// <summary>
		/// Abstract base class for all search operators.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		public SearchOperator(string Name)
			: this(Name, null)
		{
		}
		*/
		/// <summary>
		/// Abstract base class for all search operators.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="NameWildcard">Optional wildcard used in the name.</param>
		public SearchOperator(string Name, string NameWildcard)
		{
			this.name = Name;
			this.nameWildcard = NameWildcard;
			this.hasNameWildcard = !string.IsNullOrEmpty(NameWildcard);
		}

		/// <summary>
		/// Tag name.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Wildcard, if any, used in the name.
		/// </summary>
		public string NameWildcard => this.nameWildcard;

		/// <summary>
		/// If a name wildcard is used.
		/// </summary>
		public bool HasNameWildcard => this.hasNameWildcard;

		internal void Serialize(StringBuilder Request)
		{
			Request.Append('<');
			Request.Append(this.TagName);
			Request.Append(" name='");
			Request.Append(XML.Encode(this.name));

			if (this.hasNameWildcard)
			{
				Request.Append("' nameWildcard='");
				Request.Append(XML.Encode(this.nameWildcard));
			}

			this.SerializeValue(Request);

			Request.Append("'/>");
		}

		internal abstract string TagName
		{
			get;
		}

		internal abstract void SerializeValue(StringBuilder Request);
	}
}
