using System;
using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// Frequency is the number of occurrences of a repeating event per unit time.
	/// </summary>
	public class Frequency : IDerivedQuantity
	{
		/// <summary>
		/// Frequency is the number of occurrences of a repeating event per unit time.
		/// </summary>
		public Frequency()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name
		{
			get { return "Frequency"; }
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
					new KeyValuePair<string, PhysicalQuantity>("Hz", new PhysicalQuantity(1, new Unit(Prefix.Kilo, 
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("cps", new PhysicalQuantity(1, new Unit(Prefix.Kilo,
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("rpm", new PhysicalQuantity(1, new Unit(Prefix.Kilo,
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("min"), -1)
						})))
				};
			}
		}
	}
}
