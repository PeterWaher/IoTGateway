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
		/// Preprocessing
		/// </summary>
		Variables,

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
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public virtual void ExportCSharp(StringBuilder Output, CSharpExportState State,
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

		internal static string ToCSharp(string Identifier)
		{
			switch (Identifier)
			{
				case "abstract":
				case "as":
				case "base":
				case "bool":
				case "break":
				case "byte":
				case "case":
				case "catch":
				case "char":
				case "checked":
				case "class":
				case "const":
				case "continue":
				case "decimal":
				case "default":
				case "delegate":
				case "do":
				case "double":
				case "else":
				case "enum":
				case "event":
				case "explicit":
				case "extern":
				case "false":
				case "finally":
				case "fixed":
				case "float":
				case "for":
				case "foreach":
				case "goto":
				case "if":
				case "implicit":
				case "in":
				case "int":
				case "interface":
				case "internal":
				case "is":
				case "lock":
				case "long":
				case "namespace":
				case "new":
				case "null":
				case "object":
				case "operator":
				case "out":
				case "override":
				case "params":
				case "private":
				case "protected":
				case "public":
				case "readonly":
				case "ref":
				case "return":
				case "sbyte":
				case "sealed":
				case "short":
				case "sizeof":
				case "stackalloc":
				case "static":
				case "string":
				case "struct":
				case "switch":
				case "this":
				case "throw":
				case "true":
				case "try":
				case "typeof":
				case "uint":
				case "ulong":
				case "unchecked":
				case "unsafe":
				case "ushort":
				case "using":
				case "virtual":
				case "void":
				case "add":
				case "alias":
				case "ascending":
				case "async":
				case "await":
				case "by":
				case "descending":
				case "dynamic":
				case "equals":
				case "from":
				case "get":
				case "global":
				case "group":
				case "into":
				case "join":
				case "let":
				case "nameof":
				case "on":
				case "orderby":
				case "partial":
				case "remove":
				case "select":
				case "set":
				case "value":
				case "var":
				case "when":
				case "where":
				case "yield":
					return "_" + Identifier;

				default:
					int i = Identifier.IndexOf('-');
					if (i >= 0)
						Identifier = Identifier.Replace('-', '_');

					return Identifier;
			}
		}
	}
}
