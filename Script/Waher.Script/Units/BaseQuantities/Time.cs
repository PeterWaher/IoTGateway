using System;

namespace Waher.Script.Units.BaseQuantities
{
	/// <summary>
	/// Time is a measure in which events can be ordered from the past through the present into the future, and also the measure 
	/// of durations of events and the intervals between them.
	/// </summary>
	public class Time : IBaseQuantity
	{
		/// <summary>
		/// Time is a measure in which events can be ordered from the past through the present into the future, and also the measure 
		/// of durations of events and the intervals between them.
		/// </summary>
		public Time()
		{
		}

		/// <summary>
		/// Name of base quantity.
		/// </summary>
		public string Name => "Time";

		/// <summary>
		/// Reference unit of base quantity.
		/// </summary>
		public AtomicUnit ReferenceUnit => referenceUnit;

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly AtomicUnit referenceUnit = new AtomicUnit("s");
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
					"s",
					"min",
					"h",
					"d",
					"w"
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
				case "s":
					return true;

				case "min":
					k = Math.Pow(60, Exponent);
					break;

				case "h":
					k = Math.Pow(3600, Exponent);
					break;

				case "d":
					k = Math.Pow(86400, Exponent);
					break;

				case "w":
					k = Math.Pow(604800, Exponent);
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
