using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Asn1.Model.Values;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 field definition.
	/// </summary>
	public class Asn1FieldDefinition : Asn1Node
	{
		private readonly string fieldName;
		private readonly Asn1Type type;
		private int? tag;

		/// <summary>
		/// Represents an ASN.1 field definition.
		/// </summary>
		/// <param name="FieldName">Field name.</param>
		/// <param name="Tag">Tag</param>
		/// <param name="Type">Type.</param>
		public Asn1FieldDefinition(string FieldName, int? Tag, Asn1Type Type)
			: base()
		{
			this.fieldName = FieldName;
			this.type = Type;
			this.tag = Tag;
		}

		/// <summary>
		/// Field Name
		/// </summary>
		public string FieldName => this.fieldName;

		/// <summary>
		/// Type
		/// </summary>
		public Asn1Type Type => this.type;

		/// <summary>
		/// Tag
		/// </summary>
		public int? Tag
		{
			get => this.tag;
			internal set => this.tag = value;
		}

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
			if (Pass == CSharpExportPass.Explicit)
			{
				Output.Append(Tabs(Indent));
				Output.Append("public ");
				Output.Append(this.type.CSharpTypeReference);

				if (this.type.Optional.HasValue && this.type.Optional.Value &&
					!this.type.CSharpTypeNullable)
				{
					Output.Append('?');
				}

				Output.Append(' ');
				Output.Append(this.fieldName);

				if (!(this.type.Default is null))
				{
					Output.Append(" = ");
					this.type.Default.ExportCSharp(Output, Settings, Indent, Pass);
				}

				Output.AppendLine(";");
			}
			else
				this.type.ExportCSharp(Output, Settings, Indent, Pass);
		}
	}
}
