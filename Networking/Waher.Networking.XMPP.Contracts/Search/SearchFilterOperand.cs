using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Abstract base class for Contract Search filter operands.
	/// </summary>
	public abstract class SearchFilterOperand
	{
		/// <summary>
		/// Abstract base class for Contract Search filter operands.
		/// </summary>
		public SearchFilterOperand()
		{
		}

		/// <summary>
		/// Serializes the search filter operand to XML.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="ElementSuffix">Suffix, to append to element name during serialization.</param>
		public abstract void Serialize(StringBuilder Xml, string ElementSuffix);

		/// <summary>
		/// Local XML element name of string operand.
		/// </summary>
		public abstract string OperandName
		{
			get;
		}

	}
}
