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
	}
}
