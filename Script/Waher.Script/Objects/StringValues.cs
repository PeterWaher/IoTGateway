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
			return Element is StringValue;
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
			StringValue s1 = (StringValue)x;
			StringValue s2 = (StringValue)y;

			return s1.Value.CompareTo(s2.Value);
		}
	}
}
