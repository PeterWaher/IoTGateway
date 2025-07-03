using System;

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

		private static readonly Unit reference = new Unit(Prefix.None,
			new UnitFactor(new AtomicUnit("m"), 1), 
			new UnitFactor(new AtomicUnit("s"), -1));

		/// <summary>
		/// Compound quantities. Must only use base quantity units.
		/// </summary>
		public Tuple<string, Prefix, UnitFactor[]>[] CompoundQuantities
		{
			get
			{
				return new Tuple<string, Prefix, UnitFactor[]>[]
				{
					new Tuple<string, Prefix, UnitFactor[]>("mph", Prefix.None, 
						new UnitFactor[]
						{
							new UnitFactor("SM"),
							new UnitFactor("h", -1)
						}),
					new Tuple<string, Prefix, UnitFactor[]>("fps", Prefix.None,
						new UnitFactor[]
						{
							new UnitFactor("ft"),
							new UnitFactor("s", -1)
						}),
					new Tuple<string, Prefix, UnitFactor[]>("kph", Prefix.Kilo,
						new UnitFactor[]
						{
							new UnitFactor("m"),
							new UnitFactor("h", -1)
						}),
					new Tuple<string, Prefix, UnitFactor[]>("kmph", Prefix.Kilo,
						new UnitFactor[]
						{
							new UnitFactor("m"),
							new UnitFactor("h", -1)
						})
				};
			}
		}
	}
}
