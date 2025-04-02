using System.Collections.Generic;
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
		public string Name => "Power";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly Unit reference = new Unit(new AtomicUnit("W"));

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
						new UnitFactor[]
						{
							new UnitFactor("g"),
							new UnitFactor("m", 2),
							new UnitFactor("s", -3)
						})))
				};
			}
		}
	}
}
