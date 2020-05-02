using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Exceptions;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Enumerator that reorders a sequence of items.
	/// </summary>
	public class CustomOrderEnumerator : IResultSetEnumerator
	{
		private readonly KeyValuePair<ScriptNode, bool>[] order;
		private readonly IResultSetEnumerator items;
		private readonly Variables variables;
		private IEnumerator e = null;

		/// <summary>
		/// Enumerator that reorders a sequence of items.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Custom order.</param>
		public CustomOrderEnumerator(IResultSetEnumerator ItemEnumerator, Variables Variables, KeyValuePair<ScriptNode, bool>[] Order)
		{
			this.order = Order;
			this.items = ItemEnumerator;
			this.variables = Variables;
		}

		/// <summary>
		/// <see cref="IEnumerator.Current"/>
		/// </summary>
		public object Current => this.e.Current;

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public bool MoveNext()
		{
			return this.MoveNextAsync().Result;
		}

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public async Task<bool> MoveNextAsync()
		{
			if (this.e is null)
			{
				List<object> Items = new List<object>();

				while (await this.items.MoveNextAsync())
					Items.Add(this.items.Current);

				Dictionary<Type, ObjectProperties> PropertiesX = new Dictionary<Type, ObjectProperties>();
				Dictionary<Type, ObjectProperties> PropertiesY = new Dictionary<Type, ObjectProperties>();

				Items.Sort((x, y) =>
				{
					if (x is null)
					{
						if (y is null)
							return 0;
						else
							return -1;
					}
					else if (y is null)
						return 1;

					Type Tx = x.GetType();
					Type Ty = y.GetType();

					if (PropertiesX.TryGetValue(Tx, out ObjectProperties Vx))
						Vx.Object = x;
					else
					{
						Vx = new ObjectProperties(x, this.variables);
						PropertiesX[Tx] = Vx;
					}

					if (PropertiesY.TryGetValue(Ty, out ObjectProperties Vy))
						Vy.Object = y;
					else
					{
						Vy = new ObjectProperties(y, this.variables);
						PropertiesY[Ty] = Vy;
					}

					int i, j, c = this.order.Length;
					IElement Ex, Ey;
					ScriptNode Node;

					for (i = 0; i < c; i++)
					{
						Node = this.order[i].Key;
						Ex = Node.Evaluate(Vx);
						Ey = Node.Evaluate(Vy);

						if (!(Ex.AssociatedSet is IOrderedSet S))
							throw new ScriptRuntimeException("Result not member of an ordered set.", Node);

						j = S.Compare(Ex, Ey);
						if (j != 0)
						{
							if (this.order[i].Value)
								return j;
							else
								return -j;
						}
					}

					return 0;
				});

				this.e = Items.GetEnumerator();
			}

			return this.e.MoveNext();
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.items.Reset();
			this.e = null;
		}
	}
}
