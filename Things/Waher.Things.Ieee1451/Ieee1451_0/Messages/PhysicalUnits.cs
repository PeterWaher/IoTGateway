using System;
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
		/// Binary byte array representation of the physical unit.
		/// </summary>
		public byte[] Binary;

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

			if (unitPerBase64.TryGetValue(Convert.ToBase64String(this.Binary), out string s))
				return new Unit(new AtomicUnit(s));

			if (this.AddStep(Parts, "rad", this.Radians, ref Prefix, 0) &&
				this.AddStep(Parts, "sr", this.Steradians, ref Prefix, 0) &&
				this.AddStep(Parts, "m", this.Meters, ref Prefix, 0) &&
				this.AddStep(Parts, "g", this.Kilograms, ref Prefix, 3) &&
				this.AddStep(Parts, "s", this.Seconds, ref Prefix, 0) &&
				this.AddStep(Parts, "A", this.Amperes, ref Prefix, 0) &&
				this.AddStep(Parts, "K", this.Kelvins, ref Prefix, 0) &&
				this.AddStep(Parts, "mol", this.Moles, ref Prefix, 0) &&
				this.AddStep(Parts, "cd", this.Candelas, ref Prefix, 0))
			{
				return new Unit(Prefix, Parts);
			}
			else
				return null;
		}

		private const byte _ = 128;
		private const byte p1 = 130;
		private const byte p2 = 132;
		private const byte p3 = 134;
		private const byte p4 = 136;
		private const byte m1 = 126;
		private const byte m2 = 124;
		private const byte m3 = 122;
		private static readonly KeyValuePair<string, byte[]>[] specialUnits = new KeyValuePair<string, byte[]>[]
			{	//                                                    rad  sr   m  kg   s   A   K mol  cd
				new KeyValuePair<string, byte[]>("Pa", new byte[] { 0,  _,  _, m1, p1, m2,  _,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("Ω", new byte[]  { 0,  _,  _, p2, p1, m3, m2,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("C", new byte[]  { 0,  _,  _,  _,  _, p1, p1,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("J", new byte[]  { 0,  _,  _, p2, p1, m2,  _,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("Hz", new byte[] { 0,  _,  _,  _,  _, m1,  _,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("N", new byte[]  { 0,  _,  _, p1, p1, m2,  _,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("W", new byte[]  { 0,  _,  _, p2, p1, m3,  _,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("V", new byte[]  { 0,  _,  _, p2, p1, m3, m1,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("F", new byte[]  { 0,  _,  _, m2, m1, p4, p2,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("S", new byte[]  { 0,  _,  _, m2, m1, p3, p2,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("Wb", new byte[] { 0,  _,  _, p2, p1, m2, m1,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("T", new byte[]  { 0,  _,  _,  _, p1, m2, m1,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("H", new byte[]  { 0,  _,  _, p2, p1, m2, m2,  _,  _,  _ }),
				new KeyValuePair<string, byte[]>("lm", new byte[] { 0,  _, p1,  _,  _,  _,  _,  _,  _, p1 }),
				new KeyValuePair<string, byte[]>("lx", new byte[] { 0,  _, p1, m2,  _,  _,  _,  _,  _, p1 })
			};
		private static readonly Dictionary<string, string> unitPerBase64 = OrderUnits();

		private static Dictionary<string, string> OrderUnits()
		{
			Dictionary<string, string> Result = new Dictionary<string, string>();

			foreach (KeyValuePair<string, byte[]> P in specialUnits)
				Result[Convert.ToBase64String(P.Value)] = P.Key;

			return Result;
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

		/// <summary>
		/// Tries to create an IEEE 1451 units object from a script unit.
		/// </summary>
		/// <param name="Unit">Script unit.</param>
		///	<param name="Value">Reference value.</param>
		/// <param name="Units">IEEE 1451 units object, if successful.</param>
		/// <returns>If able to convert the unit.</returns>
		public static bool TryCreate(Unit Unit, ref double Value, out PhysicalUnits Units)
		{
			Units = new PhysicalUnits()
			{
				Interpretation = Ieee1451_0PhysicalUnitsInterpretation.PUI_SI_UNITS,
				Radians = 128,
				Steradians = 128,
				Meters = 128,
				Kilograms = 128,
				Seconds = 128,
				Amperes = 128,
				Kelvins = 128,
				Moles = 128,
				Candelas = 128
			};

			if (Unit.HasFactors)
			{
				Unit = Unit.ToReferenceUnits(ref Value);

				foreach (KeyValuePair<AtomicUnit, int> P in Unit.Factors)
				{
					int Exponent = 128 + (P.Value << 1);
					if (Exponent < 0 || Exponent > 255)
						return false;

					byte Exp = (byte)Exponent;

					switch (P.Key.Name)
					{
						case "rad":
							Units.Radians = Exp;
							break;

						case "sr":
							Units.Steradians = Exp;
							break;

						case "m":
							Units.Meters = Exp;
							break;

						case "g":
							Units.Kilograms = Exp;
							Value *= Math.Pow(1000, -P.Value);
							break;

						case "s":
							Units.Seconds = Exp;
							break;

						case "A":
							Units.Amperes = Exp;
							break;

						case "K":
							Units.Kelvins = Exp;
							break;

						case "mol":
							Units.Moles = Exp;
							break;

						case "cd":
							Units.Candelas = Exp;
							break;

						default:
							return false;
					}
				}
			}

			return true;
		}
	}
}
