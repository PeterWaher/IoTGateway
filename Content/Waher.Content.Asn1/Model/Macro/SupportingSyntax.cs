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
	}
}
