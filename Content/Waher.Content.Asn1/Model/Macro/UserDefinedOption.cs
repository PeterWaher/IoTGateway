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
	}
}
