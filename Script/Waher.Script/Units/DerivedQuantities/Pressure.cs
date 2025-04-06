using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// Pressure (symbol: p or P) is the force applied perpendicular to the surface of an object per unit area over which that 
	/// force is distributed.
	/// </summary>
	public class Pressure : IDerivedQuantity
	{
		/// <summary>
		/// Pressure (symbol: p or P) is the force applied perpendicular to the surface of an object per unit area over which that 
		/// force is distributed.
		/// </summary>
		public Pressure()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name => "Pressure";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly Unit reference = new Unit(new AtomicUnit("Pa"));

		/// <summary>
		/// Derived Units supported.
		/// </summary>
		public KeyValuePair<string, PhysicalQuantity>[] DerivedUnits
		{
			get
			{
				return new KeyValuePair<string, PhysicalQuantity>[]
				{
					new KeyValuePair<string, PhysicalQuantity>("Pa", new PhysicalQuantity(1, new Unit(Prefix.Kilo, 
						new UnitFactor[]
						{
							new UnitFactor("g"),
							new UnitFactor("m", -1),
							new UnitFactor("s", -2)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("bar", new PhysicalQuantity(100, new Unit(Prefix.Mega,
						new UnitFactor[]
						{
							new UnitFactor("g"),
							new UnitFactor("m", -1),
							new UnitFactor("s", -2)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("psi", new PhysicalQuantity(6.894757, new Unit(Prefix.Mega,
						new UnitFactor[]
						{
							new UnitFactor("g"),
							new UnitFactor("m", -1),
							new UnitFactor("s", -2)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("atm", new PhysicalQuantity(101325, new Unit(Prefix.Kilo,
						new UnitFactor[]
						{
							new UnitFactor("g"),
							new UnitFactor("m", -1),
							new UnitFactor("s", -2)
						})))
				};
			}
		}
	}
}
