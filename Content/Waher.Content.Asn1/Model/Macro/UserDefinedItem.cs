using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// Abstract base class for user-defined parts in macros
	/// </summary>
	public abstract class UserDefinedItem
	{
		/// <summary>
		/// Parses the portion of the document at the current position, according to the
		/// instructions available in the macro.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro being executed.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public abstract Asn1Node Parse(Asn1Document Document, Asn1Macro Macro);
	}
}
