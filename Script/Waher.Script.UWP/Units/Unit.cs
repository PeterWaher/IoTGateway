using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
using Waher.Script.Objects;

namespace Waher.Script.Units
{
	/// <summary>
	/// Represents a unit.
	/// </summary>
	public sealed class Unit
	{
		private Prefix prefix;
		private ICollection<KeyValuePair<AtomicUnit, int>> factors;
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

			LinkedList<KeyValuePair<AtomicUnit, int>> Factors = new LinkedList<KeyValuePair<Units.AtomicUnit, int>>();

			foreach (string s in AtomicUnits)
				Factors.AddLast(new KeyValuePair<Units.AtomicUnit, int>(new AtomicUnit(s), 1));

			this.factors = Factors;
		}

		/// <summary>
		/// Associated prefix.
		/// </summary>
		public Prefix Prefix
		{
			get { return this.prefix; }
		}

		/// <summary>
		/// Sequence of atomic unit factors, and their corresponding exponents.
		/// </summary>
		public ICollection<KeyValuePair<AtomicUnit, int>> Factors
		{
			get { return this.factors; }
		}

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

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			Unit U = obj as Unit;
			if (U == null)
				return false;

			if (this.prefix != U.prefix || this.factors.Count != U.factors.Count)
				return false;

			string Name;
			bool Found;

			foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
			{
				Name = Factor.Key.Name;
				Found = false;

				foreach (KeyValuePair<AtomicUnit, int> Factor2 in U.factors)
				{
					if (Name == Factor2.Key.Name)
					{
						if (Factor2.Value != Factor2.Value)
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

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			int i = (int)this.prefix;

			foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
				i ^= Factor.Key.GetHashCode() ^ Factor.Value.GetHashCode();

			return i;
		}

		/// <summary>
		/// <see cref="Object.ToString"/>
		/// </summary>
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
					if (Numerator == null)
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
					if (Denominator == null)
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

			if (Numerator == null)
			{
				if (IncludePrefix)
					Numerator = new StringBuilder(Prefixes.ToString(this.prefix));
				else
					Numerator = new StringBuilder();
			}

			if (Denominator != null)
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

				while (Loop != null && !Found)
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

				while (Loop != null && !Found)
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
				if (baseUnits == null)
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
					KeyValuePair<AtomicUnit, int>[] Units;
					PhysicalQuantity Quantity;
					int Exponent = Prefixes.PrefixToExponent(this.prefix);
					int FactorExponent;
					string Name;

					foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
					{
						FactorExponent = Factor.Value;

						if (baseUnits.ContainsKey(Name = Factor.Key.Name))
							this.Add(BaseFactors, Factor.Key, Factor.Value);
						else if (derivedUnits.TryGetValue(Name, out Quantity))
						{
							Magnitude *= Math.Pow(Quantity.Magnitude, FactorExponent);
							Exponent += Prefixes.PrefixToExponent(Quantity.Unit.prefix) * FactorExponent;

							foreach (KeyValuePair<AtomicUnit, int> Segment in Quantity.Unit.factors)
								this.Add(BaseFactors, Segment.Key, Segment.Value * FactorExponent);
						}
						else if (compoundUnits.TryGetValue(Name, out Units))
						{
							foreach (KeyValuePair<AtomicUnit, int> Segment in Units)
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

			while (Loop != null && Loop.Value.Key.Name != Name)
				Loop = Loop.Next;

			if (Loop == null)
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
			if (this.hasReferenceUnits)
				return this;

			lock (synchObject)
			{
				if (baseUnits == null)
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
					KeyValuePair<AtomicUnit, int>[] Units;
					IBaseQuantity BaseQuantity;
					PhysicalQuantity Quantity;
					int Exponent = Prefixes.PrefixToExponent(this.prefix);
					int FactorExponent;
					string Name;

					foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
					{
						FactorExponent = Factor.Value;

						if (referenceUnits.ContainsKey(Name = Factor.Key.Name))
							this.Add(ReferenceFactors, Factor.Key, Factor.Value);
						else if (baseUnits.TryGetValue(Name, out BaseQuantity))
						{
							if (BaseQuantity.ToReferenceUnit(ref Magnitude, Name, FactorExponent))
								this.Add(ReferenceFactors, BaseQuantity.ReferenceUnit, FactorExponent);
							else
								this.Add(ReferenceFactors, Factor.Key, Factor.Value);
						}
						else if (derivedUnits.TryGetValue(Name, out Quantity))
						{
							Magnitude *= Math.Pow(Quantity.Magnitude, FactorExponent);
							Exponent += Prefixes.PrefixToExponent(Quantity.Unit.prefix) * FactorExponent;

							foreach (KeyValuePair<AtomicUnit, int> Segment in Quantity.Unit.factors)
							{
								if (referenceUnits.ContainsKey(Name = Segment.Key.Name))
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								else if (baseUnits.TryGetValue(Name, out BaseQuantity))
								{
									if (BaseQuantity.ToReferenceUnit(ref Magnitude, Name, Segment.Value * FactorExponent))
										this.Add(ReferenceFactors, BaseQuantity.ReferenceUnit, Segment.Value * FactorExponent);
									else
										this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								}
								else
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
							}
						}
						else if (compoundUnits.TryGetValue(Name, out Units))
						{
							foreach (KeyValuePair<AtomicUnit, int> Segment in Units)
							{
								if (referenceUnits.ContainsKey(Name = Segment.Key.Name))
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								else if (baseUnits.TryGetValue(Name, out BaseQuantity))
								{
									if (BaseQuantity.ToReferenceUnit(ref Magnitude, Name, Segment.Value * FactorExponent))
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
						Magnitude *= Math.Pow(10, FactorExponent);

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
			if (this.hasReferenceUnits)
				return this;

			lock (synchObject)
			{
				if (baseUnits == null)
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
					KeyValuePair<AtomicUnit, int>[] Units;
					IBaseQuantity BaseQuantity;
					PhysicalQuantity Quantity;
					int Exponent = Prefixes.PrefixToExponent(this.prefix);
					int FactorExponent;
					string Name;

					foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
					{
						FactorExponent = Factor.Value;

						if (referenceUnits.ContainsKey(Name = Factor.Key.Name))
							this.Add(ReferenceFactors, Factor.Key, Factor.Value);
						else if (baseUnits.TryGetValue(Name, out BaseQuantity))
						{
							if (BaseQuantity.FromReferenceUnit(ref Magnitude, Name, FactorExponent))
								this.Add(ReferenceFactors, BaseQuantity.ReferenceUnit, FactorExponent);
							else
								this.Add(ReferenceFactors, Factor.Key, Factor.Value);
						}
						else if (derivedUnits.TryGetValue(Name, out Quantity))
						{
							Magnitude *= Math.Pow(Quantity.Magnitude, FactorExponent);
							Exponent += Prefixes.PrefixToExponent(Quantity.Unit.prefix) * FactorExponent;

							foreach (KeyValuePair<AtomicUnit, int> Segment in Quantity.Unit.factors)
							{
								if (referenceUnits.ContainsKey(Name = Segment.Key.Name))
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								else if (baseUnits.TryGetValue(Name, out BaseQuantity))
								{
									if (BaseQuantity.FromReferenceUnit(ref Magnitude, Name, Segment.Value * FactorExponent))
										this.Add(ReferenceFactors, BaseQuantity.ReferenceUnit, Segment.Value * FactorExponent);
									else
										this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								}
								else
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
							}
						}
						else if (compoundUnits.TryGetValue(Name, out Units))
						{
							foreach (KeyValuePair<AtomicUnit, int> Segment in Units)
							{
								if (referenceUnits.ContainsKey(Name = Segment.Key.Name))
									this.Add(ReferenceFactors, Segment.Key, Segment.Value * FactorExponent);
								else if (baseUnits.TryGetValue(Name, out BaseQuantity))
								{
									if (BaseQuantity.FromReferenceUnit(ref Magnitude, Name, Segment.Value * FactorExponent))
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
						Magnitude *= Math.Pow(10, FactorExponent);

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
			Dictionary<string, KeyValuePair<AtomicUnit, int>[]> CompoundUnits = new Dictionary<string, KeyValuePair<AtomicUnit, int>[]>();
			Dictionary<string, PhysicalQuantity> DerivedUnits = new Dictionary<string, PhysicalQuantity>();
			ConstructorInfo CI;
			IBaseQuantity BaseQuantity;
			ICompoundQuantity CompoundQuantity;
			IDerivedQuantity DerivedQuantity;

			foreach (Type Type in Types.GetTypesImplementingInterface(typeof(IBaseQuantity)))
			{
				CI = Type.GetConstructor(Types.NoTypes);
				if (CI == null)
					continue;

				try
				{
					BaseQuantity = (IBaseQuantity)CI.Invoke(Types.NoParameters);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					continue;
				}

				ReferenceUnits[BaseQuantity.ReferenceUnit.Name] = BaseQuantity;

				foreach (string Unit in BaseQuantity.BaseUnits)
					BaseUnits[Unit] = BaseQuantity;
			}

			foreach (Type Type in Types.GetTypesImplementingInterface(typeof(ICompoundQuantity)))
			{
				CI = Type.GetConstructor(Types.NoTypes);
				if (CI == null)
					continue;

				try
				{
					CompoundQuantity = (ICompoundQuantity)CI.Invoke(Types.NoParameters);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					continue;
				}

				foreach (KeyValuePair<string, KeyValuePair<AtomicUnit, int>[]> CompountUnit in CompoundQuantity.CompoundQuantities)
					CompoundUnits[CompountUnit.Key] = CompountUnit.Value;
			}

			foreach (Type Type in Types.GetTypesImplementingInterface(typeof(IDerivedQuantity)))
			{
				CI = Type.GetConstructor(Types.NoTypes);
				if (CI == null)
					continue;

				try
				{
					DerivedQuantity = (IDerivedQuantity)CI.Invoke(Types.NoParameters);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
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

		private static Dictionary<string, IBaseQuantity> baseUnits = null;
		private static Dictionary<string, IBaseQuantity> referenceUnits = null;
		private static Dictionary<string, KeyValuePair<AtomicUnit, int>[]> compoundUnits = null;
		private static Dictionary<string, PhysicalQuantity> derivedUnits = null;
		private static object synchObject = new object();

		static Unit()
		{
			Types.OnInvalidated += Types_OnInvalidated;
		}

		private static void Types_OnInvalidated(object sender, EventArgs e)
		{
			lock (synchObject)
			{
				baseUnits = null;
				referenceUnits = null;
				compoundUnits = null;
				derivedUnits = null;
			}
		}

		/// <summary>
		/// Tries to get the factors of a compound unit.
		/// </summary>
		/// <param name="Name">Name of compoud unit.</param>
		/// <param name="Factors">Factors in unit.</param>
		/// <returns>If a compound unit with the given name was found.</returns>
		internal static bool TryGetCompoundUnit(string Name, out KeyValuePair<AtomicUnit, int>[] Factors)
		{
			lock (synchObject)
			{
				if (compoundUnits == null)
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
				if (baseUnits == null)
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
			if (FromUnit.Equals(ToUnit))
			{
				To = From;
				return true;
			}

			FromUnit = FromUnit.ToReferenceUnits(ref From);
			To = From;
			ToUnit = ToUnit.FromReferenceUnits(ref To);

			int Exponent;
			Unit Div = Unit.Divide(FromUnit, ToUnit, out Exponent);
			Exponent += Prefixes.PrefixToExponent(Div.prefix);
			if (Exponent != 0)
				To *= Math.Pow(10, Exponent);

			return !Div.HasFactors;
		}

	}
}
