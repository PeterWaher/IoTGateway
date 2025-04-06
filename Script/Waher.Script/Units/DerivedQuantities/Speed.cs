using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// In everyday use and in kinematics, the speed of an object is the magnitude of its velocity (the rate of change of its position); 
	/// it is thus a scalar quantity.
	/// </summary>
	public class Speed : IDerivedQuantity
	{
		/// <summary>
		/// In everyday use and in kinematics, the speed of an object is the magnitude of its velocity (the rate of change of its position); 
		/// it is thus a scalar quantity.
		/// </summary>
		public Speed()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name => "Speed";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly Unit reference = new Unit(new AtomicUnit("knot"));

		/// <summary>
		/// Derived Units supported.
		/// </summary>
		public KeyValuePair<string, PhysicalQuantity>[] DerivedUnits
		{
			get
			{
				return new KeyValuePair<string, PhysicalQuantity>[]
				{
					new KeyValuePair<string, PhysicalQuantity>("knot", new PhysicalQuantity(0.514444, new Unit(Prefix.None,
						new UnitFactor[]
						{
							new UnitFactor("m"),
							new UnitFactor("s", -1)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("kn", new PhysicalQuantity(0.514444, new Unit(Prefix.None,
						new UnitFactor[]
						{
							new UnitFactor("m"),
							new UnitFactor("s", -1)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("kt", new PhysicalQuantity(0.514444, new Unit(Prefix.None,
						new UnitFactor[]
						{
							new UnitFactor("m"),
							new UnitFactor("s", -1)
						})))
				};
			}
		}
	}
}
