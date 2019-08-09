using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Abstract base class for all search operators.
	/// </summary>
	public abstract class SearchOperator
	{
		private readonly string name;

		/// <summary>
		/// Abstract base class for all search operators.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		public SearchOperator(string Name)
		{
			this.name = Name;
		}

		/// <summary>
		/// Tag name.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		internal void Serialize(StringBuilder Request)
		{
			Request.Append('<');
			Request.Append(this.TagName);
			Request.Append(" name='");
			Request.Append(XML.Encode(this.name));

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
