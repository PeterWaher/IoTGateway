using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// In physics, power is the rate of doing work.
	/// </summary>
	public class Power : IDerivedQuantity
	{
		/// <summary>
		/// In physics, power is the rate of doing work.
		/// </summary>
		public Power()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name
		{
			get { return "Power"; }
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
					new KeyValuePair<string, PhysicalQuantity>("W", new PhysicalQuantity(1, new Unit(Prefix.Kilo, 
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("g"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -3)
						})))
				};
			}
		}
	}
}
