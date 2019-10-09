using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for complex types.
	/// </summary>
	public abstract class Asn1ComplexType : Asn1Type
	{
		private readonly string name;
		private readonly bool typeDef;

		/// <summary>
		/// Abstract base class for complex types.
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		public Asn1ComplexType(string Name, bool TypeDef)
			: base()
		{
			this.name = Name;
			this.typeDef = TypeDef;
		}

		/// <summary>
		/// Optional field or type name.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// If construct is part of a type definition.
		/// </summary>
		public bool TypeDefinition => this.typeDef;
	}
}
