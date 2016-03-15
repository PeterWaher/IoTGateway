using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Objects;

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
		public string Name
		{
			get { return "Energy"; }
		}

		/// <summary>
		/// Compound quantities. Must only use base quantity units.
		/// </summary>
		public KeyValuePair<string, KeyValuePair<AtomicUnit, int>[]>[] CompoundQuantities
		{
			get
			{
				return new KeyValuePair<string, KeyValuePair<AtomicUnit, int>[]>[]
				{
					new KeyValuePair<string, KeyValuePair<AtomicUnit, int>[]>("Wh", 
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
