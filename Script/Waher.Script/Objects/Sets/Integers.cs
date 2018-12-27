using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Objects.Sets
{
	/// <summary>
	/// Set containing all integers.
	/// </summary>
	public sealed class Integers : Set
	{
		private static readonly int hashCode = typeof(Integers).GetHashCode();

		/// <summary>
		/// Set containing all integers.
		/// </summary>
		public Integers()
		{
		}

		/// <summary>
		/// Instance of the set of real numbers.
		/// </summary>
		public static readonly Integers Instance = new Integers();

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return (Element is DoubleNumber D) && (Math.Truncate(D.Value) == D.Value);
		}

		/// <summary>
		/// Compares the element to another.
		/// </summary>
		/// <param name="obj">Other element to compare against.</param>
		/// <returns>If elements are equal.</returns>
		public override bool Equals(object obj)
		{
			return obj is Integers;
		}

		/// <summary>
		/// Calculates a hash code of the element.
		/// </summary>
		/// <returns>Hash code.</returns>
		public override int GetHashCode()
		{
			return hashCode;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return "ℤ";
		}
	}
}
