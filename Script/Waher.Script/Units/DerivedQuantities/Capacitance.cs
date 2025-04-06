using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// Capacitance.
	/// </summary>
	public class Capacitance : IDerivedQuantity
	{
		/// <summary>
		/// Capacitance.
		/// </summary>
		public Capacitance()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name => "Capacitance";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly Unit reference = new Unit(new AtomicUnit("F"));

		/// <summary>
		/// Derived Units supported.
		/// </summary>
		public KeyValuePair<string, PhysicalQuantity>[] DerivedUnits
		{
			get
			{
				return new KeyValuePair<string, PhysicalQuantity>[]
				{
					new KeyValuePair<string, PhysicalQuantity>("F", new PhysicalQuantity(1, new Unit(Prefix.Milli, 
						new UnitFactor[]
						{
							new UnitFactor("m", -2),
							new UnitFactor("g", -1),
							new UnitFactor("s", 4),
							new UnitFactor("A", 2)
						})))
				};
			}
		}
	}
}
