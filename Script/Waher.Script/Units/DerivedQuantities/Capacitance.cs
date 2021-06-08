using System;
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
		public string Name
		{
			get { return "Capacitance"; }
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
					new KeyValuePair<string, PhysicalQuantity>("F", new PhysicalQuantity(1, new Unit(Prefix.Milli, 
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), -2),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("g"), -1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), 4),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("A"), 2)
						})))
				};
			}
		}
	}
}
