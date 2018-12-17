using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Abstract base class for search filters relating to the roles of contracts.
	/// </summary>
	public class FilterParameter : SearchFilter
	{
		private readonly string name;

		/// <summary>
		/// Abstract base class for search filters relating to the roles of contracts.
		/// </summary>
		/// <param name="Name">Parameter Name</param>
		/// <param name="Operands">Operands</param>
		public FilterParameter(string Name, params SearchFilterOperand[] Operands)
				: base(Operands)
		{
			this.name = Name;
		}

		/// <summary>
		/// Parameter name.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Local XML element name of filter.
		/// </summary>
		public override string ElementName => "parameter";

		/// <summary>
		/// Serializes the search filter to XML.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<parameter name='");
			Xml.Append(XML.Encode(this.name));
			Xml.Append("'>");

			foreach (SearchFilterOperand Op in this.Operands)
			{
				if (Op is LikeRegEx)
					Op.Serialize(Xml, string.Empty);
				else if (Op is SearchFilterStringOperand)
					Op.Serialize(Xml, "Str");
				else if (Op is SearchFilterNumberOperand)
					Op.Serialize(Xml, "Num");
				else
					Op.Serialize(Xml, string.Empty);
			}

			Xml.Append("</parameter>");
		}

		/// <summary>
		/// Sort order
		/// </summary>
		internal override int Order => 5;

		/// <summary>
		/// Maximum number of occurrences in a search.
		/// </summary>
		internal override int MaxOccurs => int.MaxValue;
	}
}
