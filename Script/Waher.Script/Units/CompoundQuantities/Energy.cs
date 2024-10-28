using System;
using System.Collections.Generic;

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
		public Tuple<string, Prefix, KeyValuePair<AtomicUnit, int>[]>[] CompoundQuantities
		{
			get
			{
				return new Tuple<string, Prefix, KeyValuePair<AtomicUnit, int>[]>[]
				{
					new Tuple<string, Prefix, KeyValuePair<AtomicUnit, int>[]>("Wh", Prefix.None,
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("W"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("h"), 1)
						})
				};
			}
		}
	}
}
