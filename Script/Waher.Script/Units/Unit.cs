using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Units
{
	/// <summary>
	/// Represents a unit.
	/// </summary>
	public sealed class Unit
	{
		private Prefix prefix;
		private ICollection<KeyValuePair<AtomicUnit, int>> factors;

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
		/// If the unit is empty.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return this.prefix == Prefix.None && this.factors.Count == 0;
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
			StringBuilder Numerator = null;
			StringBuilder Denominator = null;
			int NrDenominators = 0;

			foreach (KeyValuePair<AtomicUnit, int> Factor in this.factors)
			{
				if (Factor.Value > 0)
				{
					if (Numerator == null)
						Numerator = new StringBuilder(Prefixes.ToString(this.prefix));
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
				Numerator = new StringBuilder(Prefixes.ToString(this.prefix));

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

		public static bool TryConvert(double From, Unit FromUnit, Unit ToUnit, out double To)
		{
			To = From;

			if (FromUnit.Equals(ToUnit))
				return true;

			int Exponent;
			Unit Div = Unit.Divide(FromUnit, ToUnit, out Exponent);
			Exponent += Prefixes.PrefixToExponent(Div.prefix);
			if (Exponent != 0)
				To *= Math.Pow(10, Exponent);


			foreach (KeyValuePair<AtomicUnit, int> Factor in Div.factors)
			{
				return false;
			}

			return true;

			// TODO: Convert
			// TODO: Base quantities
			// TODO: Handle unit shorts (Example: Wh = W*h)
			// TODO: Handle unit confusions: ft != femto ton
		}

	}
}
