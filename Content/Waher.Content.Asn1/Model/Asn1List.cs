using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for list constructs.
	/// </summary>
	public abstract class Asn1List : Asn1Type
	{
		private readonly Asn1Node[] nodes;

		/// <summary>
		/// Abstract base class for list constructs.
		/// </summary>
		/// <param name="Nodes">Nodes</param>
		public Asn1List(Asn1Node[] Nodes)
			: base()
		{
			this.nodes = Nodes;
		}

		/// <summary>
		/// Nodes
		/// </summary>
		public Asn1Node[] Nodes => this.nodes;
	}
}
