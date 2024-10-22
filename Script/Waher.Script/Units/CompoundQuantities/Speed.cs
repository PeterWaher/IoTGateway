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
		public string Name => "Speed";

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
					new Tuple<string, Prefix, KeyValuePair<AtomicUnit, int>[]>("mph", Prefix.None, 
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("SM"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("h"), -1)
						}),
					new Tuple<string, Prefix, KeyValuePair<AtomicUnit, int>[]>("fps", Prefix.None,
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("ft"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1)
						}),
					new Tuple<string, Prefix, KeyValuePair<AtomicUnit, int>[]>("kph", Prefix.Kilo,
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("h"), -1)
						}),
					new Tuple<string, Prefix, KeyValuePair<AtomicUnit, int>[]>("kmph", Prefix.Kilo,
						new KeyValuePair<AtomicUnit, int>[]
						{
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1),
							new KeyValuePair<AtomicUnit, int>(new AtomicUnit("h"), -1)
						})
				};
			}
		}
	}
}
