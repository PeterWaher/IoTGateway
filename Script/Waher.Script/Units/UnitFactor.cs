namespace Waher.Script.Units
{
	/// <summary>
	/// A unit factor, used to form compound units.
	/// </summary>
	public class UnitFactor
	{
		private readonly AtomicUnit unit;
		private readonly int exponent;

		/// <summary>
		/// A unit factor, used to form compound units.
		/// </summary>
		/// <param name="Unit">Atomic unit.</param>
		/// <param name="Exponent">Exponent.</param>
		public UnitFactor(AtomicUnit Unit, int Exponent)
		{
			this.unit = Unit;
			this.exponent = Exponent;
		}

		/// <summary>
		/// A unit factor, used to form compound units.
		/// </summary>
		/// <param name="Unit">Atomic unit.</param>
		public UnitFactor(AtomicUnit Unit)
			: this(Unit, 1)
		{
		}

		/// <summary>
		/// A unit factor, used to form compound units.
		/// </summary>
		/// <param name="Unit">Atomic unit.</param>
		/// <param name="Exponent">Exponent.</param>
		public UnitFactor(string Unit, int Exponent)
			: this(new AtomicUnit(Unit), Exponent)
		{
		}

		/// <summary>
		/// A unit factor, used to form compound units.
		/// </summary>
		/// <param name="Unit">Atomic unit.</param>
		public UnitFactor(string Unit)
			: this(new AtomicUnit(Unit), 1)
		{
		}

		/// <summary>
		/// Unit factor, without its exponent.
		/// </summary>
		public AtomicUnit Unit => this.unit;

		/// <summary>
		/// Exponent of the unit factor.
		/// </summary>
		public int Exponent => this.exponent;
	}
}
