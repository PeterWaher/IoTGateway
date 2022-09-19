using System;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Set of object values.
	/// </summary>
	public sealed class ObjectValues : Set, IOrderedSet
	{
		private static readonly int hashCode = typeof(ObjectValues).FullName.GetHashCode();

		/// <summary>
		/// Set of object values.
		/// </summary>
		public ObjectValues()
		{
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is ObjectValue;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is ObjectValues;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return hashCode;
		}

		/// <summary>
		/// Compares two object values.
		/// </summary>
		/// <param name="x">Value 1</param>
		/// <param name="y">Value 2</param>
		/// <returns>Result</returns>
		public int Compare(IElement x, IElement y)
		{
			object o1 = x.AssociatedObjectValue;
			object o2 = y.AssociatedObjectValue;
			IComparable c1 = o1 as IComparable;
			IComparable c2 = o2 as IComparable;

			if (c1 is null || c2 is null)
			{
				if (!(o1 is null || o2 is null))
				{
					if (o1.Equals(o2))
						return 0;

					IElement E = BinaryOperator.EvaluateNamedOperator("op_LessThan", x, y, ScriptNode.EmptyNode);
					if (E is BooleanValue B)
						return B.Value ? -1 : 1;
				}

				if (c1 is null && c2 is null)
					return 0;
				else if (c1 is null)
					return -1;
				else
					return 1;
			}

			return c1.CompareTo(c2);
		}
	}
}
