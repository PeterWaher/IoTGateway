using System;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Meta-data string tag.
	/// </summary>
	public abstract class SearchOperatorString : SearchOperator
	{
		private readonly string value;

		/// <summary>
		/// Meta-data string tag.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public SearchOperatorString(string Name, string Value)
			: base(Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// Operator value.
		/// </summary>
		public string Value => this.value;

		internal override void SerializeValue(StringBuilder Request)
		{
			Request.Append("' value='");
			Request.Append(XML.Encode(this.value));
		}
	}
}
