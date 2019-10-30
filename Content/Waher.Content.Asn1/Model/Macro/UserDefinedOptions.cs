using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// Set of options
	/// </summary>
	public class UserDefinedOptions : UserDefinedItem
	{
		private readonly UserDefinedItem[] options;

		/// <summary>
		/// Set of options
		/// </summary>
		/// <param name="Options">Options</param>
		public UserDefinedOptions(UserDefinedItem[] Options)
		{
			this.options = Options;
		}

		/// <summary>
		/// Options
		/// </summary>
		public UserDefinedItem[] Options => this.options;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the
		/// instructions available in the macro.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro being executed.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			int Bak = Document.pos;

			foreach (UserDefinedItem Item in this.options)
			{
				Document.pos = Bak;

				try
				{
					return Item.Parse(Document, Macro);
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			throw Document.SyntaxError("Invalid option.");
		}
	}
}
