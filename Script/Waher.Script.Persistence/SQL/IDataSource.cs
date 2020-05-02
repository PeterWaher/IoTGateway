using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Interface for data sources that can be used in SQL statements.
	/// </summary>
	public interface IDataSource
	{
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
		Task<IResultSetEnumerator> Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node);

		/// <summary>
		/// Updates a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to update</param>
		Task Update(IEnumerable<object> Objects);

		/// <summary>
		/// Deletes a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to delete</param>
		Task Delete(IEnumerable<object> Objects);

		/// <summary>
		/// Inserts an object.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		Task Insert(object Object);

		/// <summary>
		/// Name of corresponding collection.
		/// </summary>
		string CollectionName
		{
			get;
		}

		/// <summary>
		/// Name of corresponding type.
		/// </summary>
		string TypeName
		{
			get;
		}
	}
}
