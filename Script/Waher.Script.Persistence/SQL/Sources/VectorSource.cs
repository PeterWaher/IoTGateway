using System;
using System.Collections;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Order;

namespace Waher.Script.Persistence.SQL.Sources
{
	/// <summary>
	/// Data Source defined by a vector.
	/// </summary>
	public class VectorSource : IDataSource
	{
		private readonly IVector vector;
		private readonly ScriptNode node;

		/// <summary>
		/// Data Source defined by a vector.
		/// </summary>
		/// <param name="Vector">Vector</param>
		/// <param name="Node">Node defining the vector.</param>
		public VectorSource(IVector Vector, ScriptNode Node)
		{
			this.vector = Vector;
			this.node = Node;
		}

		/// <summary>
		/// Vector
		/// </summary>
		public IVector Vector => this.vector;

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
		public IEnumerator Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			IEnumerator e = this.vector.VectorElements.GetEnumerator();
			int i, c;

			if (Where != null)
				e = new ConditionalEnumerator(e, Variables, Where);

			if ((c = Order?.Length ?? 0) > 0)
			{
				List<IElement> Items = new List<IElement>();

				while (e.MoveNext())
				{
					if (e.Current is Element E)
						Items.Add(E);
					else
						Items.Add(Expression.Encapsulate(e.Current));
				}

				Items.Add(e.Current as IElement);

				IComparer<IElement> Order2;

				if (c == 1)
					Order2 = ToPropertyOrder(Node, Order[0]);
				else
				{
					IComparer<IElement>[] Orders = new IComparer<IElement>[c];

					for (i = 0; i < c; i++)
						Orders[i] = ToPropertyOrder(Node, Order[i]);

					Order2 = new CompoundOrder(Orders);
				}

				Items.Sort(Order2);

				e = Items.GetEnumerator();
			}

			if (Offset > 0)
				e = new OffsetEnumerator(e, Offset);

			if (Top != int.MaxValue)
				e = new MaxCountEnumerator(e, Top);

			return e;
		}

		private static PropertyOrder ToPropertyOrder(ScriptNode Node, KeyValuePair<VariableReference, bool> Order)
		{
			return new PropertyOrder(Node, Order.Key.VariableName, Order.Value ? 1 : -1);
		}

		/// <summary>
		/// Updates a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to update</param>
		public void Update(IEnumerable<object> Objects)
		{
			// Do nothing.
		}

		/// <summary>
		/// Deletes a set of objects.
		/// </summary>
		/// <param name="Objects">Objects to delete</param>
		public void Delete(IEnumerable<object> Objects)
		{
			throw new ScriptRuntimeException("Unable to delete object", this.node);
		}

		/// <summary>
		/// Inserts an object.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public void Insert(object Object)
		{
			throw new ScriptRuntimeException("Unable to insert object", this.node);
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

	}
}
