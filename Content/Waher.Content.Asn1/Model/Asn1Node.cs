using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Defines different C# export passes.
	/// </summary>
	public enum CSharpExportPass
	{
		/// <summary>
		/// Preprocessing
		/// </summary>
		Preprocess,

		/// <summary>
		/// Implicit definitions
		/// </summary>
		Implicit,

		/// <summary>
		/// Explicit definitions
		/// </summary>
		Explicit
	}

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
		/// <param name="Pass">Export pass</param>
		public virtual void ExportCSharp(StringBuilder Output, CSharpExportSettings Settings, 
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
			{
				throw new NotImplementedException("Support for exporting objects of type " +
					this.GetType().FullName + " not implemented.");
			}
		}

		internal static string Tabs(int Indent)
		{
			return new string('\t', Indent);
		}

	}
}
