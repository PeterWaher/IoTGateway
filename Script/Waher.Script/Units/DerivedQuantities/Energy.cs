using System.Collections.Generic;
using Waher.Script.Objects;

namespace Waher.Script.Units.DerivedQuantities
{
	/// <summary>
	/// Energy, or work.
	/// 
	/// In physics, energy is a property of objects which can be transferred to other objects or converted into different forms.
	/// In physics, a force is said to do work if, when acting on a body, there is a displacement of the point of application in 
	/// the direction of the force.
	/// </summary>
	public class Energy : IDerivedQuantity
	{
		/// <summary>
		/// Energy, or work.
		/// 
		/// In physics, energy is a property of objects which can be transferred to other objects or converted into different forms.
		/// In physics, a force is said to do work if, when acting on a body, there is a displacement of the point of application in 
		/// the direction of the force.
		/// </summary>
		public Energy()
		{
		}

		/// <summary>
		/// Name of derived quantity.
		/// </summary>
		public string Name => "Energy";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly Unit reference = new Unit(new AtomicUnit("J"));

		/// <summary>
		/// Derived Units supported.
		/// </summary>
		public KeyValuePair<string, PhysicalQuantity>[] DerivedUnits
		{
			get
			{
				return new KeyValuePair<string, PhysicalQuantity>[]
				{
					new KeyValuePair<string, PhysicalQuantity>("J", new PhysicalQuantity(1, new Unit(Prefix.Kilo, 
						new UnitFactor[]
						{
							new UnitFactor("g"),
							new UnitFactor("m", 2),
							new UnitFactor("s", -2)
						}))),
					new KeyValuePair<string, PhysicalQuantity>("BTU", new PhysicalQuantity(1.055055853, new Unit(Prefix.Mega,
						new UnitFactor[]
						{
							new UnitFactor("g"),
							new UnitFactor("m", 2),
							new UnitFactor("s", -2)
						})))
				};
			}
		}
	}
}
