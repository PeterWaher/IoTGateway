using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Operators.Membership;

namespace Waher.Script.Objects.Sets
{
	/// <summary>
	/// Represents an implicitly defined set
	/// </summary>
	public class ImplicitSet : Set
	{
		private readonly ScriptNode pattern;
		private readonly ISet superSet;
		private readonly ScriptNode[] conditions;
		private readonly In[] finitSetConditions;
		private readonly Variables variables = null;
		private FiniteSet finiteSet = null;
		private bool doubleColon;

		/// <summary>
		/// Represents an implicitly defined set
		/// </summary>
		/// <param name="Pattern">Pattern of elements.</param>
		/// <param name="SuperSet">Optional superset.</param>
		/// <param name="Conditions">Condition subset members must fulfill.</param><
		/// <param name="Variables">Refernce to variables.</param>
		/// <param name="DoubleColon">If double-colon was used to create subset.</param>
		public ImplicitSet(ScriptNode Pattern, ISet SuperSet, ScriptNode[] Conditions, Variables Variables, bool DoubleColon)
		{
			this.pattern = Pattern;
			this.superSet = SuperSet;
			this.doubleColon = DoubleColon;
			this.variables = new Variables();

			List<In> FinitSetConditions = null;
			List<ScriptNode> OtherConditions = null;
			int i, j, c = Conditions.Length;

			for (i = 0; i < c; i++)
			{
				ScriptNode Condition = Conditions[i];
				if (Condition is In In)
				{
					if (FinitSetConditions == null)
					{
						FinitSetConditions = new List<In>();

						if (i > 0)
						{
							OtherConditions = new List<ScriptNode>();

							for (j = 0; j < i; j++)
								OtherConditions.Add(Conditions[j]);
						}
					}

					FinitSetConditions.Add(In);
				}
				else if (FinitSetConditions != null)
				{
					if (OtherConditions == null)
						OtherConditions = new List<ScriptNode>();

					OtherConditions.Add(Condition);
				}
			}

			if (FinitSetConditions != null)
			{
				this.conditions = OtherConditions?.ToArray();
				this.finitSetConditions = FinitSetConditions.ToArray();
			}
			else
			{
				this.conditions = Conditions;
				this.finitSetConditions = null;
			}

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

				switch (this.pattern.PatternMatch(Element, Variables))
				{
					case PatternMatchResult.Match:
						this.variables.Push();
						try
						{

							foreach (KeyValuePair<string, IElement> P in Variables)
								this.variables[P.Key] = P.Value;

							return this.SatisfiesConditions();
						}
						finally
						{
							this.variables.Pop();
						}

					case PatternMatchResult.NoMatch:
						return false;

					case PatternMatchResult.Unknown:
					default:
						throw new ScriptRuntimeException("Unable to compute pattern match.", this.pattern);
				}
			}
		}

		private bool SatisfiesConditions()
		{
			if (this.finitSetConditions != null)
			{
				foreach (ScriptNode Condition in this.finitSetConditions)
				{
					if (!this.SatisfiesCondition(Condition))
						return false;
				}
			}

			if (this.conditions != null)
			{
				foreach (ScriptNode Condition in this.conditions)
				{
					if (!this.SatisfiesCondition(Condition))
						return false;
				}
			}

			return true;
		}

		private bool SatisfiesCondition(ScriptNode Condition)
		{
			IElement Result = Condition.Evaluate(this.variables);

			if (Result is BooleanValue B)
				return B.Value;
			else if (Expression.TryConvert<bool>(Result.AssociatedObjectValue, out bool b))
				return b;
			else
				return false;
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
					Variables.Clear();
					switch (this.pattern.PatternMatch(Element, Variables))
					{
						case PatternMatchResult.Match:
							foreach (KeyValuePair<string, IElement> P in Variables)
								this.variables[P.Key] = P.Value;

							try
							{
								if (!this.SatisfiesConditions())
									continue;
							}
							catch (Exception)
							{
								continue;
							}

							break;

						case PatternMatchResult.NoMatch:
							continue;

						case PatternMatchResult.Unknown:
						default:
							return false;
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
				ScriptNode.AreEqual(this.conditions, S.conditions);
		}

		public override int GetHashCode()
		{
			int Result = this.pattern.GetHashCode();

			if (this.superSet != null)
				Result ^= Result << 5 ^ this.superSet.GetHashCode();

			Result ^= Result << 5 ^ ScriptNode.GetHashCode(this.conditions);
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

			bool First = true;

			foreach (ScriptNode Condition in this.conditions)
			{
				if (First)
					First = false;
				else
					sb.Append(',');

				sb.Append(Condition.SubExpression);
			}

			sb.Append('}');

			return sb.ToString();
		}
	}
}
