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
		private readonly string fieldName;

		/// <summary>
		/// Abstract base class for list constructs.
		/// </summary>
		/// <param name="FieldName">Optional field name.</param>
		/// <param name="Nodes">Nodes</param>
		public Asn1List(string FieldName, Asn1Node[] Nodes)
			: base()
		{
			this.nodes = Nodes;
			this.fieldName = FieldName;
		}

		/// <summary>
		/// Nodes
		/// </summary>
		public Asn1Node[] Nodes => this.nodes;

		/// <summary>
		/// Optional field name.
		/// </summary>
		public string FieldName => this.fieldName;
	}
}
