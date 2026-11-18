using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for ASN.1 restrictions.
	/// </summary>
	public abstract class Asn1Restriction : Asn1Node
	{
		/// <summary>
		/// Abstract base class for ASN.1 restrictions.
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Restriction(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Exports restriction variable definitions to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="VariableName">Name of variable to check.</param>
		public virtual Task ExportRestrictionVariables(StringBuilder Output,
			CSharpExportState State, int Indent, string VariableName)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets checks to perform.
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="VariableName">Variable being checked.</param>
		/// <param name="Type">Data type.</param>
		public virtual Task RenderChecks(StringBuilder Output,
			CSharpExportState State, string VariableName, Asn1Type Type)
		{
			throw new NotImplementedException("Restriction of type " +
				this.GetType().FullName + " not implemented.");
		}
	}
}
