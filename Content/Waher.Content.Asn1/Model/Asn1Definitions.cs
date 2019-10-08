using System;
using System.Collections.Generic;
using System.Text;

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
		private readonly Asn1Node body;

		/// <summary>
		/// Represents a collection of ASN.1 definitions.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Oid">Optional Object ID</param>
		/// <param name="Tags">How tags are handled.</param>
		/// <param name="Abstract">If abstract syntax is used.</param>
		/// <param name="Body">Definition body.</param>
		public Asn1Definitions(string Identifier, Asn1Oid Oid, Asn1Tags? Tags, bool Abstract, Asn1Node Body)
		{
			this.identifier = Identifier;
			this.oid = Oid;
			this.tags = Tags;
			this._abstract = Abstract;
			this.body = Body;
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
		public Asn1Node Body => this.body;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportSettings Settings, 
			int Indent, CSharpExportPass Pass)
		{
			this.body?.ExportCSharp(Output, Settings, Indent, Pass);
		}
	}
}
