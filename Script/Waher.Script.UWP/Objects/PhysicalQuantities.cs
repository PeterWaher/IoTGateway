using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Units;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Pseudo-field of physical quantities.
	/// </summary>
	public sealed class PhysicalQuantities : Field, IOrderedSet
	{
        private static readonly int hashCode = typeof(PhysicalQuantities).FullName.GetHashCode();

		/// <summary>
		/// Pseudo-field of physical quantities.
		/// </summary>
		public PhysicalQuantities()
		{
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public override ICommutativeRingWithIdentityElement One
		{
			get { return PhysicalQuantity.OneElement; }
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get { return PhysicalQuantity.ZeroElement; }
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is PhysicalQuantity;
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is PhysicalQuantities;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return hashCode;
		}

		/// <summary>
		/// Compares two double values.
		/// </summary>
		/// <param name="x">Value 1</param>
		/// <param name="y">Value 2</param>
		/// <returns>Result</returns>
		public int Compare(IElement x, IElement y)
		{
			PhysicalQuantity d1 = (PhysicalQuantity)x;
			PhysicalQuantity d2 = (PhysicalQuantity)y;
			double Magnitude2;

			if (Unit.TryConvert(d2.Magnitude, d2.Unit, d1.Unit, out Magnitude2))
				return d1.Magnitude.CompareTo(Magnitude2);
			else
				throw new ScriptException("Incompatible units.");
		}
	}
}
