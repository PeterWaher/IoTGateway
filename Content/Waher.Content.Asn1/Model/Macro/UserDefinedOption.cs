using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// One option of many
	/// </summary>
	public class UserDefinedOption : UserDefinedItem
	{
		private readonly UserDefinedItem[] items;

		/// <summary>
		/// One option of many
		/// </summary>
		/// <param name="Items">Items comprising option</param>
		public UserDefinedOption(UserDefinedItem[] Items)
		{
			this.items = Items;
		}

		/// <summary>
		/// Items comprising option
		/// </summary>
		public UserDefinedItem[] Items => this.items;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the
		/// instructions available in the macro.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro being executed.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			Asn1Node Result = null;

			foreach (UserDefinedItem Item in this.items)
				Result = Item.Parse(Document, Macro);

			return Result;
		}
	}
}
