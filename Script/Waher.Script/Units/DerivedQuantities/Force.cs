using System;
using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// In physics, a force is any interaction that, when unopposed, will change the motion of an object.
	/// </summary>
	public class Force : IDerivedQuantity
	{
		/// <summary>
		/// In physics, a force is any interaction that, when unopposed, will change the motion of an object.
		/// </summary>
		public Force()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name
		{
			get { return "Force"; }
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
					new KeyValuePair<string, PhysicalQuantity>("N", new PhysicalQuantity(1, new Unit(Prefix.Kilo, 
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("g"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -2)
						})))
				};
			}
		}
	}
}
