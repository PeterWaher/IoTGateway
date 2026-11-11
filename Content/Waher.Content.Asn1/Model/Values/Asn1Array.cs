using System;
using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Types;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Array of values
	/// </summary>
	public class Asn1Array : Asn1Value
	{
		private readonly Asn1Value[] values;
		private Asn1TypeReference typeReference;
		private Asn1Node type;
		private bool? isFlagsEnum;

		/// <summary>
		/// Array of values
		/// </summary>
		/// <param name="Values">Values</param>
		/// <param name="Document">ASN.1 document reference</param>
		public Asn1Array(Asn1Value[] Values, Asn1Document Document)
			: base(Document)
		{
			this.values = Values;
			this.SetAsChild(this.values);
		}

		/// <summary>
		/// Value
		/// </summary>
		public Asn1Value[] Values => this.values;

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => acceptedTags;

		private static readonly Asn1Tag[] acceptedTags = new Asn1Tag[]
		{
			new Asn1Tag(UniversalTagNumber.SequenceOf)
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
			if (Pass == CSharpExportPass.Explicit)
			{
				bool First = true;

				if (this.IsFlagsEnum)
				{
					foreach (Asn1Value Value in this.values)
					{
						if (First)
							First = false;
						else
							Output.Append(" | ");

						Output.Append(this.typeReference.Identifier);
						Output.Append('.');
						await Value.ExportCSharp(Output, State, Indent, Pass);
					}
				}
				else
				{
					Output.Append("new ");
					Output.Append(this.CSharpType);
					Output.Append(" { ");

					foreach (Asn1Value Value in this.values)
					{
						if (First)
							First = false;
						else
							Output.Append(", ");

						await Value.ExportCSharp(Output, State, Indent, Pass);
					}

					Output.Append(" }");
				}
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			sb.Append('[');

			foreach (Asn1Value Value in this.values)
			{
				if (First)
					First = false;
				else
					sb.Append(", ");
			}

			sb.Append(']');

			return sb.ToString();
		}

		/// <summary>
		/// If the array represents a <see cref="System.FlagsAttribute"/> enumeration value.
		/// </summary>
		public bool IsFlagsEnum
		{
			get
			{
				if (!this.isFlagsEnum.HasValue)
					this.isFlagsEnum = this.CalcIsFlagsEnum();
				
				return this.isFlagsEnum.Value;
			}
		}

		private bool CalcIsFlagsEnum()
		{
			if (!this.TryGetAncestor(out this.typeReference))
				return false;

			if (!this.Document.namedNodes.TryGetValue(this.typeReference.Identifier,
				out this.type))
			{
				return false;
			}

			if (this.type is Asn1TypeDefinition TypeDefinition)
				this.type = TypeDefinition.Definition;

			return this.type is Asn1Enumeration Enumeration && Enumeration.Flags;
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType
		{
			get
			{
				if (this.IsFlagsEnum)
					return this.typeReference.Identifier;
				
				string CommonTypeName = null;

				foreach (Asn1Value Value in this.values)
				{
					if (CommonTypeName is null)
						CommonTypeName = Value.CSharpType;
					else if (CommonTypeName != Value.CSharpType)
						return "object[]";
				}

				return (CommonTypeName ?? "object") + "[]";
			}
		}

	}
}
