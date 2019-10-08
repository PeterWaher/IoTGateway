using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Asn1;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SEQUENCE OF construct.
	/// </summary>
	public class Asn1SequenceOf : Asn1ComplexType
	{
		private readonly Asn1Values size;
		private readonly Asn1Type elementType;

		/// <summary>
		/// Represents a ASN.1 SEQUENCE OF construct.
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Size">Optional SIZE</param>
		/// <param name="ElementType">Element type.</param>
		public Asn1SequenceOf(string Name, bool TypeDef, Asn1Values Size, Asn1Type ElementType)
			: base(Name, TypeDef)
		{
			this.size = Size;
			this.elementType = ElementType;
		}

		/// <summary>
		/// Optional SIZE
		/// </summary>
		public Asn1Values Size => this.size;

		/// <summary>
		/// Element Type
		/// </summary>
		public Asn1Type ElementType => this.elementType;

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => this.elementType.CSharpTypeReference + "[]";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => true;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportSettings Settings,
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Preprocess && this.TypeDefinition)
			{
				Output.Append(Tabs(Indent));
				Output.Append("using ");
				Output.Append(this.Name);
				Output.Append(" = Array<");
				Output.Append(this.elementType.CSharpTypeReference);
				Output.AppendLine(">;");
				Output.AppendLine();
			}
		}

	}
}
