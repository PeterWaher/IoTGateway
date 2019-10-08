using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for list constructs.
	/// </summary>
	public abstract class Asn1List : Asn1ComplexType
	{
		private readonly Asn1Node[] nodes;

		/// <summary>
		/// Abstract base class for list constructs.
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1List(string Name, bool TypeDef, Asn1Node[] Nodes, bool Implicit)
			: base(Name, TypeDef, Implicit)
		{
			this.nodes = Nodes;
		}

		/// <summary>
		/// Nodes
		/// </summary>
		public Asn1Node[] Nodes => this.nodes;
	}
}
