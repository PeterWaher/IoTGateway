using System;

namespace Waher.Script.Units.CompoundQuantities
{
	/// <summary>
	/// Energy, or work.
	/// 
	/// In physics, energy is a property of objects which can be transferred to other objects or converted into different forms.
	/// In physics, a force is said to do work if, when acting on a body, there is a displacement of the point of application in 
	/// the direction of the force.
	/// </summary>
	public class Energy : ICompoundQuantity
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
		/// Name of compound quantity.
		/// </summary>
		public string Name => "Energy";

		/// <summary>
		/// Reference unit of category.
		/// </summary>
		public Unit Reference => reference;

		private static readonly Unit reference = new Unit(new AtomicUnit("W"), new AtomicUnit("h"));

		/// <summary>
		/// Compound quantities. Must only use base quantity units.
		/// </summary>
		public Tuple<string, Prefix, UnitFactor[]>[] CompoundQuantities
		{
			get
			{
				return new Tuple<string, Prefix, UnitFactor[]>[]
				{
					new Tuple<string, Prefix, UnitFactor[]>("Wh", Prefix.None,
						new UnitFactor[]
						{
							new UnitFactor("W"),
							new UnitFactor("h")
						})
				};
			}
		}
	}
}
