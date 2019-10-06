using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Base class for all ASN.1 nodes.
	/// </summary>
	public abstract class Asn1Node
	{
		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		public virtual void ExportCSharp(StringBuilder Output, CSharpExportSettings Settings, int Indent)
		{
			throw new NotImplementedException("Support for exporting objects of type " + 
				this.GetType().FullName + " not implemented.");
		}

		/// <summary>
		/// Exports implicit definitions to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		public virtual void ExportImplicitCSharp(StringBuilder Output, CSharpExportSettings Settings, int Indent)
		{
			// No implicit definitions to export, by default.
		}

		internal static string Tabs(int Indent)
		{
			return new string('\t', Indent);
		}

	}
}
