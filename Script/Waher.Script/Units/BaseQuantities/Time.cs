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
		public string Name
		{
			get
			{
				return "Time";
			}
		}

		/// <summary>
		/// Reference unit of base quantity.
		/// </summary>
		public AtomicUnit ReferenceUnit
		{
			get { return referenceUnit; }
		}

		private static readonly AtomicUnit referenceUnit = new AtomicUnit("s");

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
		/// Tries to convert a magnitude in one unit to a magnitude in another.
		/// 
		/// NOTE: No consideration must be taken to the prefixes of the different units.
		/// 
		/// NOTE 2: When this method is called, units have been recognized and considered being part of the same base quantity.
		/// </summary>
		/// <param name="From">Original magnitude.</param>
		/// <param name="FromUnit">Original unit.</param>
		/// <param name="ToUnit">Desired unit.</param>
		/// <param name="To">Converted magnitude.</param>
		/// <returns>If conversion was successful.</returns>
		public bool TryConvert(double From, Unit FromUnit, Unit ToUnit, out double To)
		{
			// Reference: s

			switch (FromUnit.ToString(false))
			{
				case "s":
					To = From;
					break;

				case "min":
					To = From * 60;
					break;

				case "h":
					To = From * 3600;
					break;

				case "d":
					To = From * 86400;
					break;

				case "w":
					To = From * 604800;
					break;

				default:
					To = 0;
					return false;
			}

			switch (ToUnit.ToString(false))
			{
				case "s":
					break;

				case "min":
					To /= 60;
					break;

				case "h":
					To /= 3600;
					break;

				case "d":
					To /= 86400;
					break;

				case "w":
					To /= 604800;
					break;

				default:
					To = 0;
					return false;
			}

			return true;
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
			switch (BaseUnit)
			{
				case "s":
					return true;

				case "min":
					Magnitude *= Math.Pow(60, Exponent);
					return true;

				case "h":
					Magnitude *= Math.Pow(3600, Exponent);
					return true;

				case "d":
					Magnitude *= Math.Pow(86400, Exponent);
					return true;

				case "w":
					Magnitude *= Math.Pow(604800, Exponent);
					return true;

				default:
					return false;
			}
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
			return this.ToReferenceUnit(ref Magnitude, BaseUnit, -Exponent);
		}

	}
}
