using System;
using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// Volume is the quantity of three-dimensional space enclosed by some closed boundary, for example, the space that a substance 
	/// (solid, liquid, gas, or plasma) or shape occupies or contains.
	/// </summary>
	public class Volume : IDerivedQuantity
	{
		/// <summary>
		/// Volume is the quantity of three-dimensional space enclosed by some closed boundary, for example, the space that a substance 
		/// (solid, liquid, gas, or plasma) or shape occupies or contains.
		/// </summary>
		public Volume()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name
		{
			get { return "Volume"; }
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
					new KeyValuePair<string, PhysicalQuantity>("l", new PhysicalQuantity(0.001, new Unit(Prefix.None, 
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 3)
						})))
				};
			}
		}
	}
}
