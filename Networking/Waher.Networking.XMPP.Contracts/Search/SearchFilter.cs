using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Abstract base class for Smart Contract Search filters.
	/// </summary>
	public abstract class SearchFilter
	{
		private readonly SearchFilterOperand[] operands;

		/// <summary>
		/// Abstract base class for Smart Contract Search filters.
		/// </summary>
		/// <param name="Operands">Operands</param>
		public SearchFilter(SearchFilterOperand[] Operands)
		{
			this.operands = Operands;
		}

		/// <summary>
		/// Operands.
		/// </summary>
		public SearchFilterOperand[] Operands => this.operands;

		/// <summary>
		/// Serializes the search filter to XML.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		public virtual void Serialize(StringBuilder Xml)
		{
			Xml.Append('<');
			Xml.Append(this.ElementName);
			Xml.Append('>');

			foreach (SearchFilterOperand Op in this.operands)
				Op.Serialize(Xml, string.Empty);

			Xml.Append("</");
			Xml.Append(this.ElementName);
			Xml.Append('>');
		}

		/// <summary>
		/// Local XML element name of filter.
		/// </summary>
		public abstract string ElementName
		{
			get;
		}
	}
}
