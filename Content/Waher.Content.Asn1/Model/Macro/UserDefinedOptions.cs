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
	}
}
