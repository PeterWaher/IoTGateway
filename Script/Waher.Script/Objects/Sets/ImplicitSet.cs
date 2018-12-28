using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Objects.Sets
{
	/// <summary>
	/// Represents a subset
	/// </summary>
	public class ImplicitSet : Set
	{
		private readonly ScriptNode pattern;
		private readonly ISet superSet;
		private readonly ScriptNode condition;
		private readonly Variables variables = null;
		private FiniteSet finiteSet = null;
		private bool doubleColon;

		/// <summary>
		/// Represents a subset
		/// </summary>
		/// <param name="Pattern">Pattern of elements.</param>
		/// <param name="SuperSet">Optional superset.</param>
		/// <param name="Condition">Condition subset members must fulfill.</param><
		/// <param name="Variables">Refernce to variables.</param>
		/// <param name="DoubleColon">If double-colon was used to create subset.</param>
		public ImplicitSet(ScriptNode Pattern, ISet SuperSet, ScriptNode Condition, Variables Variables, bool DoubleColon)
		{
			this.pattern = Pattern;
			this.superSet = SuperSet;
			this.condition = Condition;
			this.doubleColon = DoubleColon;
			this.variables = new Variables();

			Variables.CopyTo(this.variables);
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			if (this.finiteSet != null)
				return this.finiteSet.Contains(Element);
			else
			{
				if (this.superSet != null && !this.superSet.Contains(Element))
					return false;

				Dictionary<string, IElement> Variables = new Dictionary<string, IElement>();

				try
				{
					this.pattern.PatternMatch(Element, Variables);

					this.variables.Push();
					try
					{

						foreach (KeyValuePair<string, IElement> P in Variables)
							this.variables[P.Key] = P.Value;

						IElement Result = this.condition.Evaluate(this.variables);

						if (Result is BooleanValue B)
							return B.Value;
						else if (Expression.TryConvert<bool>(Result.AssociatedObjectValue, out bool b))
							return b;
						else
							return false;
					}
					finally
					{
						this.variables.Pop();
					}
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		public override ICollection<IElement> ChildElements
		{
			get
			{
				if (this.finiteSet != null)
					return this.finiteSet.ChildElements;

				if (!this.CalcSubset())
					throw new ScriptException("Unable to calculate enumerable set.");

				return this.finiteSet.ChildElements;
			}
		}

		/// <summary>
		/// If the element represents a scalar value.
		/// </summary>
		public override int? Size
		{
			get
			{
				if (this.finiteSet != null)
					return this.finiteSet.Size;

				if (!this.CalcSubset())
					return null;

				return this.finiteSet.Size;
			}
		}

		private bool CalcSubset()
		{
			if (this.superSet == null || !this.superSet.Size.HasValue)
				return false;

			Dictionary<string, IElement> Variables = new Dictionary<string, IElement>();
			LinkedList<IElement> Items = new LinkedList<IElement>();

			this.variables.Push();
			try
			{
				foreach (IElement Element in this.superSet.ChildElements)
				{
					try
					{
						Variables.Clear();
						this.pattern.PatternMatch(Element, Variables);

						foreach (KeyValuePair<string, IElement> P in Variables)
							this.variables[P.Key] = P.Value;

						IElement Result = this.condition.Evaluate(this.variables);

						if (Result is BooleanValue B)
						{
							if (!B.Value)
								continue;
						}
						else if (Expression.TryConvert<bool>(Result.AssociatedObjectValue, out bool b))
						{
							if (!b)
								continue;
						}
						else
							continue;
					}
					catch (Exception)
					{
						continue;
					}

					Items.AddLast(Element);
				}
			}
			finally
			{
				this.variables.Pop();
			}

			this.finiteSet = new FiniteSet(Items);
			return true;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ImplicitSet S))
				return false;

			if (this.superSet is null ^ S.superSet is null)
				return false;

			if (!(this.superSet is null) && !this.superSet.Equals(S.superSet))
				return false;

			return
				this.pattern.Equals(S.pattern) &&
				this.doubleColon.Equals(S.doubleColon) &&
				this.condition.Equals(S.condition);
		}

		public override int GetHashCode()
		{
			int Result = this.pattern.GetHashCode();

			if (this.superSet != null)
				Result ^= Result << 5 ^ this.superSet.GetHashCode();

			Result ^= Result << 5 ^ this.condition.GetHashCode();
			Result ^= Result << 5 ^ this.doubleColon.GetHashCode();

			return Result;
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			if (this.finiteSet != null)
				return this.finiteSet.ToString();

			StringBuilder sb = new StringBuilder();

			sb.Append('{');
			sb.Append(this.pattern.SubExpression);

			if (this.superSet != null)
			{
				sb.Append('∈');
				sb.Append(this.superSet.ToString());
			}

			sb.Append(':');

			if (this.doubleColon)
				sb.Append(':');

			sb.Append(this.condition.SubExpression);
			sb.Append('}');

			return sb.ToString();
		}
	}
}
