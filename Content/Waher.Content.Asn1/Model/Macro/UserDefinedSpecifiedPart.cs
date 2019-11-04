using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// Typed user-defined part.
	/// </summary>
	public class UserDefinedSpecifiedPart : UserDefinedPart
	{
		private readonly string name;
		private readonly Asn1Type type;

		/// <summary>
		/// Typed user-defined part.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Name">Name</param>
		/// <param name="Type">Type</param>
		public UserDefinedSpecifiedPart(string Identifier, string Name, Asn1Type Type)
			: base(Identifier)
		{
			this.name = Name;
			this.type = Type;
		}

		/// <summary>
		/// Name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Type
		/// </summary>
		public Asn1Type Type => this.type;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the
		/// instructions available in the macro.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro being executed.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			switch (this.name)
			{
				case "TYPE": return Document.ParseType(this.Identifier, false);
				case "VALUE": return Document.ParseValue();
				default:
					if (this.Identifier == "type")
						return Document.ParseType(this.Identifier, false);
					else
						return this.type.Parse(Document, Macro);
			}
		}
	}
}
