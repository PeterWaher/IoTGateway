using System.Collections.Generic;
using Waher.Script.Units;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// How to interpret physical units.
	/// </summary>
	public enum Ieee1451_0PhysicalUnitsInterpretation
	{
		/// <summary>
		/// Unit is described by the product of SI base units, plus radians and
		/// steradians, raised to the powers recorded in fields 2 through 10. Units for 
		/// some quantities, such as the number of people through a turnstile, cannot
		/// be represented using these units. Enumeration zero, with fields 2–10 set
		/// to 128, is the appropriate enumeration for these cases when a quantity is 
		/// being defined.
		/// </summary>
		PUI_SI_UNITS = 0,

		/// <summary>
		/// Unit is U/U, where U is described by the product of SI base units, plus
		/// radians and steradians, raised to the powers recorded in fields 2 through
		/// 10. 
		/// </summary>
		PUI_RATIO_SI_UNITS = 1,

		/// <summary>
		/// Unit is log10(U), where U is described by the product of SI base units,
		/// plus radians and steradians, raised to the powers recorded in fields 2 
		/// through 10. 
		/// </summary>
		PUI_LOG10_SI_UNITS = 2,

		/// <summary>
		/// Unit is log10(U/U), where U is described by the product of SI base units, 
		/// plus radians and steradians, raised to the powers recorded in fields 2 
		/// through 10. 
		/// </summary>
		PUI_LOG10_RATIO_SI_UNITS = 3,

		/// <summary>
		/// The associated quantity is digital data(for example, a bit vector) and has
		/// no unit.Fields 2–10 shall be set to 128. The “digital data” type applies to
		/// data that do not represent a quantity, such as the current positions of a
		/// bank of switches. 
		/// </summary>
		PUI_DIGITAL_DATA = 4,

		/// <summary>
		/// The associated physical quantity is represented by values on an arbitrary
		/// scale (for example, hardness). Fields 2–10 are reserved and shall be set
		/// to 128.
		/// </summary>
		PUI_ARBITRARY = 5
	}

	/// <summary>
	/// Physical Unit exponents.
	/// </summary>
	public struct PhysicalUnits
	{
		/// <summary>
		/// Physical Units interpretation
		/// </summary>
		public Ieee1451_0PhysicalUnitsInterpretation Interpretation;

		/// <summary>
		/// (2 * &lt;exponent of radians&gt;) + 128 
		/// </summary>
		public byte Radians;

		/// <summary>
		/// (2 * &lt;exponent of steradians&gt;) + 128 
		/// </summary>
		public byte Steradians;

		/// <summary>
		/// (2 * &lt;exponent of meters&gt;) + 128 
		/// </summary>
		public byte Meters;

		/// <summary>
		/// (2 * &lt;exponent of kilograms&gt;) + 128 
		/// </summary>
		public byte Kilograms;

		/// <summary>
		/// (2 * &lt;exponent of seconds&gt;) + 128 
		/// </summary>
		public byte Seconds;

		/// <summary>
		/// (2 * &lt;exponent of amperes&gt;) + 128 
		/// </summary>
		public byte Amperes;

		/// <summary>
		/// (2 * &lt;exponent of kelvins&gt;) + 128 
		/// </summary>
		public byte Kelvins;

		/// <summary>
		/// (2 * &lt;exponent of moles&gt;) + 128 
		/// </summary>
		public byte Moles;

		/// <summary>
		/// (2 * &lt;exponent of candelas&gt;) + 128
		/// </summary>
		public byte Candelas;

		/// <summary>
		/// Tries to create a unit from the description available in the object.
		/// </summary>
		/// <returns>Unit, if able, null if not.</returns>
		public Unit TryCreateUnit()
		{
			LinkedList<KeyValuePair<AtomicUnit, int>> Parts = new LinkedList<KeyValuePair<AtomicUnit, int>>();
			Prefix Prefix = Prefix.None;

			if (this.AddStep(Parts, "rad", this.Radians, ref Prefix, 0) &&
				this.AddStep(Parts, "sterad", this.Steradians, ref Prefix, 0) &&
				this.AddStep(Parts, "m", this.Meters, ref Prefix, 0) &&
				this.AddStep(Parts, "g", this.Kilograms, ref Prefix, 3) &&
				this.AddStep(Parts, "s", this.Seconds, ref Prefix, 0) &&
				this.AddStep(Parts, "A", this.Amperes, ref Prefix, 0) &&
				this.AddStep(Parts, "K", this.Kelvins, ref Prefix, 0) &&
				this.AddStep(Parts, "mol", this.Moles, ref Prefix, 0) &&
				this.AddStep(Parts, "Cd", this.Candelas, ref Prefix, 0))
			{
				return new Unit(Prefix, Parts);
			}
			else
				return null;
		}

		private bool AddStep(LinkedList<KeyValuePair<AtomicUnit, int>> Parts, string Unit, byte Exponent, 
			ref Prefix Prefix, int ExponentModifier)
		{
			int i = Exponent;
			if ((i & 1) != 0)
				return false;

			i -= 128;
			i /= 2;

			if (i != 0)
			{
				Parts.AddLast(new KeyValuePair<AtomicUnit, int>(new AtomicUnit(Unit), i));
				Prefix += ExponentModifier;
			}

			return true;
		}
	}
}
