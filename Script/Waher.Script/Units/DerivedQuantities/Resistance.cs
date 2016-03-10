using System;
using System.Collections.Generic;
using System.Text;
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
		public string Name
		{
			get { return "Resistance"; }
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
					new KeyValuePair<string, PhysicalQuantity>("Ω", new PhysicalQuantity(1, new Unit(Prefix.Kilo, 
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("g"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -3),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("A"), -2)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("Ohm", new PhysicalQuantity(1, new Unit(Prefix.Kilo,
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("g"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -3),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("A"), -2)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("ohm", new PhysicalQuantity(1, new Unit(Prefix.Kilo,
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("g"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -3),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("A"), -2)
						})))
				};
			}
		}
	}
}
