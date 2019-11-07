using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// Supporting syntax in a macro
	/// </summary>
	public class SupportingSyntax : UserDefinedItem
	{
		private readonly string name;
		private readonly UserDefinedItem notation;

		/// <summary>
		/// Type notation declaration
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Notation">Notation</param>
		public SupportingSyntax(string Name, UserDefinedItem Notation)
		{
			this.name = Name;
			this.notation = Notation;
		}

		/// <summary>
		/// Name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Notation
		/// </summary>
		public UserDefinedItem Notation => this.notation;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the
		/// instructions available in the macro.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro being executed.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			return this.notation.Parse(Document, Macro);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.name;
		}
	}
}
