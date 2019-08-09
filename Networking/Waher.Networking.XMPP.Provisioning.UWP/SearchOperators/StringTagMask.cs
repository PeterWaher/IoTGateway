using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named string-valued tag like a given value.
	/// </summary>
	public class StringTagMask : SearchOperatorString
	{
		private readonly string wildcard;

		/// <summary>
		/// Filters things with a named string-valued tag like a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		/// <param name="Wildcard">Wildcard string used.</param>
		public StringTagMask(string Name, string Value, string Wildcard)
			: base(Name, Value)
		{
			this.wildcard = Wildcard;
		}

		/// <summary>
		/// Wildcard string used.
		/// </summary>
		public string Wildcard
		{
			get { return this.wildcard; }
		}

		internal override void SerializeValue(StringBuilder Request)
		{
			base.SerializeValue(Request);

			Request.Append("' wildcard='");
			Request.Append(XML.Encode(this.wildcard));
		}

		internal override string TagName
		{
			get { return "strMask"; }
		}
	}
}
