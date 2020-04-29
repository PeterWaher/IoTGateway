using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Order
{
	/// <summary>
	/// Orders elements based on values of a given property.
	/// </summary>
	public class PropertyOrder : IComparer<IElement>
	{
		private readonly ScriptNode node;
		private readonly string name;
		private readonly int sign;
		private Type lastType = null;
		private FieldInfo lastFieldInfo = null;
		private PropertyInfo lastPropertyInfo = null;

		/// <summary>
		/// Orders elements based on values of a given property.
		/// </summary>
		/// <param name="Node">Node performing evaluation.</param>
		/// <param name="Name">Name of property to order on.</param>
		/// <param name="Sign">If ascending (1) or descending (-1) order is desired.</param>
		public PropertyOrder(ScriptNode Node, string Name, int Sign)
		{
			this.node = Node;
			this.name = Name;
			this.sign = Sign;
		}

		/// <summary>
		/// Compares two elements.
		/// </summary>
		/// <param name="x">First element</param>
		/// <param name="y">Second element</param>
		/// <returns>Ordinal difference between elements.</returns>
		public int Compare(IElement x, IElement y)
		{
			IElement v1 = this.GetValue(x);
			IElement v2 = this.GetValue(y);

			return this.sign * ElementOrder.Compare(v1, v2, this.node);
		}

		private IElement GetValue(IElement Obj)
		{
			object Value = Obj.AssociatedObjectValue;
			Type T = Value.GetType();
			if (T != this.lastType)
			{
				this.lastType = T;

				PropertyInfo PI = T.GetRuntimeProperty(this.name);

				if (!(PI is null))
				{
					this.lastPropertyInfo = PI;
					this.lastFieldInfo = null;
				}
				else
				{
					FieldInfo FI = T.GetRuntimeField(this.name);

					if (!(PI is null))
					{
						this.lastPropertyInfo = null;
						this.lastFieldInfo = FI;
					}
					else
					{
						this.lastPropertyInfo = null;
						this.lastFieldInfo = null;
					}
				}
			}

			if (!(this.lastPropertyInfo is null))
				Value = this.lastPropertyInfo.GetValue(Value);
			else if (!(this.lastFieldInfo is null))
				Value = this.lastFieldInfo.GetValue(Value);
			else
				Value = null;

			return Expression.Encapsulate(Value);
		}
	}
}
