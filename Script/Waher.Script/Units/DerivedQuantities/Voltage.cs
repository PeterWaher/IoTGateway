using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// Electric potential difference, electromotive force
	/// </summary>
	public class Voltage : IDerivedQuantity
	{
		/// <summary>
		/// Electric potential difference, electromotive force
		/// </summary>
		public Voltage()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name => "Voltage";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly Unit reference = new Unit(new AtomicUnit("V"));

		/// <summary>
		/// Derived Units supported.
		/// </summary>
		public KeyValuePair<string, PhysicalQuantity>[] DerivedUnits
		{
			get
			{
				return new KeyValuePair<string, PhysicalQuantity>[]
				{
					new KeyValuePair<string, PhysicalQuantity>("V", new PhysicalQuantity(1, new Unit(Prefix.Kilo, 
						new UnitFactor[]
						{
							new UnitFactor("g"),
							new UnitFactor("m", 2),
							new UnitFactor("s", -3),
							new UnitFactor("A", -1)
						})))
				};
			}
		}
	}
}
