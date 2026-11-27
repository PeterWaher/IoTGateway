using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;
using Waher.Content.Asn1.Model.Values;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for ASN.1 types.
	/// </summary>
	public abstract class Asn1Type : Asn1Node
	{
		private Asn1Tags? tags;
		private Asn1Restriction restriction = null;
		private bool? unique;
		private bool? present;
		private bool? absent;
		private Asn1Node _default = null;
		private Asn1NamedValue[] namedOptions = null;
		private string classSetReference = null;

		/// <summary>
		/// Abstract base class for ASN.1 types.
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Type(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// How tags are encoded
		/// </summary>
		public virtual Asn1Tags? Tags
		{
			get => this.tags;
			internal set => this.tags = value;
		}

		/// <summary>
		/// Any restrictions placed on the type.
		/// </summary>
		public virtual Asn1Restriction Restriction
		{
			get => this.restriction;
			internal set
			{
				this.restriction?.SetParentNode(null);
				this.restriction = value;
				this.restriction?.SetParentNode(this);
			}
		}

		/// <summary>
		/// If values must be unique.
		/// </summary>
		public virtual bool? Unique
		{
			get => this.unique;
			internal set => this.unique = value;
		}

		/// <summary>
		/// If the type must be present
		/// </summary>
		public virtual bool? Present
		{
			get => this.present;
			internal set => this.present = value;
		}

		/// <summary>
		/// If the type must be absent
		/// </summary>
		public virtual bool? Absent
		{
			get => this.absent;
			internal set => this.absent = value;
		}

		/// <summary>
		/// Default value for type
		/// </summary>
		public virtual Asn1Node Default
		{
			get => this._default;
			internal set
			{
				this._default?.SetParentNode(null);
				this._default = value;
				this._default?.SetParentNode(this);
			}
		}

		/// <summary>
		/// Any named options.
		/// </summary>
		public Asn1NamedValue[] NamedOptions
		{
			get => this.namedOptions;
			internal set
			{
				if (!(this.namedOptions is null))
				{
					foreach (Asn1NamedValue Value in this.namedOptions)
						Value.SetParentNode(null);
				}

				this.namedOptions = value;

				if (!(this.namedOptions is null))
				{
					foreach (Asn1NamedValue Value in this.namedOptions)
						Value.SetParentNode(this);
				}
			}
		}

		/// <summary>
		/// Optional class set reference associated with type.
		/// </summary>
		public string ClassSetReference
		{
			get => this.classSetReference;
			internal set => this.classSetReference = value;
		}

		/// <summary>
		/// If the type is a constructed type.
		/// </summary>
		public virtual bool ConstructedType => false;

		/// <summary>
		/// If the type is passed by reference.
		/// </summary>
		public virtual bool ByReference => true;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public virtual Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			return Document.ParseValue();
		}

		/// <summary>
		/// Exports any restriction to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="VariableName">Name of variable to check.</param>
		/// <param name="Pass">Export pass</param>
		public virtual Task ExportRestrictionCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string VariableName, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Variables && !(this.restriction is null))
			{
				return this.restriction.ExportRestrictionVariables(Output, State, Indent,
					VariableName);
			}
			else
				return Task.CompletedTask;
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
			if (this.restriction is null)
				return;

			JSON.Indent(Output, Indent);
			Output.Append("if (!(");
			await this.restriction.RenderChecks(Output, State, VariableName, this);
			Output.AppendLine("))");

			JSON.Indent(Output, Indent + 1);
			Output.Append("throw new Asn1F.AsnContentException(\"");
			
			if (VariableName.StartsWith("this."))
				Output.Append(VariableName.Substring(5));
			else
				Output.Append(VariableName);
			
			Output.AppendLine(" out of bounds.\");");
		}

	}
}
