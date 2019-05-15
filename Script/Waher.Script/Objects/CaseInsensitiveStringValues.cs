using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Semi-group of case-insensitive string values.
	/// </summary>
	public sealed class CaseInsensitiveStringValues : SemiGroup, IOrderedSet
	{
		private static readonly int hashCode = typeof(CaseInsensitiveStringValues).FullName.GetHashCode();

        /// <summary>
        /// Semi-group of case-insensitive string values.
        /// </summary>
        public CaseInsensitiveStringValues()
		{
		}

		/// <summary>
		/// Instance of the set of case-insensitive string values.
		/// </summary>
		public static readonly CaseInsensitiveStringValues Instance = new CaseInsensitiveStringValues();

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is StringValue S && S.CaseInsensitive;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is CaseInsensitiveStringValues;
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

			return string.Compare(s1.Value, s2.Value, true);
		}
	}
}
