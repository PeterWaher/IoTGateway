using System;

namespace Waher.Script.Units.BaseQuantities
{
	/// <summary>
	/// An electric current is a flow of electric charge.
	/// </summary>
	public class Current : IBaseQuantity
	{
		/// <summary>
		/// An electric current is a flow of electric charge.
		/// </summary>
		public Current()
		{
		}

		/// <summary>
		/// Name of base quantity.
		/// </summary>
		public string Name
		{
			get
			{
				return "Current";
			}
		}

		/// <summary>
		/// Reference unit of base quantity.
		/// </summary>
		public AtomicUnit ReferenceUnit
		{
			get { return referenceUnit; }
		}

		private static readonly AtomicUnit referenceUnit = new AtomicUnit("A");

		/// <summary>
		/// Base Units supported.
		/// </summary>
		public string[] BaseUnits
		{
			get
			{
				return new string[]
				{
					"A"
				};
			}
		}

		/// <summary>
		/// Tries to convert a magnitude from a specified base unit, to the reference unit.
		/// </summary>
		/// <param name="Magnitude">Magnitude</param>
		/// <param name="BaseUnit">Base unit of <paramref name="Magnitude"/>.</param>
		/// <param name="Exponent">Exponent.</param>
		/// <returns>If the conversion was successful. If not, the magnitude value is unchanged.</returns>
		public bool ToReferenceUnit(ref double Magnitude, string BaseUnit, int Exponent)
		{
			return (BaseUnit == "A");
		}

		/// <summary>
		/// Tries to convert a magnitude to a specified base unit, from the reference unit.
		/// </summary>
		/// <param name="Magnitude">Magnitude</param>
		/// <param name="BaseUnit">Desired base unit of <paramref name="Magnitude"/>.</param>
		/// <param name="Exponent">Exponent.</param>
		/// <returns>If the conversion was successful. If not, the magnitude value is unchanged.</returns>
		public bool FromReferenceUnit(ref double Magnitude, string BaseUnit, int Exponent)
		{
			return (BaseUnit == "A");
		}

	}
}
