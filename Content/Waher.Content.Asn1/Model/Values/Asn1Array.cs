using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Array of values
	/// </summary>
	public class Asn1Array : Asn1Value
	{
		private readonly Asn1Value[] values;

		/// <summary>
		/// Array of values
		/// </summary>
		/// <param name="Values">Values</param>
		public Asn1Array(Asn1Value[] Values)
		{
			this.values = Values;
		}

		/// <summary>
		/// Value
		/// </summary>
		public Asn1Value[] Values => this.values;

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


				Output.Append("new ");
				Output.Append(this.CSharpType);
				Output.Append(" { ");

				foreach (Asn1Value Value in this.values)
				{
					if (First)
						First = false;
					else
						Output.Append(", ");

					Value.ExportCSharp(Output, State, Indent, Pass);
				}

				Output.Append(" }");
			}
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType
		{
			get
			{
				string CommonType = null;

				foreach (Asn1Value Value in this.values)
				{
					if (CommonType is null)
						CommonType = Value.CSharpType;
					else if (CommonType != Value.CSharpType)
						return "object[]";
				}

				return (CommonType ?? "object") + "[]";
			}
		}

	}
}
