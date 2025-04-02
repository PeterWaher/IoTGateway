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
		public string Name => "Charge";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly Unit reference = new Unit(new AtomicUnit("C"));

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
						new UnitFactor[]
						{
							new UnitFactor("s"),
							new UnitFactor("A")
						})))
				};
			}
		}
	}
}
