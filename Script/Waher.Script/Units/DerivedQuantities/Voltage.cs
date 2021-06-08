using System;
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
		public string Name
		{
			get { return "Voltage"; }
		}

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
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("g"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -3),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("A"), -1)
						})))
				};
			}
		}
	}
}
