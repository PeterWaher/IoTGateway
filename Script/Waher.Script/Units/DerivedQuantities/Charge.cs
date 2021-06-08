using System;
using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// Electric charge.
	/// </summary>
	public class Charge : IDerivedQuantity
	{
		/// <summary>
		/// Electric charge.
		/// </summary>
		public Charge()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name
		{
			get { return "Charge"; }
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
					new KeyValuePair<string, PhysicalQuantity>("C", new PhysicalQuantity(1, new Unit(Prefix.None, 
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("A"), 1)
						})))
				};
			}
		}
	}
}
