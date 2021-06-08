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
		public string Name
		{
			get
			{
				return "Length";
			}
		}

		/// <summary>
		/// Reference unit of base quantity.
		/// </summary>
		public AtomicUnit ReferenceUnit
		{
			get { return referenceUnit; }
		}

		private static readonly AtomicUnit referenceUnit = new AtomicUnit("m");

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
		/// <param name="BaseUnit">Base unit of <paramref name="Magnitude"/>.</param>
		/// <param name="Exponent">Exponent.</param>
		/// <returns>If the conversion was successful. If not, the magnitude value is unchanged.</returns>
		public bool ToReferenceUnit(ref double Magnitude, string BaseUnit, int Exponent)
		{
			switch (BaseUnit)
			{
				case "m":
					return true;

				case "Å":
					Magnitude *= Math.Pow(1e-10, Exponent);
					return true;

				case "in":
				case "inch":
					Magnitude *= Math.Pow(0.0254, Exponent);
					return true;

				case "ft":
				case "foot":
					Magnitude *= Math.Pow(0.3048, Exponent);
					return true;

				case "yd":
				case "yard":
					Magnitude *= Math.Pow(0.9144, Exponent);
					return true;

				case "SM":
					Magnitude *= Math.Pow(1609.344, Exponent);
					return true;

				case "NM":
					Magnitude *= Math.Pow(1852, Exponent);
					return true;

				case "px":
					Magnitude *= Math.Pow(0.0254 / 96, Exponent);
					return true;

				case "pt":
					Magnitude *= Math.Pow(0.0254 / 72, Exponent);
					return true;

				case "pc":
					Magnitude *= Math.Pow(0.0254 / 6, Exponent);
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
