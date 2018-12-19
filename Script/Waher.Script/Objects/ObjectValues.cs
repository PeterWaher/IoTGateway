using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;

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

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is ObjectValues;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
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
			ObjectValue o1 = (ObjectValue)x;
			ObjectValue o2 = (ObjectValue)y;

			IComparable c1 = o1.Value as IComparable;
			IComparable c2 = o2.Value as IComparable;

			if (c1 is null || c2 is null)
			{
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
