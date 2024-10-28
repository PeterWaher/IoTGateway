namespace Waher.Script.Units.BaseQuantities
{
	/// <summary>
	/// A temperature is an objective comparative measure of hot or cold.
	/// </summary>
	public class Temperature : IBaseQuantity
	{
		/// <summary>
		/// A temperature is an objective comparative measure of hot or cold.
		/// </summary>
		public Temperature()
		{
		}

		/// <summary>
		/// Name of base quantity.
		/// </summary>
		public string Name => "Temperature";

		/// <summary>
		/// Reference unit of base quantity.
		/// </summary>
		public AtomicUnit ReferenceUnit => referenceUnit;

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly AtomicUnit referenceUnit = new AtomicUnit("K");
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
					"°C",
					//"C",		// To avoid confusion with the Coulomb unit C
					"°F",
					//"F",		// To avoid confusion with the Faraday unit F
					"K"
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
			if (Exponent == 1)
			{
				switch (BaseUnit)
				{
					case "°C":
					//case "C":		// To avoid confusion with the Coulomb unit C
						Magnitude += 273.15;
						return true;

					case "°F":
					//case "F":		// To avoid confusion with the Faraday unit F
						Magnitude = (Magnitude - 32) / 1.8 + 273.15;
						return true;

					case "K":
						return true;
				}
			}

			return false;
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
			if (Exponent == 1)
			{
				switch (BaseUnit)
				{
					case "°C":
					//case "C":		// To avoid confusion with the Coulomb unit C
						Magnitude -= 273.15;
						return true;

					case "°F":
					//case "F":		// To avoid confusion with the Faraday unit F
						Magnitude = (Magnitude - 273.15) * 1.8 + 32;
						return true;

					case "K":
						return true;
				}
			}

			return false;
		}

	}
}
