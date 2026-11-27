using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// Represents an ASN.1 property type reference.
	/// </summary>
	public class Asn1PropertyTypeReference : Asn1Type
	{
		private readonly string className;
		private readonly string classSet;
		private readonly string identifier;
		private readonly string lookupKey;

		/// <summary>
		/// Represents an ASN.1 property type reference.
		/// </summary>
		/// <param name="ClassName">Class name.</param>
		/// <param name="ClassSet">Optional class set.</param>
		/// <param name="Identifier">Identifier</param>
		/// <param name="LookupKey">Optional lookup key.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1PropertyTypeReference(string ClassName, string ClassSet, 
			string Identifier, string LookupKey, Asn1Document Document)
			: base(Document)
		{
			this.className = ClassName;
			this.classSet = ClassSet;
			this.identifier = Identifier;
			this.lookupKey = LookupKey;
		}

		/// <summary>
		/// Class name.
		/// </summary>
		public string ClassName => this.className;

		/// <summary>
		/// Optional class set.
		/// </summary>
		public string ClassSet => this.classSet;

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => this.identifier;

		/// <summary>
		/// Optional lookup key.
		/// </summary>
		public string LookupKey => this.lookupKey;

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Reference: ");
			sb.Append(this.className);
			sb.Append('.');
			sb.Append(this.identifier);

			if (!string.IsNullOrEmpty(this.lookupKey))
			{
				sb.Append(" (");
				sb.Append(this.lookupKey);
				sb.Append(')');
			}

			return sb.ToString();
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType
		{
			get
			{
				if (this.Document.aliases.TryGetValue(this.identifier, out Asn1TypeDefinition TypeDef))
				{
					string s = TypeDef.CSharpType;

					if (!TypeDef.ByReference &&
						(this.Optional ?? false) &&
						!(TypeDef.Optional ?? false))
					{
						s += "?";
					}

					return s;
				}
				else if (this.Document.TryGetNamedNode(this.identifier, out Asn1Node Node))
					return Node.CSharpType;
				else
					return ToCSharp(this.identifier);
			}
		}

		/// <summary>
		/// If the type is passed by reference.
		/// </summary>
		public override bool ByReference
		{
			get
			{
				if (this.Document.TryGetNamedNode(this.identifier, out Asn1Type Type))
					return Type.ByReference;
				else
					return true;
			}
		}

		/// <summary>
		/// If the type is a constructed type.
		/// </summary>
		public override bool ConstructedType
		{
			get
			{
				if (this.Document.TryGetNamedNode(this.identifier, out Asn1Type Type))
					return Type.ConstructedType;
				else
					return true;
			}
		}

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags
		{
			get
			{
				if (this.Document.aliases.TryGetValue(this.identifier, out Asn1TypeDefinition TypeDef))
					return TypeDef.AcceptedTags;
				else if (this.Document.TryGetNamedNode(this.identifier, out Asn1Node Node))
					return Node.AcceptedTags;
				else
					return base.AcceptedTags;
			}
		}

		/// <summary>
		/// If the node is optional
		/// </summary>
		public override bool? Optional
		{
			get
			{
				if (base.Optional.HasValue)
					return base.Optional;
				else if (this.Document.aliases.TryGetValue(this.identifier, out Asn1TypeDefinition TypeDef))
					return TypeDef.Optional;
				else if (this.Document.TryGetNamedNode(this.identifier, out Asn1Node Node))
					return Node.Optional;
				else
					return base.Optional;
			}
		}

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override async Task ExportCSharp(StringBuilder Output, CSharpExportState State, int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
			{
				if (this.Document.aliases.TryGetValue(this.identifier, out Asn1TypeDefinition TypeDef))
					await TypeDef.Definition.ExportCSharp(Output, State, Indent, Pass);
				else if (this.Document.TryGetNamedNode(this.identifier, out Asn1Node Node))
					Output.Append(ToCSharp(State, this, Node.CSharpType));
				else
					Output.Append(ToCSharp(State, this, this.identifier));
			}
		}

		/// <summary>
		/// Gets a C# identifier reference for the referenced type.
		/// </summary>
		/// <param name="State">C# export state.</param>
		/// <returns>C# identifier.</returns>
		public async Task<string> GetCSharpIdentifier(CSharpExportState State)
		{
			if (this.Document.aliases.TryGetValue(this.identifier, out Asn1TypeDefinition TypeDef))
				return await this.GetCSharpIdentifier(State, TypeDef);
			else if (this.Document.TryGetNamedNode(this.identifier, out Asn1Node Node))
			{
				if (Node is Asn1TypeDefinition TypeDef2)
					return await this.GetCSharpIdentifier(State, TypeDef2);
				else
					return ToCSharp(State, Node, this.identifier);
			}
			else
				return ToCSharp(State, this, this.identifier);
		}

		/// <summary>
		/// Gets a C# identifier reference for the referenced type.
		/// </summary>
		/// <param name="State">C# export state.</param>
		/// <param name="TypeDef">Type definition</param>
		/// <returns>C# identifier.</returns>
		public async Task<string> GetCSharpIdentifier(CSharpExportState State,
			Asn1TypeDefinition TypeDef)
		{
			string s;

			if ((TypeDef.ConstructedType && !(TypeDef.Definition is Asn1TypeReference)) ||
				TypeDef.Definition is Asn1Enumeration)
			{
				s = ToCSharp(State, TypeDef.Definition, this.identifier);
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				await TypeDef.Definition.ExportCSharp(sb, State, 0, CSharpExportPass.Explicit);
				s = sb.ToString();
			}

			if (!TypeDef.ByReference &&
				(this.Optional ?? false) &&
				!(TypeDef.Optional ?? false))
			{
				s += "?";
			}

			return s;
		}

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			if (!Macro.Document.TryGetNamedNode(this.identifier, out Asn1Node Node))
				throw Document.SyntaxError("Type named " + this.identifier + " not found.");

			if (Node is Asn1Type Type)
				return Type.Parse(Document, Macro);
			else if (Node is Asn1FieldDefinition FieldDef)
				return FieldDef.Type.Parse(Document, Macro);

			throw Document.SyntaxError("Type expected.");
		}

		/// <summary>
		/// Exports read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="Index">Item index.</param>
		public override Task ExportReadCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf, int Index)
		{
			if (this.Document.TryGetNamedNode(this.identifier, out Asn1Node Node))
				return Node.ExportReadCSharp(Output, State, Indent, MemberOf, Index);
			else
			{
				Output.Append(ToCSharp(State, this, this.identifier));
				Output.Append(".Create(Reader)");

				return Task.CompletedTask;
			}
		}

		/// <summary>
		/// Exports optional read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="DefaultName">Name of default value.</param>
		/// <param name="ItemName">Name of item variable.</param>
		/// <param name="Index">Item index.</param>
		public override Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string DefaultName,
			string ItemName, int Index)
		{
			if (this.Document.TryGetNamedNode(this.identifier, out Asn1Node Node))
			{
				return Node.ExportTryReadCSharp(Output, State, Indent, MemberOf,
					DefaultName, ItemName, Index);
			}
			else
			{
				if (string.IsNullOrEmpty(ItemName))
				{
					Output.Append(ToCSharp(State, this, this.identifier));
					Output.Append(".TryCreate(Reader, out var _Item) ? _Item : default");
				}
				else
				{
					Output.Append(ToCSharp(State, this, this.identifier));
					Output.Append(".TryCreate(Reader, out ");
					Output.Append(ItemName);
					Output.Append(')');
				}

				return Task.CompletedTask;
			}
		}

		/// <summary>
		/// Exports any restriction to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="VariableName">Name of variable to check.</param>
		/// <param name="Pass">Export pass</param>
		public override Task ExportRestrictionCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string VariableName,
			CSharpExportPass Pass)
		{
			if (this.Document.TryGetNamedNode(this.identifier, out Asn1Type Type))
				return Type.ExportRestrictionCSharp(Output, State, Indent, VariableName, Pass);
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
		public override Task ExportRestrictionCheck(StringBuilder Output,
			CSharpExportState State, int Indent, string VariableName)
		{
			if (this.Document.TryGetNamedNode(this.identifier, out Asn1Node Node))
				return Node.ExportRestrictionCheck(Output, State, Indent, VariableName);
			else
				return Task.CompletedTask;
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
		public override Task ExportWriteCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, int Index,
			Asn1Tags? Tags)
		{
			if (this.Document.TryGetNamedNode(this.identifier, out Asn1Node Node))
				return Node.ExportWriteCSharp(Output, State, Indent, MemberOf, Index, Tags);
			else
				throw new AsnContentException("Type " + this.identifier + " not found.");
		}

	}
}
