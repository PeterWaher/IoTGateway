using System;

namespace Waher.Script.Units
{
	/// <summary>
	/// Interface for physical base quantities 
	/// </summary>
	public interface IBaseQuantity
	{
		/// <summary>
		/// Name of base quantity.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Base Units supported.
		/// </summary>
		string[] BaseUnits
		{
			get;
		}

		/// <summary>
		/// Reference unit of base quantity.
		/// </summary>
		AtomicUnit ReferenceUnit
		{
			get;
		}

		/// <summary>
		/// Tries to convert a magnitude from a specified base unit, to the reference unit.
		/// </summary>
		/// <param name="Magnitude">Magnitude</param>
		/// <param name="BaseUnit">Base unit of <paramref name="Magnitude"/>.</param>
		/// <param name="Exponent">Exponent.</param>
		/// <returns>If the conversion was successful. If not, the magnitude value is unchanged.</returns>
		bool ToReferenceUnit(ref double Magnitude, string BaseUnit, int Exponent);

		/// <summary>
		/// Tries to convert a magnitude to a specified base unit, from the reference unit.
		/// </summary>
		/// <param name="Magnitude">Magnitude</param>
		/// <param name="BaseUnit">Desired base unit of <paramref name="Magnitude"/>.</param>
		/// <param name="Exponent">Exponent.</param>
		/// <returns>If the conversion was successful. If not, the magnitude value is unchanged.</returns>
		bool FromReferenceUnit(ref double Magnitude, string BaseUnit, int Exponent);

	}
}
