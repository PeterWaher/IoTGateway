using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Types;
using Waher.Content.Asn1.Model.Values;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Defines a property name and value in a class instance definition.
	/// </summary>
	public class Asn1PropertyValue : Asn1Node
	{
		private readonly Asn1Value value;
		private readonly string name;
		private Asn1Type type = null;

		/// <summary>
		/// Defines a property name and value in a class instance definition.
		/// </summary>
		/// <param name="Name">Property value.</param>
		/// <param name="Value">Type definition.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1PropertyValue(string Name, Asn1Value Value, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.value = Value;
		}

		/// <summary>
		/// Property name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Property value
		/// </summary>
		public Asn1Value Value => this.value;

		/// <summary>
		/// Associated type of property.
		/// </summary>
		public Asn1Type Type
		{
			get => this.type;
			internal set => this.type = value;
		}

		/// <inheritdoc/>
		public override string ToString() => this.name + ": " + this.value.ToString();

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override async Task ExportCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
			{
				JSON.Indent(Output, Indent);
				Output.Append("this.");
				Output.Append(ToCSharp(this.name));
				Output.Append(" = ");
				await this.value.ExportCSharp(Output, State, Indent + 1, Pass);
				Output.AppendLine(";");
			}
			else
			{
				if (Pass == CSharpExportPass.Preprocess || Pass == CSharpExportPass.Variables)
				{
					await this.type.ExportRestrictionCSharp(Output, State, Indent,
						ToCSharp(this.name), Pass);
				}

				await this.type.ExportCSharp(Output, State, Indent, Pass);
				await this.value.ExportCSharp(Output, State, Indent, Pass);
			}
		}

		/// <summary>
		/// Exports read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="Index">Item index.</param>
		public override async Task ExportReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, int Index)
		{
			string Name = MemberOf + "." + ToCSharp(this.name);

			if (this.type.Optional ?? false)
			{
				await this.ExportTryReadCSharp(Output, State, Indent, string.Empty,
					Name, Name, Index);
			}
			else
			{
				JSON.Indent(Output, Indent);
				Output.Append(Name);
				Output.Append(" = ");

				await this.type.ExportReadCSharp(Output, State, Indent + 1, string.Empty,
					Index);

				Output.AppendLine(";");
			}
		}

		/// <summary>
		/// Exports read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="VariableName">Name of variable to check.</param>
		public override async Task ExportRestrictionCheck(StringBuilder Output,
			CSharpExportState State, int Indent, string VariableName)
		{
			StringBuilder Temp = new StringBuilder();
			string Name = VariableName + "." + ToCSharp(this.name);

			if (this.type.Optional ?? false)
				await this.type.ExportRestrictionCheck(Temp, State, Indent + 1, Name);
			else
				await this.type.ExportRestrictionCheck(Temp, State, Indent, Name);

			string Check = Temp.ToString();

			if (!string.IsNullOrWhiteSpace(Check))
			{
				if (this.type.Optional ?? false)
				{
					JSON.Indent(Output, Indent);
					Output.Append("if (!(");
					Output.Append(Name);
					Output.AppendLine(" is null))");

					if (Check.IndexOfAny(CommonTypes.CRLF) >= 0)
					{
						JSON.Indent(Output, Indent);
						Output.AppendLine("{");

						Output.Append(Check);

						JSON.Indent(Output, Indent);
						Output.AppendLine("}");
					}
					else
						Output.Append(Check);
				}
				else
					Output.Append(Check);
			}
		}

		/// <summary>
		/// Exports read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="DefaultName">Name of default value.</param>
		/// <param name="ItemName">Name of item variable.</param>
		/// <param name="Index">Item index.</param>
		public override async Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string DefaultName,
			string ItemName, int Index)
		{
			JSON.Indent(Output, Indent);

			if (string.IsNullOrEmpty(ItemName))
			{
				await this.type.ExportTryReadCSharp(Output, State, Indent, string.Empty,
					null, MemberOf + "." + ToCSharp(this.name), Index);
			}
			else
			{
				await this.type.ExportTryReadCSharp(Output, State, Indent, string.Empty,
					DefaultName, ItemName, Index);

				if (!string.IsNullOrEmpty(MemberOf))
				{
					Output.AppendLine(";");

					JSON.Indent(Output, Indent);
					Output.Append(MemberOf);
					Output.Append('.');
					Output.Append(ToCSharp(this.name));
					Output.Append(" = ");
					Output.Append(ItemName);
				}
			}

			Output.AppendLine(";");
		}

		/// <summary>
		/// Exports write logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Tags">How to encode tags.</param>
		public override async Task ExportWriteCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, int Index,
			Asn1Tags? Tags)
		{
			string Name = MemberOf + "." + ToCSharp(this.name);

			JSON.Indent(Output, Indent);

			if (this.Optional ?? this.type.Optional ?? false)
			{
				Output.Append("if (");

				if (this.type.ByReference)
				{
					Output.Append("!(");
					Output.Append(ToCSharp(Name));
					Output.AppendLine(" is null))");
				}
				else
				{
					Output.Append(ToCSharp(Name));
					Output.AppendLine(".HasValue)");
				}

				JSON.Indent(Output, Indent + 1);

				if (this.type.ConstructedType)
				{
					Output.Append(Name);
					Output.Append(".Write(Writer)");
				}
				else if (this.type.ByReference)
				{
					await this.type.ExportWriteCSharp(Output, State, Indent + 1,
						Name, Index, Tags);
				}
				else
				{
					await this.type.ExportWriteCSharp(Output, State, Indent + 1,
						Name + ".Value", Index, Tags);
				}
			}
			else if (this.type.ConstructedType)
			{
				Output.Append(Name);
				Output.Append(".Write(Writer)");
			}
			else
				await this.type.ExportWriteCSharp(Output, State, Indent, Name, Index, Tags);

			Output.AppendLine(";");
		}
	}
}
