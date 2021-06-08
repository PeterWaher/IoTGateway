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
		public string Name
		{
			get
			{
				return "Mass";
			}
		}

		/// <summary>
		/// Reference unit of base quantity.
		/// </summary>
		public AtomicUnit ReferenceUnit
		{
			get { return referenceUnit; }
		}

		private static readonly AtomicUnit referenceUnit = new AtomicUnit("g");

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
			// Reference: g

			switch (FromUnit.ToString(false))
			{
				case "g":
					To = From;
					break;

				case "t":
					To = From * 1e6;
					break;

				case "u":
					To = From * 1.66e-24;
					break;

				case "lb":
					To = From * 450;
					break;

				default:
					To = 0;
					return false;
			}

			switch (ToUnit.ToString(false))
			{
				case "g":
					break;

				case "t":
					To /= 1e6;
					break;

				case "u":
					To /= 1.66e-24;
					break;

				case "lb":
					To /= 450;
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
				case "g":
					return true;

				case "t":
					Magnitude *= Math.Pow(1e6, Exponent);
					return true;

				case "u":
					Magnitude *= Math.Pow(1.66e-24, Exponent);
					return true;

				case "lb":
					Magnitude *= Math.Pow(450, Exponent);
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
