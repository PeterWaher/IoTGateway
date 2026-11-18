using System.Collections.Generic;
using System.Text;
using Waher.Content.Asn1.Model;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// C# export settings
	/// </summary>
	public class CSharpExportSettings
	{
		private readonly Dictionary<string, KeyValuePair<Asn1Document, string>> imported =
			new Dictionary<string, KeyValuePair<Asn1Document, string>>();
		private readonly Dictionary<string, Dictionary<string, bool>> fullyQualifiedNames = new Dictionary<string, Dictionary<string, bool>>();
		private readonly string baseNamespace;

		/// <summary>
		/// What encoders and decoders to include in the generation of C# code.
		/// </summary>
		/// <param name="BaseNamespace">Base namespace of generated code.</param>
		public CSharpExportSettings(string BaseNamespace)
		{
			this.baseNamespace = BaseNamespace;
		}

		/// <summary>
		/// Base Namespace
		/// </summary>
		public string BaseNamespace => this.baseNamespace;

		/// <summary>
		/// Namespace for a given identifier.
		/// </summary>
		/// <param name="Identifier">Identifier.</param>
		/// <returns>Namespace</returns>
		public string Namespace(string Identifier)
		{
			return this.baseNamespace + "." + Asn1Node.ToCSharp(Identifier);
		}

		/// <summary>
		/// If C# code is available for a given identifier.
		/// </summary>
		/// <param name="Identifier">Identifier.</param>
		/// <returns>If C# code exists.</returns>
		public bool ContainsCode(string Identifier)
		{
			return this.imported.ContainsKey(Identifier);
		}

		/// <summary>
		/// Adds C# code for a given identifier.
		/// </summary>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Code">C# Code</param>
		/// <param name="Document">Document</param>
		public void AddCode(string Identifier, string Code, Asn1Document Document)
		{
			this.imported[Identifier] = new KeyValuePair<Asn1Document, string>(Document, Code);
		}

		/// <summary>
		/// Gets C# code for a given identifier.
		/// </summary>
		/// <param name="Identifier">Identifier.</param>
		/// <returns>C# code</returns>
		public string GetCode(string Identifier)
		{
			return this.imported[Identifier].Value;
		}

		/// <summary>
		/// Gets C# code for a given identifier.
		/// </summary>
		/// <param name="Identifier">Identifier.</param>
		/// <returns>ASN.1 document.</returns>
		public Asn1Document GetDocument(string Identifier)
		{
			return this.imported[Identifier].Key;
		}

		/// <summary>
		/// Declares a type.
		/// </summary>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Node">Node defining the type and identifier.</param>
		/// <param name="Identifier">Name of type.</param>
		/// <param name="MustBeQualified">If the identifier requires qualified access.</param>
		public void DeclareType(CSharpExportSettings Settings, Asn1Node Node, 
			string Identifier, bool MustBeQualified)
		{
			if (!this.fullyQualifiedNames.TryGetValue(Identifier, 
				out Dictionary<string, bool> Namespaces))
			{
				Namespaces = new Dictionary<string, bool>();
				this.fullyQualifiedNames[Identifier] = Namespaces;
			}

			StringBuilder sb = new StringBuilder();
			string[] Fqn = Node.Document.GetFullyQualifiedName(Settings, Identifier);
			int i, c = Fqn.Length;

			for (i = 0; i < c; i++)
			{
				if (i > 0)
					sb.Append('.');

				sb.Append(Fqn[i]);
			}

			Namespaces[sb.ToString()] = MustBeQualified;
		}

		/// <summary>
		/// Checks if a type is declared multiple times.
		/// </summary>
		/// <param name="Identifier">Name of type.</param>
		/// <returns>If the type has been declared multiple times.</returns>
		public bool ContainsMultipleDefinitions(string Identifier)
		{
			if (!this.fullyQualifiedNames.TryGetValue(Identifier,
				out Dictionary<string, bool> Namespaces))
			{
				return false;
			}

			if (Namespaces.Count > 1)
				return true;

			foreach (KeyValuePair<string, bool> P in Namespaces)
				return P.Value;

			return false;
		}

		/// <summary>
		/// Modules exported.
		/// </summary>
		public string[] Modules
		{
			get
			{
				string[] Result = new string[this.imported.Count];
				this.imported.Keys.CopyTo(Result, 0);
				return Result;
			}
		}
	}
}
