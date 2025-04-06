using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// Resistance.
	/// </summary>
	public class Resistance : IDerivedQuantity
	{
		/// <summary>
		/// Resistance.
		/// </summary>
		public Resistance()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name => "Resistance";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly Unit reference = new Unit(new AtomicUnit("Ω"));

		/// <summary>
		/// Derived Units supported.
		/// </summary>
		public KeyValuePair<string, PhysicalQuantity>[] DerivedUnits
		{
			get
			{
				return new KeyValuePair<string, PhysicalQuantity>[]
				{
					new KeyValuePair<string, PhysicalQuantity>("Ω", new PhysicalQuantity(1, new Unit(Prefix.Kilo, 
						new UnitFactor[]
						{
							new UnitFactor("m", 2),
							new UnitFactor("g"),
							new UnitFactor("s", -3),
							new UnitFactor("A", -2)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("Ohm", new PhysicalQuantity(1, new Unit(Prefix.Kilo,
						new UnitFactor[]
						{
							new UnitFactor("m", 2),
							new UnitFactor("g"),
							new UnitFactor("s", -3),
							new UnitFactor("A", -2)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("ohm", new PhysicalQuantity(1, new Unit(Prefix.Kilo,
						new UnitFactor[]
						{
							new UnitFactor("m", 2),
							new UnitFactor("g"),
							new UnitFactor("s", -3),
							new UnitFactor("A", -2)
						})))
				};
			}
		}
	}
}
