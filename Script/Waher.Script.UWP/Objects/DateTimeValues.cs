using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// The set of Date & Time values.
	/// </summary>
	public sealed class DateTimeValues : Set, IOrderedSet
	{
		private static readonly int hashCode = typeof(DateTimeValues).FullName.GetHashCode();

		/// <summary>
		/// The set of Date & Time values.
		/// </summary>
		public DateTimeValues()
		{
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is DateTimeValue;
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is DateTimeValues;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return hashCode;
		}

		/// <summary>
		/// Compares two DateTime values.
		/// </summary>
		/// <param name="x">Value 1</param>
		/// <param name="y">Value 2</param>
		/// <returns>Result</returns>
		public int Compare(IElement x, IElement y)
		{
			DateTimeValue d1 = (DateTimeValue)x;
			DateTimeValue d2 = (DateTimeValue)y;

			return d1.Value.CompareTo(d2.Value);
		}
	}
}
