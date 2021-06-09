using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Semi-group of string values.
	/// </summary>
	public sealed class StringValues : SemiGroup, IOrderedSet
	{
		private static readonly int hashCode = typeof(StringValues).FullName.GetHashCode();

		/// <summary>
		/// Semi-group of string values.
		/// </summary>
		public StringValues()
		{
		}

		/// <summary>
		/// Instance of the set of string values.
		/// </summary>
		public static readonly StringValues Instance = new StringValues();

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is StringValue S && !S.CaseInsensitive;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is StringValues;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			return hashCode;
		}

		/// <summary>
		/// Compares two string values.
		/// </summary>
		/// <param name="x">Value 1</param>
		/// <param name="y">Value 2</param>
		/// <returns>Result</returns>
		public int Compare(IElement x, IElement y)
		{
			if (!(x.AssociatedObjectValue is string s1))
				s1 = Expression.ToString(x.AssociatedObjectValue);

			if (!(y.AssociatedObjectValue is string s2))
				s2 = Expression.ToString(y.AssociatedObjectValue);

			return string.Compare(s1, s2, false);
		}
	}
}
