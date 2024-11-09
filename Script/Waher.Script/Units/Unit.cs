using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Script.Objects;

namespace Waher.Script.Units
{
	/// <summary>
	/// Represents a unit.
	/// </summary>
	public sealed class Unit
	{
		private readonly Prefix prefix;
		private readonly ICollection<KeyValuePair<AtomicUnit, int>> factors;
		private bool hasBaseUnits = false;
		private bool hasReferenceUnits = false;

		/// <summary>
		/// Represents a unit.
		/// </summary>
		/// <param name="Prefix">Associated prefix.</param>
		/// <param name="Factors">Sequence of atomic unit factors, and their corresponding exponents.</param>
		public Unit(Prefix Prefix, ICollection<KeyValuePair<AtomicUnit, int>> Factors)
		{
			this.prefix = Prefix;
			this.factors = Factors;
		}

		/// <summary>
		/// Represents a unit.
		/// </summary>
		/// <param name="Prefix">Associated prefix.</param>
		/// <param name="Factors">Sequence of atomic unit factors, and their corresponding exponents.</param>
		public Unit(Prefix Prefix, params KeyValuePair<AtomicUnit, int>[] Factors)
		{
			this.prefix = Prefix;
			this.factors = Factors;
		}

		/// <summary>
		/// Represents a unit.
		/// </summary>
		/// <param name="Prefix">Associated prefix.</param>
		/// <param name="Factors">Sequence of atomic unit factors, and their corresponding exponents.</param>
		public Unit(Prefix Prefix, params KeyValuePair<string, int>[] Factors)
		{
			this.prefix = Prefix;

			int i, c = Factors.Length;
			KeyValuePair<AtomicUnit, int>[] Factors2 = new KeyValuePair<AtomicUnit, int>[c];

			for (i = 0; i < c; i++)
				Factors2[i] = new KeyValuePair<AtomicUnit, int>(new AtomicUnit(Factors[i].Key), Factors[i].Value);

			this.factors = Factors2;
		}

		/// <summary>
		/// Represents a unit.
		/// </summary>
		/// <param name="AtomicUnits">Sequence of atomic units.</param>
		public Unit(params AtomicUnit[] AtomicUnits)
			: this(Prefix.None, AtomicUnits)
		{
		}

		/// <summary>
		/// Represents a unit.
		/// </summary>
		/// <param name="Prefix">Associated prefix.</param>
		/// <param name="AtomicUnits">Sequence of atomic units.</param>
		public Unit(Prefix Prefix, params AtomicUnit[] AtomicUnits)
			: this(Prefix, Prepare(AtomicUnits))
		{
		}

		private static KeyValuePair<AtomicUnit, int>[] Prepare(AtomicUnit[] AtomicUnits)
		{
			int i, c = AtomicUnits.Length;
			KeyValuePair<AtomicUnit, int>[] Result = new KeyValuePair<AtomicUnit, int>[c];

			for (i = 0; i < c; i++)
				Result[i] = new KeyValuePair<AtomicUnit, int>(AtomicUnits[i], 1);

			return Result;
		}

		/// <summary>
		/// Represents a unit.
		/// </summary>
		/// <param name="AtomicUnits">Sequence of atomic units.</param>
		public Unit(params string[] AtomicUnits)
			: this(Prefix.None, AtomicUnits)
		{
		}

		/// <summary>
		/// Represents a unit.
		/// </summary>
		/// <param name="Prefix">Associated prefix.</param>
		/// <param name="AtomicUnits">Sequence of atomic units.</param>
		public Unit(Prefix Prefix, params string[] AtomicUnits)
		{
			this.prefix = Prefix;

			int i, c = AtomicUnits.Length;
			KeyValuePair<AtomicUnit, int>[] Factors = new KeyValuePair<AtomicUnit, int>[c];

			for (i = 0; i < c; i++)
				Factors[i] = new KeyValuePair<AtomicUnit, int>(new AtomicUnit(AtomicUnits[i]), 1);

			this.factors = Factors;
		}

		/// <summary>
		/// Parses a unit string.
		/// </summary>
		/// <param name="UnitString">Unit string</param>
		/// <returns>Parsed unit.</returns>
		public static Unit Parse(string UnitString)
		{
			if (TryParse(UnitString, out Unit Unit))
				return Unit;
			else
				throw new Exceptions.ScriptException("Unable to parse unit: " + UnitString);
		}

		/// <summary>
		/// Tries to parse a string into a unit.
		/// </summary>
		/// <param name="UnitString">String expression</param>
		/// <param name="Unit">Parsed unit.</param>
		/// <returns>If string could be parsed into a unit.</returns>
		public static bool TryParse(string UnitString, out Unit Unit)
		{
			int Pos = 0;
			int Len = UnitString?.Length ?? 0;

			if (!TryParse(UnitString, ref Pos, Len, true, out Unit))
				return false;

			if (Pos < Len)
				return false;

			return true;
		}

		private static bool TryParse(string UnitString, ref int Pos, int Len, bool PermitPrefix, out Unit Unit)
		{
			Unit = null;
			if (Pos >= Len)
				return false;

			Prefix Prefix;
			LinkedList<KeyValuePair<AtomicUnit, int>> Factors = new LinkedList<KeyValuePair<AtomicUnit, int>>();
			KeyValuePair<Prefix, KeyValuePair<AtomicUnit, int>[]> CompoundFactors;
			bool HasCompoundFactors;
			string Name, Name2, s;
			int Exponent;
			int Start = Pos;
			int i;
			char ch = UnitString[Pos++];
			bool LastDivision = false;

			if (PermitPrefix)
			{
				if (ch == 'd' && Pos < Len && UnitString[Pos] == 'a')
				{
					Pos++;
					Prefix = Prefix.Deka;
				}
				else if (!Prefixes.TryParsePrefix(ch, out Prefix))
					Pos--;

				i = Pos;
				ch = Pos < Len ? UnitString[Pos++] : (char)0;
			}
			else
			{
				Prefix = Prefix.None;
				i = Pos - 1;
			}

			if (!char.IsLetter(ch) && Prefix != Prefix.None)
			{
				Pos = i = Start;
				Prefix = Prefix.None;
				ch = Pos < Len ? UnitString[Pos++] : (char)0;
			}
			else if (ch == '/')
			{
				LastDivision = true;
				ch = Pos < Len ? UnitString[Pos++] : (char)0;
				while (ch > 0 && (ch <= ' ' || ch == 160))
					ch = Pos < Len ? UnitString[Pos++] : (char)0;
			}

			while (char.IsLetter(ch) || ch == '(' || ch == '°')
			{
				if (ch == '(')
				{
					if (!TryParse(UnitString, ref Pos, Len, false, out Unit Unit2))
						return false;

					ch = Pos < Len ? UnitString[Pos++] : (char)0;
					while (ch > 0 && (ch <= ' ' || ch == 160))
						ch = Pos < Len ? UnitString[Pos++] : (char)0;

					if (ch != ')')
						return false;

					ch = Pos < Len ? UnitString[Pos++] : (char)0;
					while (ch > 0 && (ch <= ' ' || ch == 160))
						ch = Pos < Len ? UnitString[Pos++] : (char)0;

					if (ch == '^')
					{
						ch = Pos < Len ? UnitString[Pos++] : (char)0;
						while (ch > 0 && (ch <= ' ' || ch == 160))
							ch = Pos < Len ? UnitString[Pos++] : (char)0;

						if (ch == '-' || char.IsDigit(ch))
						{
							i = Pos - 1;

							if (ch == '-')
								ch = Pos < Len ? UnitString[Pos++] : (char)0;

							while (char.IsDigit(ch))
								ch = Pos < Len ? UnitString[Pos++] : (char)0;

							if (ch == 0)
								s = UnitString.Substring(i, Pos - i);
							else
								s = UnitString.Substring(i, Pos - i - 1);

							if (!int.TryParse(s, out Exponent))
								return false;
						}
						else
							return false;
					}
					else if (ch == '²')
					{
						Exponent = 2;
						ch = Pos < Len ? UnitString[Pos++] : (char)0;
					}
					else if (ch == '³')
					{
						Exponent = 3;
						ch = Pos < Len ? UnitString[Pos++] : (char)0;
					}
					else
						Exponent = 1;

					if (LastDivision)
					{
						foreach (KeyValuePair<AtomicUnit, int> Factor in Unit2.Factors)
							Factors.AddLast(new KeyValuePair<AtomicUnit, int>(Factor.Key, -Factor.Value * Exponent));
					}
					else
					{
						foreach (KeyValuePair<AtomicUnit, int> Factor in Unit2.Factors)
							Factors.AddLast(new KeyValuePair<AtomicUnit, int>(Factor.Key, Factor.Value * Exponent));
					}
				}
				else
				{
					if (ch == '°')
						ch = Pos < Len ? UnitString[Pos++] : (char)0;

					while (char.IsLetter(ch))
						ch = Pos < Len ? UnitString[Pos++] : (char)0;

					if (ch == 0)
						Name = UnitString.Substring(i, Pos - i);
					else
						Name = UnitString.Substring(i, Pos - i - 1);

					if (PermitPrefix)
					{
						if (Expression.keywords.ContainsKey(Name2 = UnitString.Substring(Start, i - Start) + Name))
							return false;
						else if (HasCompoundFactors = TryGetCompoundUnit(Name2, out CompoundFactors))
						{
							Prefix = CompoundFactors.Key;
							Name = Name2;
						}
						else if (ContainsDerivedOrBaseUnit(Name2))
						{
							Prefix = Prefix.None;
							Name = Name2;
						}
						else
							HasCompoundFactors = TryGetCompoundUnit(Name, out CompoundFactors);
					}
					else
						HasCompoundFactors = TryGetCompoundUnit(Name, out CompoundFactors);

					while (ch > 0 && (ch <= ' ' || ch == 160))
						ch = Pos < Len ? UnitString[Pos++] : (char)0;

					if (ch == '^')
					{
						ch = Pos < Len ? UnitString[Pos++] : (char)0;
						while (ch > 0 && (ch <= ' ' || ch == 160))
							ch = Pos < Len ? UnitString[Pos++] : (char)0;

						if (ch == '-' || char.IsDigit(ch))
						{
							i = Pos - 1;

							if (ch == '-')
								ch = Pos < Len ? UnitString[Pos++] : (char)0;

							while (char.IsDigit(ch))
								ch = Pos < Len ? UnitString[Pos++] : (char)0;

							if (ch == 0)
								s = UnitString.Substring(i, Pos - i);
							else
								s = UnitString.Substring(i, Pos - i - 1);

							if (!int.TryParse(s, out Exponent))
								return false;
						}
						else
							return false;
					}
					else if (ch == '²')
					{
						Exponent = 2;
						ch = Pos < Len ? UnitString[Pos++] : (char)0;
					}
					else if (ch == '³')
					{
						Exponent = 3;
						ch = Pos < Len ? UnitString[Pos++] : (char)0;
					}
					else
						Exponent = 1;

					if (HasCompoundFactors)
					{
						if (LastDivision)
						{
							foreach (KeyValuePair<AtomicUnit, int> Segment in CompoundFactors.Value)
								Factors.AddLast(new KeyValuePair<AtomicUnit, int>(Segment.Key, -Segment.Value * Exponent));
						}
						else
						{
							foreach (KeyValuePair<AtomicUnit, int> Segment in CompoundFactors.Value)
								Factors.AddLast(new KeyValuePair<AtomicUnit, int>(Segment.Key, Segment.Value * Exponent));
						}
					}
					else
					{
						if (LastDivision)
							Factors.AddLast(new KeyValuePair<AtomicUnit, int>(new AtomicUnit(Name), -Exponent));
						else
							Factors.AddLast(new KeyValuePair<AtomicUnit, int>(new AtomicUnit(Name), Exponent));
					}
				}

				while (ch > 0 && (ch <= ' ' || ch == 160))
					ch = Pos < Len ? UnitString[Pos++] : (char)0;

				if (ch == '*' || ch == '⋅')
					LastDivision = false;
				else if (ch == '/')
					LastDivision = true;
				else if (ch == ')')
				{
					Pos--;
					break;
				}
				else if (ch == 0)
					break;
				else
					return false;

				ch = Pos < Len ? UnitString[Pos++] : (char)0;
				while (ch > 0 && (ch <= ' ' || ch == 160))
					ch = Pos < Len ? UnitString[Pos++] : (char)0;

				i = Pos - 1;
				PermitPrefix = false;
			}

			if (Factors.First is null)
				return false;

			Unit = new Unit(Prefix, Factors);

			return true;
		}

		/// <summary>
		/// Associated prefix.
		/// </summary>
		public Prefix Prefix => this.prefix;

		/// <summary>
		/// Sequence of atomic unit factors, and their corresponding exponents.
		/// </summary>
		public ICollection<KeyValuePair<AtomicUnit, int>> Factors => this.factors;

		/// <summary>
		/// If the unit is empty. (A unit of only a prefix, but no factors, is not empty.)
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return this.prefix == Prefix.None && this.factors.Count == 0;
			}
		}

		/// <summary>
		/// If the unit has any factors.
		/// </summary>
		public bool HasFactors
		{
			get
			{
				return this.factors.Count > 0;
			}
		}

		/// <summary>
		/// Empty unit.
		/// </summary>
		public static readonly Unit Empty = new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>[0]);

		/// <summary>
		/// Inverts the unit.
		/// </summary>
		/// <param name="ResidueExponent">Any exponential residue. If this value is 0, it means the exponent corresponds exactly 
		/// to the returned prefix.</param>
		/// <returns>Inverted unit.</returns>
		public Unit Invert(out int ResidueExponent)
		{
			int Exponent = Prefixes.PrefixToExponent(this.prefix);
			LinkedList<KeyValuePair<AtomicUnit, int>> Factors = new LinkedList<KeyValuePair<AtomicUnit, int>>();

			foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
				Factors.AddLast(new KeyValuePair<AtomicUnit, int>(Factor.Key, -Factor.Value));

			return new Unit(Prefixes.ExponentToPrefix(-Exponent, out ResidueExponent), Factors);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is Unit U))
				return false;

			return this.Equals(U, true);
		}

		/// <summary>
		/// Checks if the unit is equal to another
		/// </summary>
		/// <param name="Unit2">Second unit.</param>
		/// <param name="CheckPrefix"></param>
		/// <returns></returns>
		public bool Equals(Unit Unit2, bool CheckPrefix)
		{
			if (CheckPrefix && this.prefix != Unit2.prefix)
				return false;

			if (this.factors.Count != Unit2.factors.Count)
				return false;

			string Name;
			bool Found;

			foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
			{
				Name = Factor.Key.Name;
				Found = false;

				foreach (KeyValuePair<AtomicUnit, int> Factor2 in Unit2.factors)
				{
					if (Name == Factor2.Key.Name)
					{
						if (Factor.Value != Factor2.Value)
							return false;

						Found = true;
						break;
					}
				}

				if (!Found)
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int i = (int)this.prefix;

			foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
				i ^= Factor.Key.GetHashCode() ^ Factor.Value.GetHashCode();

			return i;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.ToString(true);
		}

		/// <summary>
		/// Converts the unit to a string.
		/// </summary>
		/// <param name="IncludePrefix">If the prefix should be included in the string or not.</param>
		public string ToString(bool IncludePrefix)
		{
			StringBuilder Numerator = null;
			StringBuilder Denominator = null;
			int NrDenominators = 0;

			foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
			{
				if (Factor.Value > 0)
				{
					if (Numerator is null)
					{
						if (IncludePrefix)
							Numerator = new StringBuilder(Prefixes.ToString(this.prefix));
						else
							Numerator = new StringBuilder();
					}
					else
						Numerator.Append('⋅');

					Numerator.Append(Factor.Key.Name);

					if (Factor.Value > 1)
					{
						if (Factor.Value == 2)
							Numerator.Append('²');
						else if (Factor.Value == 3)
							Numerator.Append('³');
						else
						{
							Numerator.Append('^');
							Numerator.Append(Factor.Value.ToString());
						}
					}
				}
				else
				{
					if (Denominator is null)
						Denominator = new StringBuilder();
					else
						Denominator.Append('⋅');

					NrDenominators++;
					Denominator.Append(Factor.Key.Name);

					if (Factor.Value < -1)
					{
						if (Factor.Value == -2)
							Denominator.Append('²');
						else if (Factor.Value == -3)
							Denominator.Append('³');
						else
						{
							Denominator.Append('^');
							Denominator.Append((-Factor.Value).ToString());
						}
					}
				}
			}

			if (Numerator is null)
			{
				if (IncludePrefix)
					Numerator = new StringBuilder(Prefixes.ToString(this.prefix));
				else
					Numerator = new StringBuilder();
			}

			if (!(Denominator is null))
			{
				Numerator.Append('/');

				if (NrDenominators > 1)
					Numerator.Append('(');

				Numerator.Append(Denominator.ToString());

				if (NrDenominators > 1)
					Numerator.Append(')');
			}

			return Numerator.ToString();
		}

		/// <summary>
		/// Multiplies two units with each other.
		/// </summary>
		/// <param name="Left">Left unit.</param>
		/// <param name="Right">Right unit.</param>
		/// <param name="ResidueExponent">Any residual exponent resulting from the multiplication.</param>
		/// <returns>Resulting unit.</returns>
		public static Unit Multiply(Unit Left, Unit Right, out int ResidueExponent)
		{
			LinkedList<KeyValuePair<AtomicUnit, int>> Result = new LinkedList<KeyValuePair<AtomicUnit, int>>();
			int LeftExponent = Prefixes.PrefixToExponent(Left.prefix);
			int RightExponent = Prefixes.PrefixToExponent(Right.prefix);
			int ResultExponent = LeftExponent + RightExponent;
			Prefix ResultPrefix = Prefixes.ExponentToPrefix(ResultExponent, out ResidueExponent);
			LinkedListNode<KeyValuePair<AtomicUnit, int>> Loop;
			string Name;
			int Exponent;
			bool Found;

			foreach (KeyValuePair<AtomicUnit, int> Factor in Left.factors)
				Result.AddLast(Factor);

			foreach (KeyValuePair<AtomicUnit, int> Factor in Right.factors)
			{
				Name = Factor.Key.Name;
				Loop = Result.First;
				Found = false;

				while (!(Loop is null) && !Found)
				{
					if (Loop.Value.Key.Name == Name)
					{
						Found = true;
						Exponent = Loop.Value.Value + Factor.Value;
						if (Exponent == 0)
							Result.Remove(Loop);
						else
							Loop.Value = new KeyValuePair<AtomicUnit, int>(Loop.Value.Key, Exponent);
					}
					else
						Loop = Loop.Next;
				}

				if (Found)
					continue;

				Result.AddLast(Factor);
			}

			return new Unit(ResultPrefix, Result);
		}

		/// <summary>
		/// Divides the right unit from the left unit: <paramref name="Left"/>/<paramref name="Right"/>.
		/// </summary>
		/// <param name="Left">Left unit.</param>
		/// <param name="Right">Right unit.</param>
		/// <param name="ResidueExponent">Any residual exponent resulting from the division.</param>
		/// <returns>Resulting unit.</returns>
		public static Unit Divide(Unit Left, Unit Right, out int ResidueExponent)
		{
			LinkedList<KeyValuePair<AtomicUnit, int>> Result = new LinkedList<KeyValuePair<AtomicUnit, int>>();
			int LeftExponent = Prefixes.PrefixToExponent(Left.prefix);
			int RightExponent = Prefixes.PrefixToExponent(Right.prefix);
			int ResultExponent = LeftExponent - RightExponent;
			Prefix ResultPrefix = Prefixes.ExponentToPrefix(ResultExponent, out ResidueExponent);
			LinkedListNode<KeyValuePair<AtomicUnit, int>> Loop;
			string Name;
			int Exponent;
			bool Found;

			foreach (KeyValuePair<AtomicUnit, int> Factor in Left.factors)
				Result.AddLast(Factor);

			foreach (KeyValuePair<AtomicUnit, int> Factor in Right.factors)
			{
				Name = Factor.Key.Name;
				Loop = Result.First;
				Found = false;

				while (!(Loop is null) && !Found)
				{
					if (Loop.Value.Key.Name == Name)
					{
						Found = true;
						Exponent = Loop.Value.Value - Factor.Value;
						if (Exponent == 0)
							Result.Remove(Loop);
						else
							Loop.Value = new KeyValuePair<AtomicUnit, int>(Loop.Value.Key, Exponent);
					}
					else
						Loop = Loop.Next;
				}

				if (Found)
					continue;

				Result.AddLast(new KeyValuePair<AtomicUnit, int>(Factor.Key, -Factor.Value));
			}

			return new Unit(ResultPrefix, Result);
		}

		/// <summary>
		/// Converts the unit to a series of base unit factors. (Unrecognized units will be assumed to be base units.)
		/// </summary>
		/// <param name="Magnitude">Reference magnitude.</param>
		/// <returns>Unit consisting of base unit factors.</returns>
		public Unit ToBaseUnits(ref double Magnitude)
		{
			if (this.hasBaseUnits)
				return this;

			lock (synchObject)
			{
				if (baseUnits is null)
					Search();

				bool HasNonBase = false;

				foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
				{
					if (!baseUnits.ContainsKey(Factor.Key.Name))
					{
						HasNonBase = true;
						break;
					}
				}

				if (HasNonBase)
				{
					LinkedList<KeyValuePair<AtomicUnit, int>> BaseFactors = new LinkedList<KeyValuePair<AtomicUnit, int>>();
					int Exponent = Prefixes.PrefixToExponent(this.prefix);
					int FactorExponent;
					string Name;

					foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
					{
						FactorExponent = Factor.Value;

						if (baseUnits.ContainsKey(Name = Factor.Key.Name))
							this.Add(BaseFactors, Factor.Key, Factor.Value);
						else if (derivedUnits.TryGetValue(Name, out PhysicalQuantity Quantity))
						{
							Magnitude *= Math.Pow(Quantity.Magnitude, FactorExponent);
							Exponent += Prefixes.PrefixToExponent(Quantity.Unit.prefix) * FactorExponent;

							foreach (KeyValuePair<AtomicUnit, int> Segment in Quantity.Unit.factors)
								this.Add(BaseFactors, Segment.Key, Segment.Value * FactorExponent);
						}
						else if (compoundUnits.TryGetValue(Name, out KeyValuePair<Prefix, KeyValuePair<AtomicUnit, int>[]> Units))
						{
							Exponent += Prefixes.PrefixToExponent(Units.Key) * FactorExponent;

							foreach (KeyValuePair<AtomicUnit, int> Segment in Units.Value)
								this.Add(BaseFactors, Segment.Key, Segment.Value * FactorExponent);
						}
						else
							this.Add(BaseFactors, Factor.Key, Factor.Value);
					}

					Unit Result = new Unit(Prefixes.ExponentToPrefix(Exponent, out FactorExponent), BaseFactors);
					if (FactorExponent != 0)
						Magnitude *= Math.Pow(10, FactorExponent);

					Result.hasBaseUnits = true;

					return Result;
				}
				else
				{
					this.hasBaseUnits = true;
					return this;
				}
			}
		}

		private void Add(LinkedList<KeyValuePair<AtomicUnit, int>> Factors, AtomicUnit AtomicUnit, int Exponent)
		{
			LinkedListNode<KeyValuePair<AtomicUnit, int>> Loop = Factors.First;
			string Name = AtomicUnit.Name;

			while (!(Loop is null) && Loop.Value.Key.Name != Name)
				Loop = Loop.Next;

			if (Loop is null)
				Factors.AddLast(new KeyValuePair<AtomicUnit, int>(AtomicUnit, Exponent));
			else
				Loop.Value = new KeyValuePair<AtomicUnit, int>(AtomicUnit, Loop.Value.Value + Exponent);
		}

		/// <summary>
		/// Converts the unit to a series of reference unit factors. (Unrecognized units will be assumed to be reference units.)
		/// </summary>
		/// <param name="Magnitude">Reference magnitude.</param>
		/// <returns>Unit consisting of reference unit factors.</returns>
		public Unit ToReferenceUnits(ref double Magnitude)
		{
			double x = 0;
			return this.ToReferenceUnits(ref Magnitude, ref x);
		}

		/// <summary>
		/// Converts the unit to a series of reference unit factors. (Unrecognized units will be assumed to be reference units.)
		/// </summary>
		/// <param name="Magnitude">Reference magnitude.</param>
		/// <param name="NrDecimals">Number of decimals to present.</param>
		/// <returns>Unit consisting of reference unit factors.</returns>
		public Unit ToReferenceUnits(ref double Magnitude, ref double NrDecimals)
		{
			if (this.hasReferenceUnits)
				return this;

			lock (synchObject)
			{
				if (baseUnits is null)
					Search();

				bool HasNonReference = false;

				foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
				{
					if (!referenceUnits.ContainsKey(Factor.Key.Name))
					{
						HasNonReference = true;
						break;
					}
				}

				if (HasNonReference)
				{
					LinkedList<KeyValuePair<AtomicUnit, int>> ReferenceFactors = new LinkedList<KeyValuePair<AtomicUnit, int>>();
					int Exponent = Prefixes.PrefixToExponent(this.prefix);
					int FactorExponent;
					string Name;

					foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
					{
						FactorExponent = Factor.Value;

						if (referenceUnits.ContainsKey(Name = Factor.Key.Name))
							this.Add(ReferenceFactors, Factor.Key, Factor.Value);
						else if (baseUnits.TryGetValue(Name, out IBaseQuantity BaseQuantity))
						{
							if (BaseQuantity.ToReferenceUnit(ref Magnitude, ref NrDecimals, Name, FactorExponent))
								this.Add(ReferenceFactors, BaseQuantity.ReferenceUnit, FactorExponent);
							else
								this.Add(ReferenceFactors, Factor.Key, Factor.Value);
						}
						else if (derivedUnits.TryGetValue(Name, out PhysicalQuantity Quantity))
						{
							if (FactorExponent != 0)
							{
								double k = Math.Pow(Quantity.Magnitude, FactorExponent);
								Magnitude *= k;
								NrDecimals -= Math.Log10(k);
								Exponent += Prefixes.PrefixToExponent(Quantity.Unit.prefix) * FactorExponent;
							}

							foreach (KeyValuePair<AtomicUnit, int> Segment in Quantity.Unit.factors)
							{
								if (referenceUnits.ContainsKey(Name = Segment.Key.Name))
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								else if (baseUnits.TryGetValue(Name, out BaseQuantity))
								{
									if (BaseQuantity.ToReferenceUnit(ref Magnitude, ref NrDecimals, Name, Segment.Value * FactorExponent))
										this.Add(ReferenceFactors, BaseQuantity.ReferenceUnit, Segment.Value * FactorExponent);
									else
										this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								}
								else
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
							}
						}
						else if (compoundUnits.TryGetValue(Name, out KeyValuePair<Prefix, KeyValuePair<AtomicUnit, int>[]> Units))
						{
							Exponent += Prefixes.PrefixToExponent(Units.Key) * FactorExponent;

							foreach (KeyValuePair<AtomicUnit, int> Segment in Units.Value)
							{
								if (referenceUnits.ContainsKey(Name = Segment.Key.Name))
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								else if (baseUnits.TryGetValue(Name, out BaseQuantity))
								{
									if (BaseQuantity.ToReferenceUnit(ref Magnitude, ref NrDecimals, Name, Segment.Value * FactorExponent))
										this.Add(ReferenceFactors, BaseQuantity.ReferenceUnit, Segment.Value * FactorExponent);
									else
										this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								}
								else
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
							}
						}
						else
							this.Add(ReferenceFactors, Factor.Key, Factor.Value);
					}

					Unit Result = new Unit(Prefixes.ExponentToPrefix(Exponent, out FactorExponent), ReferenceFactors);
					if (FactorExponent != 0)
					{
						Magnitude *= Math.Pow(10, FactorExponent);
						NrDecimals -= FactorExponent;
					}

					Result.hasBaseUnits = true;
					Result.hasReferenceUnits = true;

					return Result;
				}
				else
				{
					this.hasBaseUnits = true;
					this.hasReferenceUnits = true;
					return this;
				}
			}
		}

		/// <summary>
		/// Converts the unit to a series of reference unit factors. (Unrecognized units will be assumed to be reference units.)
		/// </summary>
		/// <param name="Magnitude">Reference magnitude.</param>
		/// <returns>Unit consisting of reference unit factors.</returns>
		public Unit FromReferenceUnits(ref double Magnitude)
		{
			double x = 0;
			return this.FromReferenceUnits(ref Magnitude, ref x);
		}

		/// <summary>
		/// Converts the unit to a series of reference unit factors. (Unrecognized units will be assumed to be reference units.)
		/// </summary>
		/// <param name="Magnitude">Reference magnitude.</param>
		/// <param name="NrDecimals">Number of decimals to present.</param>
		/// <returns>Unit consisting of reference unit factors.</returns>
		public Unit FromReferenceUnits(ref double Magnitude, ref double NrDecimals)
		{
			if (this.hasReferenceUnits)
				return this;

			lock (synchObject)
			{
				if (baseUnits is null)
					Search();

				bool HasNonReference = false;

				foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
				{
					if (!referenceUnits.ContainsKey(Factor.Key.Name))
					{
						HasNonReference = true;
						break;
					}
				}

				if (HasNonReference)
				{
					LinkedList<KeyValuePair<AtomicUnit, int>> ReferenceFactors = new LinkedList<KeyValuePair<AtomicUnit, int>>();
					int Exponent = Prefixes.PrefixToExponent(this.prefix);
					int FactorExponent;
					string Name;

					foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
					{
						FactorExponent = Factor.Value;

						if (referenceUnits.ContainsKey(Name = Factor.Key.Name))
							this.Add(ReferenceFactors, Factor.Key, Factor.Value);
						else if (baseUnits.TryGetValue(Name, out IBaseQuantity BaseQuantity))
						{
							if (BaseQuantity.FromReferenceUnit(ref Magnitude, ref NrDecimals, Name, FactorExponent))
								this.Add(ReferenceFactors, BaseQuantity.ReferenceUnit, FactorExponent);
							else
								this.Add(ReferenceFactors, Factor.Key, Factor.Value);
						}
						else if (derivedUnits.TryGetValue(Name, out PhysicalQuantity Quantity))
						{
							if (FactorExponent != 0)
							{
								double k = Math.Pow(Quantity.Magnitude, FactorExponent);
								Magnitude /= k;
								NrDecimals += Math.Log10(k);
								Exponent += Prefixes.PrefixToExponent(Quantity.Unit.prefix) * FactorExponent;
							}

							foreach (KeyValuePair<AtomicUnit, int> Segment in Quantity.Unit.factors)
							{
								if (referenceUnits.ContainsKey(Name = Segment.Key.Name))
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								else if (baseUnits.TryGetValue(Name, out BaseQuantity))
								{
									if (BaseQuantity.FromReferenceUnit(ref Magnitude, ref NrDecimals, Name, Segment.Value * FactorExponent))
										this.Add(ReferenceFactors, BaseQuantity.ReferenceUnit, Segment.Value * FactorExponent);
									else
										this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								}
								else
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
							}
						}
						else if (compoundUnits.TryGetValue(Name, out KeyValuePair<Prefix, KeyValuePair<AtomicUnit, int>[]> Units))
						{
							Exponent += Prefixes.PrefixToExponent(Units.Key) * FactorExponent;

							foreach (KeyValuePair<AtomicUnit, int> Segment in Units.Value)
							{
								if (referenceUnits.ContainsKey(Name = Segment.Key.Name))
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								else if (baseUnits.TryGetValue(Name, out BaseQuantity))
								{
									if (BaseQuantity.FromReferenceUnit(ref Magnitude, ref NrDecimals, Name, Segment.Value * FactorExponent))
										this.Add(ReferenceFactors, BaseQuantity.ReferenceUnit, Segment.Value * FactorExponent);
									else
										this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								}
								else
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
							}
						}
						else
							this.Add(ReferenceFactors, Factor.Key, Factor.Value);
					}

					Unit Result = new Unit(Prefixes.ExponentToPrefix(Exponent, out FactorExponent), ReferenceFactors);
					if (FactorExponent != 0)
					{
						Magnitude *= Math.Pow(10, FactorExponent);
						NrDecimals -= FactorExponent;
					}

					Result.hasBaseUnits = true;
					Result.hasReferenceUnits = true;

					return Result;
				}
				else
				{
					this.hasBaseUnits = true;
					this.hasReferenceUnits = true;
					return this;
				}
			}
		}

		private static void Search()
		{
			Dictionary<string, IBaseQuantity> BaseUnits = new Dictionary<string, IBaseQuantity>();
			Dictionary<string, IBaseQuantity> ReferenceUnits = new Dictionary<string, IBaseQuantity>();
			Dictionary<string, KeyValuePair<Prefix, KeyValuePair<AtomicUnit, int>[]>> CompoundUnits = new Dictionary<string, KeyValuePair<Prefix, KeyValuePair<AtomicUnit, int>[]>>();
			Dictionary<string, PhysicalQuantity> DerivedUnits = new Dictionary<string, PhysicalQuantity>();
			IBaseQuantity BaseQuantity;
			ICompoundQuantity CompoundQuantity;
			IDerivedQuantity DerivedQuantity;

			foreach (Type Type in Types.GetTypesImplementingInterface(typeof(IBaseQuantity)))
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(Type);
				if (CI is null)
					continue;

				try
				{
					BaseQuantity = (IBaseQuantity)CI.Invoke(Types.NoParameters);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					continue;
				}

				ReferenceUnits[BaseQuantity.ReferenceUnit.Name] = BaseQuantity;

				foreach (string Unit in BaseQuantity.BaseUnits)
					BaseUnits[Unit] = BaseQuantity;
			}

			foreach (Type Type in Types.GetTypesImplementingInterface(typeof(ICompoundQuantity)))
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(Type);
				if (CI is null)
					continue;

				try
				{
					CompoundQuantity = (ICompoundQuantity)CI.Invoke(Types.NoParameters);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					continue;
				}

				foreach (Tuple<string, Prefix, KeyValuePair<AtomicUnit, int>[]> CompoundUnit in CompoundQuantity.CompoundQuantities)
					CompoundUnits[CompoundUnit.Item1] = new KeyValuePair<Prefix, KeyValuePair<AtomicUnit, int>[]>(CompoundUnit.Item2, CompoundUnit.Item3);
			}

			foreach (Type Type in Types.GetTypesImplementingInterface(typeof(IDerivedQuantity)))
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(Type);
				if (CI is null)
					continue;

				try
				{
					DerivedQuantity = (IDerivedQuantity)CI.Invoke(Types.NoParameters);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					continue;
				}

				foreach (KeyValuePair<string, PhysicalQuantity> Derived in DerivedQuantity.DerivedUnits)
					DerivedUnits[Derived.Key] = Derived.Value;
			}

			baseUnits = BaseUnits;
			referenceUnits = ReferenceUnits;
			compoundUnits = CompoundUnits;
			derivedUnits = DerivedUnits;
		}

		private readonly static Dictionary<string, IUnitCategory> categoryPerUnit = new Dictionary<string, IUnitCategory>();
		private static Dictionary<string, IBaseQuantity> baseUnits = null;
		private static Dictionary<string, IBaseQuantity> referenceUnits = null;
		private static Dictionary<string, KeyValuePair<Prefix, KeyValuePair<AtomicUnit, int>[]>> compoundUnits = null;
		private static Dictionary<string, PhysicalQuantity> derivedUnits = null;
		private static IUnitCategory[] unitCategories = null;
		private static readonly object synchObject = new object();

		static Unit()
		{
			Types.OnInvalidated += Types_OnInvalidated;
		}

		private static void Types_OnInvalidated(object Sender, EventArgs e)
		{
			lock (synchObject)
			{
				baseUnits = null;
				referenceUnits = null;
				compoundUnits = null;
				derivedUnits = null;
				unitCategories = null;
				categoryPerUnit.Clear();
			}
		}

		/// <summary>
		/// Tries to get the factors of a compound unit.
		/// </summary>
		/// <param name="Name">Name of compoud unit.</param>
		/// <param name="Factors">Factors in unit.</param>
		/// <returns>If a compound unit with the given name was found.</returns>
		internal static bool TryGetCompoundUnit(string Name, out KeyValuePair<Prefix, KeyValuePair<AtomicUnit, int>[]> Factors)
		{
			lock (synchObject)
			{
				if (compoundUnits is null)
					Search();

				return compoundUnits.TryGetValue(Name, out Factors);
			}
		}

		/// <summary>
		/// If there's a derived unit with a given name.
		/// </summary>
		/// <param name="Name">Name of derived unit.</param>
		/// <returns>If such a derived unit is recognized.</returns>
		internal static bool ContainsDerivedOrBaseUnit(string Name)
		{
			lock (synchObject)
			{
				if (baseUnits is null)
					Search();

				return baseUnits.ContainsKey(Name) || derivedUnits.ContainsKey(Name);
			}
		}

		/// <summary>
		/// Tries to convert a magnitude in one unit to a magnitude in another.
		/// </summary>
		/// <param name="From">Original magnitude.</param>
		/// <param name="FromUnit">Original unit.</param>
		/// <param name="ToUnit">Desired unit.</param>
		/// <param name="To">Converted magnitude.</param>
		/// <returns>If conversion was successful.</returns>
		public static bool TryConvert(double From, Unit FromUnit, Unit ToUnit, out double To)
		{
			return TryConvert(From, FromUnit, 0, ToUnit, out To, out _);
		}

		/// <summary>
		/// Tries to convert a magnitude in one unit to a magnitude in another.
		/// </summary>
		/// <param name="From">Original magnitude.</param>
		/// <param name="FromUnit">Original unit.</param>
		/// <param name="FromNrDec">Number of decimals used for <paramref name="From"/>.</param>
		/// <param name="ToUnit">Desired unit.</param>
		/// <param name="To">Converted magnitude.</param>
		/// <param name="ToNrDec">Number of decimals used for <paramref name="To"/>.</param>
		/// <returns>If conversion was successful.</returns>
		public static bool TryConvert(double From, Unit FromUnit, byte FromNrDec, Unit ToUnit, out double To, out byte ToNrDec)
		{
			int ToNrDec2;

			if (FromUnit.Equals(ToUnit, false))
			{
				To = From;
				ToNrDec = FromNrDec;

				if (FromUnit.prefix != ToUnit.prefix)
				{
					int ExponentDiff = Prefixes.PrefixToExponent(FromUnit.prefix);
					ExponentDiff -= Prefixes.PrefixToExponent(ToUnit.prefix);

					if (ExponentDiff != 0)
					{
						To *= Math.Pow(10, ExponentDiff);
						ToNrDec2 = ToNrDec - ExponentDiff;
						if (ToNrDec2 < 0)
							ToNrDec = 0;
						else if (ToNrDec2 > 255)
							ToNrDec = 255;
						else
							ToNrDec = (byte)ToNrDec2;
					}
				}

				return true;
			}

			double NrDec = FromNrDec;

			FromUnit = FromUnit.ToReferenceUnits(ref From, ref NrDec);
			To = From;
			ToUnit = ToUnit.FromReferenceUnits(ref To, ref NrDec);
			ToNrDec2 = (int)Math.Round(NrDec);

			Unit Div = Divide(FromUnit, ToUnit, out int Exponent);
			Exponent += Prefixes.PrefixToExponent(Div.prefix);
			if (Exponent != 0)
			{
				To *= Math.Pow(10, Exponent);
				ToNrDec2 -= Exponent;
			}

			if (ToNrDec2 < 0)
				ToNrDec = 0;
			else if (ToNrDec2 > 255)
				ToNrDec = 255;
			else
				ToNrDec = (byte)ToNrDec2;

			return !Div.HasFactors;
		}

		/// <summary>
		/// Tries to get the unit category of a unit.
		/// </summary>
		/// <param name="Unit">Unit</param>
		/// <param name="Category">Unit category.</param>
		/// <returns>If a unit category was found matching the unit.</returns>
		public static bool TryGetCategory(Unit Unit, out IUnitCategory Category)
		{
			string s = Unit.ToString();

			lock (synchObject)
			{
				if (categoryPerUnit.TryGetValue(s, out Category))
					return !(Category is null);

				if (unitCategories is null)
				{
					List<IUnitCategory> Categories = new List<IUnitCategory>();

					foreach (Type Type in Types.GetTypesImplementingInterface(typeof(IUnitCategory)))
					{
						ConstructorInfo CI = Types.GetDefaultConstructor(Type);
						if (CI is null)
							continue;

						try
						{
							Category = (IUnitCategory)CI.Invoke(Types.NoParameters);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
							continue;
						}

						Categories.Add(Category);
					}

					unitCategories = Categories.ToArray();
				}
			}

			Category = null;

			foreach (IUnitCategory Category2 in unitCategories)
			{
				if (TryConvert(1, Unit, Category2.Reference, out double _))
				{
					Category = Category2;
					break;
				}
			}

			lock (synchObject)
			{
				categoryPerUnit[s] = Category;
			}

			return !(Category is null);
		}

	}
}
