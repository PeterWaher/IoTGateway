using System;

namespace Waher.Script.Units.BaseQuantities
{
	/// <summary>
	/// It is generally a measure of an object's resistance to changing its state of motion when a force is applied.
	/// </summary>
	public class Mass : IBaseQuantity
	{
		/// <summary>
		/// It is generally a measure of an object's resistance to changing its state of motion when a force is applied.
		/// </summary>
		public Mass()
		{
		}

		/// <summary>
		/// Name of base quantity.
		/// </summary>
		public string Name => "Mass";

		/// <summary>
		/// Reference unit of base quantity.
		/// </summary>
		public AtomicUnit ReferenceUnit => referenceUnit;

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly AtomicUnit referenceUnit = new AtomicUnit("g");
		private static readonly Unit reference = new Unit(referenceUnit);

		/// <summary>
		/// Base Units supported.
		/// </summary>
		public string[] BaseUnits
		{
			get
			{
				return new string[]
				{
					"g",
					"t",
					"u",
					"lb"
				};
			}
		}

		/// <summary>
		/// Tries to convert a magnitude from a specified base unit, to the reference unit.
		/// </summary>
		/// <param name="Magnitude">Magnitude</param>
		/// <param name="NrDecimals">Number of decimals to use when presenting magnitude.</param>
		/// <param name="BaseUnit">Base unit of <paramref name="Magnitude"/>.</param>
		/// <param name="Exponent">Exponent.</param>
		/// <returns>If the conversion was successful. If not, the magnitude value is unchanged.</returns>
		public bool ToReferenceUnit(ref double Magnitude, ref double NrDecimals, string BaseUnit, int Exponent)
		{
			double k;

			switch (BaseUnit)
			{
				case "g":
					return true;

				case "t":
					k = Math.Pow(1e6, Exponent);
					break;

				case "u":
					k = Math.Pow(1.66e-24, Exponent);
					break;

				case "lb":
					k = Math.Pow(450, Exponent);
					break;

				default:
					return false;
			}

			Magnitude *= k;
			NrDecimals -= Math.Log10(k);

			return true;
		}

		/// <summary>
		/// Tries to convert a magnitude to a specified base unit, from the reference unit.
		/// </summary>
		/// <param name="Magnitude">Magnitude</param>
		/// <param name="NrDecimals">Number of decimals to use when presenting magnitude.</param>
		/// <param name="BaseUnit">Desired base unit of <paramref name="Magnitude"/>.</param>
		/// <param name="Exponent">Exponent.</param>
		/// <returns>If the conversion was successful. If not, the magnitude value is unchanged.</returns>
		public bool FromReferenceUnit(ref double Magnitude, ref double NrDecimals, string BaseUnit, int Exponent)
		{
			return this.ToReferenceUnit(ref Magnitude, ref NrDecimals, BaseUnit, -Exponent);
		}

	}
}
