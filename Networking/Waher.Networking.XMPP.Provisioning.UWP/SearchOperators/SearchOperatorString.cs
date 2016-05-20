using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Meta-data string tag.
	/// </summary>
	public abstract class SearchOperatorString : SearchOperator
	{
		private string value;

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

		internal override void SerializeValue(StringBuilder Request)
		{
			Request.Append("' value='");
			Request.Append(XML.Encode(this.value));
		}
	}
}
