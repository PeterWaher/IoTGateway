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
		public string Name => "Current";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public AtomicUnit ReferenceUnit => referenceUnit;

		/// <summary>
		/// Reference unit for category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly AtomicUnit referenceUnit = new AtomicUnit("A");
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
					"A"
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
			return BaseUnit == "A";
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
			return BaseUnit == "A";
		}

	}
}
