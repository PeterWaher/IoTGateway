using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 module.
	/// </summary>
	public class Asn1Module : Asn1Node
	{
		private readonly Asn1Import[] imports;
		private readonly string[] exports;
		private readonly Asn1Node[] items;

		/// <summary>
		/// Represents an ASN.1 module.
		/// </summary>
		/// <param name="Imports">Optional Import instructions.</param>
		/// <param name="Exports">Optional Export instructions.</param>
		/// <param name="Items">Items in module.</param>
		public Asn1Module(Asn1Import[] Imports, string[] Exports, Asn1Node[] Items)
		{
			this.imports = Imports;
			this.exports = Exports;
			this.items = Items;
		}

		/// <summary>
		/// Optional Import instructions.
		/// </summary>
		public Asn1Import[] Imports => this.imports;

		/// <summary>
		/// Optional Export instructions.
		/// </summary>
		public string[] Exports => this.exports;

		/// <summary>
		/// Items in module.
		/// </summary>
		public Asn1Node[] Items => this.items;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportState State, 
			int Indent, CSharpExportPass Pass)
		{
			foreach (Asn1Node Item in this.items)
				Item.ExportCSharp(Output, State, Indent, Pass);
		}
	}
}
