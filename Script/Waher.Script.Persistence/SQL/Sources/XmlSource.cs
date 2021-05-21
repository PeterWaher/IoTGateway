using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Persistence.Functions;

namespace Waher.Script.Persistence.SQL.Sources
{
	/// <summary>
	/// Data Source defined by XML.
	/// </summary>
	public class XmlSource : IDataSource
	{
		private readonly XmlDocument xmlDocument;
		private readonly XmlNode xmlNode;
		private readonly ScriptNode node;
		private readonly string name;
		private readonly string alias;

		/// <summary>
		/// Data Source defined by XML.
		/// </summary>
		/// <param name="Name">Name of source.</param>
		/// <param name="Alias">Alias for source.</param>
		/// <param name="Xml">XML</param>
		/// <param name="Node">Node defining the vector.</param>
		public XmlSource(string Name, string Alias, XmlDocument Xml, ScriptNode Node)
		{
			this.xmlDocument = Xml;
			this.xmlNode = null;
			this.node = Node;
			this.name = Name;
			this.alias = Alias;
		}

		/// <summary>
		/// Data Source defined by XML.
		/// </summary>
		/// <param name="Name">Name of source.</param>
		/// <param name="Alias">Alias for source.</param>
		/// <param name="Xml">XML</param>
		/// <param name="Node">Node defining the vector.</param>
		public XmlSource(string Name, string Alias, XmlNode Xml, ScriptNode Node)
		{
			this.xmlDocument = null;
			this.xmlNode = Xml;
			this.node = Node;
			this.name = Name;
			this.alias = Alias;
		}

		/// <summary>
		/// Finds objects matching filter conditions in <paramref name="Where"/>.
		/// </summary>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Enumerator.</returns>
		public Task<IResultSetEnumerator> Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			XPath WhereXPath = Where as XPath;
			if (WhereXPath is null && !(Where is null))
				throw new ScriptRuntimeException("WHERE clause must use an XPATH expression when selecting nodes from XML.", Where);

			IElement Obj;

			if (WhereXPath is null)
				Obj = new ObjectValue(this.xmlDocument ?? this.xmlNode);
			else
			{
				ObjectProperties P = new ObjectProperties(this.xmlDocument ?? this.xmlNode, Variables);
				Obj = WhereXPath.Evaluate(P);
			}

			if (!(Obj is IVector Vector))
				Vector = Operators.Vectors.VectorDefinition.Encapsulate(new IElement[] { Obj }, false, Node);

			return VectorSource.Find(Vector, Offset, Top, null, Variables, Order, Node);
		}

		/// <summary>
		/// Updates a set of objects.
		/// </summary>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Objects">Objects to update</param>
		public Task Update(bool Lazy, IEnumerable<object> Objects)
		{
			return Task.CompletedTask;	// Do nothing.
		}

		private Exception InvalidOperation()
		{
			return new ScriptRuntimeException("Operation not permitted on joined sources.", this.node);
		}

		/// <summary>
		/// Finds and Deletes a set of objects.
		/// </summary>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Number of objects deleted, if known.</returns>
		public Task<int?> FindDelete(bool Lazy, int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			throw this.InvalidOperation();
		}

		/// <summary>
		/// Inserts an object.
		/// </summary>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Object">Object to insert.</param>
		public Task Insert(bool Lazy, object Object)
		{
			throw this.InvalidOperation();
		}

		/// <summary>
		/// Name of corresponding collection.
		/// </summary>
		public string CollectionName
		{
			get { throw new ScriptRuntimeException("Collection not defined.", this.node); }
		}

		/// <summary>
		/// Name of corresponding type.
		/// </summary>
		public string TypeName
		{
			get { throw new ScriptRuntimeException("Type not defined.", this.node); }
		}

		/// <summary>
		/// Collection name or alias.
		/// </summary>
		public string Name
		{
			get => string.IsNullOrEmpty(this.alias) ? this.name : this.alias;
		}

		/// <summary>
		/// Checks if the name refers to the source.
		/// </summary>
		/// <param name="Name">Name to check.</param>
		/// <returns>If the name refers to the source.</returns>
		public bool IsSource(string Name)
		{
			return 
				string.Compare(this.name, Name, true) == 0 ||
				string.Compare(this.alias, Name, true) == 0;
		}

		/// <summary>
		/// Checks if the label is a label in the source.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <returns>If the label is a label in the source.</returns>
		public Task<bool> IsLabel(string Label)
		{
			return Task.FromResult<bool>(false);
		}

		/// <summary>
		/// Creates an index in the source.
		/// </summary>
		/// <param name="Name">Name of index.</param>
		/// <param name="Fields">Field names. Prefix with hyphen (-) to define descending order.</param>
		public Task CreateIndex(string Name, string[] Fields)
		{
			throw this.InvalidOperation();
		}

		/// <summary>
		/// Drops an index from the source.
		/// </summary>
		/// <param name="Name">Name of index.</param>
		/// <returns>If an index was found and dropped.</returns>
		public Task<bool> DropIndex(string Name)
		{
			throw InvalidOperation();
		}

		/// <summary>
		/// Drops the collection from the source.
		/// </summary>
		public Task DropCollection()
		{
			throw InvalidOperation();
		}

	}
}
