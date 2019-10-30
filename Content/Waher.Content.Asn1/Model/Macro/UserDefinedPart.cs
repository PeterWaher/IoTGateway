using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// Un-typed user-defined part.
	/// </summary>
	public class UserDefinedPart : UserDefinedItem
	{
		private readonly string identifier;

		/// <summary>
		/// Un-typed user-defined part.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		public UserDefinedPart(string Identifier)
		{
			this.identifier = Identifier;
		}

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => this.identifier;
	}
}
