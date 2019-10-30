using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// String literal
	/// </summary>
	public class UserDefinedLiteral : UserDefinedItem
	{
		private readonly string value;

		/// <summary>
		/// String literal
		/// </summary>
		/// <param name="Value">Literal value.</param>
		public UserDefinedLiteral(string Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// String literal
		/// </summary>
		public string Value => this.value;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the
		/// instructions available in the macro.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro being executed.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			Document.AssertNextToken(this.value);
			return new Values.Asn1StringValue(this.value);
		}
	}
}
