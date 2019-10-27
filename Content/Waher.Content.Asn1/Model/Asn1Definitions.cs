using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Asn1.Model.Values;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// How ASN.1 tags are managed.
	/// </summary>
	public enum Asn1Tags
	{
		/// <summary>
		/// Automatically
		/// </summary>
		Automatic,

		/// <summary>
		/// Implicitly
		/// </summary>
		Implicit,

		/// <summary>
		/// Explicitly
		/// </summary>
		Explicit
	}

	/// <summary>
	/// Represents a collection of ASN.1 definitions.
	/// </summary>
	public class Asn1Definitions : Asn1Node
	{
		private readonly string identifier;
		private readonly Asn1Oid oid;
		private readonly Asn1Tags? tags;
		private readonly bool _abstract;
		private readonly Asn1Module body;
		private readonly Asn1Document document;

		/// <summary>
		/// Represents a collection of ASN.1 definitions.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Oid">Optional Object ID</param>
		/// <param name="Tags">How tags are handled.</param>
		/// <param name="Abstract">If abstract syntax is used.</param>
		/// <param name="Body">Definition body.</param>
		/// <param name="Document">ASN.1 document</param>
		public Asn1Definitions(string Identifier, Asn1Oid Oid, Asn1Tags? Tags, bool Abstract,
			Asn1Module Body, Asn1Document Document)
		{
			this.identifier = Identifier;
			this.oid = Oid;
			this.tags = Tags;
			this._abstract = Abstract;
			this.body = Body;
			this.document = Document;
		}

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => this.identifier;

		/// <summary>
		/// Optional Object ID
		/// </summary>
		public Asn1Oid Oid => this.oid;

		/// <summary>
		/// How tags are handled.
		/// </summary>
		public Asn1Tags? Tags => this.tags;

		/// <summary>
		/// If abstract syntax is used.
		/// </summary>
		public bool Abstract => this._abstract;

		/// <summary>
		/// Definition body.
		/// </summary>
		public Asn1Module Body => this.body;

		/// <summary>
		/// ASN.1 document of which the definition is part.
		/// </summary>
		public Asn1Document Document => this.document;

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
			if (Pass == CSharpExportPass.Explicit && !(this.body is null))
			{
				if (!(this.body.Imports is null))
				{
					foreach (Asn1Import Import in this.body.Imports)
					{
						if (string.IsNullOrEmpty(Import.Module))
							continue;

						Asn1Document ImportedDocument;

						if (State.Settings.ContainsCode(Import.Module))
							ImportedDocument = State.Settings.GetDocument(Import.Module);
						else
						{
							ImportedDocument = Import.LoadDocument(State.Settings);
							string CSharp = ImportedDocument.ExportCSharp(State.Settings);
							State.Settings.AddCode(Import.Module, CSharp, ImportedDocument);
						}

						Output.Append("using ");
						Output.Append(State.Settings.Namespace(Import.Module));
						Output.AppendLine(";");
					}

					Output.AppendLine();
				}
				else
					Output.AppendLine();

				Output.Append(Tabs(Indent));
				Output.Append("namespace ");
				Output.Append(State.Settings.BaseNamespace);
				Output.Append('.');
				Output.AppendLine(Asn1Node.ToCSharp(this.identifier));
				Output.Append(Tabs(Indent));
				Output.AppendLine("{");
				Indent++;

				if (!(this.body.Imports is null))
				{
					foreach (Asn1Import Import in this.body.Imports)
					{
						if (string.IsNullOrEmpty(Import.Module))
							continue;

						Asn1Document ImportedDocument = State.Settings.GetDocument(Import.Module);
						Asn1Node[] Nodes = ImportedDocument.Root?.Body?.Items;

						if (!(Nodes is null))
						{
							foreach (Asn1Node Node in Nodes)
							{
								if (Node is Asn1TypeDefinition TypeDef &&
									!TypeDef.ConstructedType &&
									Array.IndexOf<string>(Import.Identifiers, TypeDef.TypeName) >= 0)
								{
									TypeDef.ExportCSharp(Output, State, Indent, CSharpExportPass.Preprocess);
								}
							}
						}
					}
				}

				this.body.ExportCSharp(Output, State, Indent, CSharpExportPass.Preprocess);
				State.ClosePending(Output);

				this.body.ExportCSharp(Output, State, Indent, CSharpExportPass.Variables);
				State.ClosePending(Output);

				this.body.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
				State.ClosePending(Output);

				Indent--;
				Output.Append(Tabs(Indent));
				Output.AppendLine("}");
			}
		}
	}
}
