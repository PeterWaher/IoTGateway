using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

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
		private Asn1Document document;
		private Asn1Node parent;
		private ChunkedList<Asn1Node> children = null;

		/// <summary>
		/// Base class for all ASN.1 nodes.
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Node(Asn1Document Document)
		{
			this.document = Document;
		}

		/// <summary>
		/// ASN.1 document hosting the node.
		/// </summary>
		public Asn1Document Document
		{
			get => this.document;
			protected set => this.document = value;
		}

		/// <summary>
		/// Parent node.
		/// </summary>
		public Asn1Node Parent => this.parent;

		/// <summary>
		/// If the node is optional
		/// </summary>
		public virtual bool? Optional => false;

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public virtual Asn1Tag[] AcceptedTags => Array.Empty<Asn1Tag>();

		/// <summary>
		/// Sets the parent reference.
		/// </summary>
		/// <param name="Parent"></param>
		internal void SetParentNode(Asn1Node Parent)
		{
			this.parent?.children?.Remove(this);
			this.parent = Parent;

			if (this.parent.children is null)
				this.parent.children = new ChunkedList<Asn1Node>();

			this.parent.children.Add(this);
		}

		/// <summary>
		/// Defines a node as a child node of the current node.
		/// </summary>
		/// <param name="Child">Child node.</param>
		internal void SetAsChild(Asn1Node Child)
		{
			Child?.SetParentNode(this);
		}

		/// <summary>
		/// Defines a node as a child node of the current node.
		/// </summary>
		/// <param name="Children">Child nodes.</param>
		internal void SetAsChild(IEnumerable<Asn1Node> Children)
		{
			if (!(Children is null))
			{
				foreach (Asn1Node Child in Children)
					Child.SetParentNode(this);
			}
		}

		/// <summary>
		/// Executes a callback method for each ASN.1 node in the syntax tree.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		/// <returns>If the callback method was called on all nodes in the ASN.1 syntax
		/// tree (true), or if the procedure was cancelled (false).</returns>
		public bool ForEach(Predicate<Asn1Node> Callback)
		{
			if (!Callback.Invoke(this))
				return false;

			if (!(this.children is null))
			{
				foreach (Asn1Node Child in this.children)
				{
					if (!Child.ForEach(Callback))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Tries to get an ancestor node of a given type.
		/// </summary>
		/// <typeparam name="T">Type of ancestor to search for.</typeparam>
		/// <param name="Result">Ancestor, if found.</param>
		/// <returns>If an ancestor of the given type was found.</returns>
		public bool TryGetAncestor<T>(out T Result)
		{
			Asn1Node Loop = this.Parent;
			while (!(Loop is null))
			{
				if (Loop is T Typed)
				{
					Result = Typed;
					return true;
				}

				Loop = Loop.Parent;
			}

			Result = default;
			return false;
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public virtual string CSharpType
		{
			get
			{
				throw new NotImplementedException("C# Type name for type " +
					this.GetType().FullName + " not implemented.");
			}
		}

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public virtual Task ExportCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
			{
				throw new NotImplementedException("Support for exporting objects of type " +
					this.GetType().FullName + " not implemented.");
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Exports read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="Index">Item index.</param>
		public virtual Task ExportReadCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf, int Index)
		{
			throw new NotImplementedException("Read operation for objects of type " +
				this.GetType().FullName + " not implemented.");
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
		public virtual Task ExportTryReadCSharp(StringBuilder Output, 
			CSharpExportState State, int Indent, string MemberOf, string DefaultName, 
			string ItemName, int Index)
		{
			throw new NotImplementedException("Optional read operation for objects of type " +
				this.GetType().FullName + " not implemented.");
		}

		/// <summary>
		/// Exports write logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Index">Item index.</param>
		public virtual Task ExportWriteCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, int Index)
		{
			throw new NotImplementedException("Write operation for objects of type " +
				this.GetType().FullName + " not implemented.");
		}

		internal static string ToCSharp(CSharpExportState State, Asn1Node Node,
			string Identifier)
		{
			if (State?.Settings.ContainsMultipleDefinitions(Identifier) ?? false)
				return ToCSharp(Node.Document.GetFullyQualifiedName(State.Settings, Identifier));
			else
				return ToCSharp(Identifier);
		}

		private static string CheckKeyword(string Identifier)
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
				case "volatile":
					return "@" + Identifier;

				default:
					return Identifier;
			}
		}

		internal static string ToCSharp(params string[] Identifier)
		{
			int i, c = Identifier.Length;
			StringBuilder sb = new StringBuilder();

			for (i = 0; i < c; i++)
			{
				if (i > 0)
					sb.Append('.');

				sb.Append(CheckKeyword(Identifier[i]).Replace('-', '_'));
			}

			return sb.ToString();
		}
	}
}
