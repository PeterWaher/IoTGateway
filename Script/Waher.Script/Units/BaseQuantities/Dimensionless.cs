using System;
using Waher.Script.Operators.Arithmetics;

namespace Waher.Script.Units.BaseQuantities
{
	/// <summary>
	/// Dimensionless units.
	/// </summary>
	public class Dimensionless : IBaseQuantity
	{
		/// <summary>
		/// Dimensionless units.
		/// </summary>
		public Dimensionless()
		{
		}

		/// <summary>
		/// Name of base quantity.
		/// </summary>
		public string Name => "Dimensionless";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public AtomicUnit ReferenceUnit => referenceUnit;

		/// <summary>
		/// Reference unit for category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly AtomicUnit referenceUnit = new AtomicUnit("1");
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
					"1",
					"pcs",
					"dz",
					"dozen",
					"gr",
					"gross",
					"%",
					"‰",
					"‱",
					"%0",
					"%00",
					"°",
					"rad",
					"deg"
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
				case "1":
				case "pcs":
				case "rad":
					return true;

				case "dz":
				case "dozen":
					k = Math.Pow(12, Exponent);
					break;

				case "gr":
				case "gross":
					k = Math.Pow(144, Exponent);
					break;

				case "%":
					k = Math.Pow(1e-2, Exponent);
					break;

				case "‰":
				case "%0":
					k = Math.Pow(1e-3, Exponent);
					break;

				case "‱":
				case "%00":
					k = Math.Pow(1e-4, Exponent);
					break;

				case "°":
				case "deg":
					k = Math.Pow(DegToRad.Scale, Exponent);
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
