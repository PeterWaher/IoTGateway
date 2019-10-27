using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Represents an ASN.1 Object ID
	/// </summary>
	public class Asn1Oid : Asn1Value
	{
		private readonly Asn1Node[] values;

		/// <summary>
		/// Represents an ASN.1 Object ID
		/// </summary>
		/// <param name="Values">OID values.</param>
		public Asn1Oid(Asn1Node[] Values)
		{
			this.values = Values;
		}

		/// <summary>
		/// OID values.
		/// </summary>
		public Asn1Node[] Values => this.values;

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
			if (Pass == CSharpExportPass.Explicit)
			{
				bool First = true;

				Output.Append("new ObjectId(");

				foreach (Asn1Node Node in this.values)
				{
					if (First)
						First = false;
					else
						Output.Append(", ");

					Node.ExportCSharp(Output, State, Indent, Pass);
				}

				Output.Append(')');
			}
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => "ObjectId";
	}
}
