using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Exceptions;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Enumerator that reorders a sequence of items.
	/// </summary>
	public class CustomOrderEnumerator : IEnumerator
	{
		private readonly ScriptNode[] order;
		private readonly Variables variables;
		private readonly IEnumerator e;

		/// <summary>
		/// Enumerator that reorders a sequence of items.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Custom order.</param>
		public CustomOrderEnumerator(IEnumerator ItemEnumerator, Variables Variables, ScriptNode[] Order)
		{
			this.variables = Variables;
			this.order = Order;

			List<object> Items = new List<object>();

			while (ItemEnumerator.MoveNext())
				Items.Add(ItemEnumerator.Current);

			Dictionary<Type, ObjectProperties> PropertiesX = new Dictionary<Type, ObjectProperties>();
			Dictionary<Type, ObjectProperties> PropertiesY = new Dictionary<Type, ObjectProperties>();

			Items.Sort((x, y) =>
			{
				if (x == null)
				{
					if (y == null)
						return 0;
					else
						return -1;
				}
				else if (y == null)
					return 1;

				Type Tx = x.GetType();
				Type Ty = y.GetType();

				if (PropertiesX.TryGetValue(Tx, out ObjectProperties Vx))
					Vx.Object = x;
				else
				{
					Vx = new ObjectProperties(x, Variables);
					PropertiesX[Tx] = Vx;
				}

				if (PropertiesY.TryGetValue(Ty, out ObjectProperties Vy))
					Vy.Object = y;
				else
				{
					Vy = new ObjectProperties(y, Variables);
					PropertiesY[Ty] = Vy;
				}

				int i, j, c = this.order.Length;
				IElement Ex, Ey;

				for (i = 0; i < c; i++)
				{
					Ex = this.order[i].Evaluate(Vx);
					Ey = this.order[i].Evaluate(Vy);

					if (!(Ex.AssociatedSet is IOrderedSet S))
						throw new ScriptRuntimeException("Result not member of an ordered set.", this.order[i]);

					j = S.Compare(Ex, Ey);
					if (j != 0)
						return j;
				}

				return 0;
			});

			this.e = Items.GetEnumerator();
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
			return this.e.MoveNext();
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.e.Reset();
		}
	}
}
