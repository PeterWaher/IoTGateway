using System;

namespace Waher.Script.Units.BaseQuantities
{
	/// <summary>
	/// The one-dimensional extent of an object.
	/// </summary>
	public class Length : IBaseQuantity
	{
		/// <summary>
		/// The one-dimensional extent of an object.
		/// </summary>
		public Length()
		{
		}

		/// <summary>
		/// Name of base quantity.
		/// </summary>
		public string Name => "Length";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public AtomicUnit ReferenceUnit => referenceUnit;

		/// <summary>
		/// Reference unit for category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly AtomicUnit referenceUnit = new AtomicUnit("m");
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
					"m",
					"Å",
					"in",
					"inch",
					"ft",
					"foot",
					"yd",
					"yard",
					"SM",
					"NM",
					"px",
					"pt",
					"pc",
					"em",
					"ex",
					"ch",
					"rem",
					"vw",
					"vh",
					"vmin",
					"vmax"
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
				case "m":
					return true;

				case "Å":
					k = Math.Pow(1e-10, Exponent);
					break;

				case "in":
				case "inch":
					k = Math.Pow(0.0254, Exponent);
					break;

				case "ft":
				case "foot":
					k = Math.Pow(0.3048, Exponent);
					break;

				case "yd":
				case "yard":
					k = Math.Pow(0.9144, Exponent);
					break;

				case "SM":
					k = Math.Pow(1609.344, Exponent);
					break;

				case "NM":
					k = Math.Pow(1852, Exponent);
					break;

				case "px":
					k = Math.Pow(0.0254 / 96, Exponent);
					break;

				case "pt":
					k = Math.Pow(0.0254 / 72, Exponent);
					break;

				case "pc":
					k = Math.Pow(0.0254 / 6, Exponent);
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
