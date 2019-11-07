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
			int BestPos = -1;
			Asn1Node Best = null;
			Asn1Node Option;

			foreach (UserDefinedItem Item in this.options)
			{
				Document.pos = Bak;

				try
				{
					Option = Item.Parse(Document, Macro);
					if (Best is null || Document.pos > BestPos)
					{
						BestPos = Document.pos;
						Best = Option;
					}
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			if (BestPos < 0)
				throw Document.SyntaxError("Invalid option.");

			Document.pos = BestPos;

			return Best;
		}
	}
}
