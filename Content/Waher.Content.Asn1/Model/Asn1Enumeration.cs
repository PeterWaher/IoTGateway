using System;
using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Values;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// ENUMERATED
	/// </summary>
	public class Asn1Enumeration : Asn1List, INamedNode
	{
		private readonly Asn1Node baseType;
		private readonly bool flags;

		/// <summary>
		/// ENUMERATED
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Flags">If the enumeration lists options that can be combined.
		/// <see cref="System.FlagsAttribute"/>.</param>
		/// <param name="BaseType">Base type of enumeration.</param>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Enumeration(string Name, bool TypeDef, bool Flags, Asn1Node BaseType, 
			Asn1Node[] Nodes, Asn1Document Document)
			: base(Name, TypeDef, Nodes, Document)
		{
			this.baseType = BaseType;
			this.flags = Flags;

			this.SetAsChild(this.baseType);

			foreach (Asn1Node Node in Nodes)
			{
				if (Node is Asn1NamedValue NamedValue)
					NamedValue.EnumeratedValue = true;

				if (Flags && Node is Asn1Value Value)
					Value.TwosPower();
			}
		}

		/// <summary>
		/// If the type is a constructed type.
		/// </summary>
		public override bool ConstructedType => true;

		/// <summary>
		/// Base type.
		/// </summary>
		public Asn1Node BaseType => this.baseType;

		/// <summary>
		/// If the enumeration represents a <see cref="System.FlagsAttribute"/> enumeration.
		/// </summary>
		public bool Flags => this.flags;

		/// <summary>
		/// If the named node represents a type.
		/// </summary>
		public bool IsType => true;

		/// <summary>
		/// Gets the fully qualified name, in reverse order.
		/// </summary>
		public void GetFullyQualifiedName(ChunkedList<string> Fqn)
		{
			Fqn.Add(this.Name);

			if (this.TryGetAncestor(out INamedNode Container))
				Container.GetFullyQualifiedName(Fqn);
		}

		/// <summary>
		/// If the named element must be qualified.
		/// </summary>
		public bool MustBeQualified => false;

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType
		{
			get
			{
				string s = ToCSharp(this.Name);
				if (!this.TypeDefinition)
					s += "Enum";
				return s;
			}
		}

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => acceptedTags;

		private static readonly Asn1Tag[] acceptedTags = new Asn1Tag[]
		{
			new Asn1Tag(UniversalTagNumber.Enumerated),
			new Asn1Tag(UniversalTagNumber.Integer)
		};

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
			if (Pass == CSharpExportPass.Implicit)
			{
				State.ClosePending(Output);

				foreach (Asn1Node Node in this.Nodes)
					await Node.ExportCSharp(Output, State, Indent, Pass);

				if (this.flags)
				{
					JSON.Indent(Output, Indent);
					Output.AppendLine("[System.Flags]");
				}

				JSON.Indent(Output, Indent);
				Output.Append("public enum ");
				Output.Append(ToCSharp(this.Name));
				if (!this.TypeDefinition)
					Output.Append("Enum");
				Output.AppendLine();

				JSON.Indent(Output, Indent);
				Output.AppendLine("{");

				Indent++;

				bool First = true;

				foreach (Asn1Node Node in this.Nodes)
				{
					if (First)
						First = false;
					else
						Output.AppendLine(",");

					JSON.Indent(Output, Indent);
					await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
				}

				Indent--;

				Output.AppendLine();
				JSON.Indent(Output, Indent);
				Output.AppendLine("}");
				Output.AppendLine();
			}
			else if (Pass == CSharpExportPass.Explicit)
			{
				if (this.TypeDefinition)
				{
					State.ClosePending(Output);

					if (this.flags)
					{
						JSON.Indent(Output, Indent);
						Output.AppendLine("[System.Flags]");
					}

					JSON.Indent(Output, Indent);
					Output.Append("public enum ");
					Output.AppendLine(ToCSharp(this.Name));

					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Implicit);

					bool First = true;

					foreach (Asn1Node Node in this.Nodes)
					{
						if (First)
							First = false;
						else
							Output.AppendLine(",");

						JSON.Indent(Output, Indent);
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
					}

					if (!First)
						Output.AppendLine();

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");
					Output.AppendLine();
				}
				else
				{
					Output.Append(ToCSharp(State, this, this.Name));
					Output.Append("Enum");
				}
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
		public override Task ExportReadCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf, int Index)
		{
			Output.Append("ReadEnumeration");

			if (this.Tags.HasValue)
				Output.Append(this.Tags.Value.ToString());

			Output.Append('<');
			Output.Append(ToCSharp(State, this, this.Name));
			if (!this.TypeDefinition)
				Output.Append("Enum");
			Output.Append(">(Reader");

			if (this.Tags.HasValue)
			{
				Output.Append(", ");
				Output.Append(Index.ToString());
			}

			Output.Append(')');

			return Task.CompletedTask;
		}

		/// <summary>
		/// Exports optional read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="ItemName">Name of item variable.</param>
		/// <param name="Index">Item index.</param>
		public override Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string ItemName,
			int Index)
		{
			Output.Append("TryReadEnumeration");

			if (this.Tags.HasValue)
				Output.Append(this.Tags.Value.ToString());

			Output.Append('<');
			Output.Append(ToCSharp(State, this, this.Name));
			if (!this.TypeDefinition)
				Output.Append("Enum");
			Output.Append(">(Reader");

			if (this.Tags.HasValue)
			{
				Output.Append(", ");
				Output.Append(Index.ToString());
			}

			Output.Append(", out ");

			if (string.IsNullOrEmpty(ItemName))
				Output.Append("var Item) ? Item : null");
			else
			{
				Output.Append(ItemName);
				Output.Append(')');
			}

			return Task.CompletedTask;
		}
	}
}
