using System;
using System.Collections.Generic;

namespace Waher.Script.Units.CompoundQuantities
{
	/// <summary>
	/// In everyday use and in kinematics, the speed of an object is the magnitude of its velocity (the rate of change of its position); 
	/// it is thus a scalar quantity.
	/// </summary>
	public class Speed : ICompoundQuantity
	{
		/// <summary>
		/// In everyday use and in kinematics, the speed of an object is the magnitude of its velocity (the rate of change of its position); 
		/// it is thus a scalar quantity.
		/// </summary>
		public Speed()
		{
		}

		/// <summary>
		/// Name of compound quantity.
		/// </summary>
		public string Name
		{
			get { return "Speed"; }
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
					new KeyValuePair<string, KeyValuePair<AtomicUnit, int>[]>("mph", 
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("SM"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("h"), -1)
						}),
					new KeyValuePair<string, KeyValuePair<AtomicUnit, int>[]>("fps",
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("ft"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1)
						})
				};
			}
		}
	}
}
