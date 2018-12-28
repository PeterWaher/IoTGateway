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
		private readonly ScriptNode[] otherConditions;
		private readonly In[] setConditions;
		private readonly Variables variables = null;
		private FiniteSet finiteSet = null;
		private bool doubleColon;

		/// <summary>
		/// Represents an implicitly defined set
		/// </summary>
		/// <param name="Pattern">Pattern of elements.</param>
		/// <param name="SuperSet">Optional superset.</param>
		/// <param name="SetConditions">Set membership conditions that must be fulfulled.</param>
		/// <param name="OtherConditions">Other condition subset members must fulfill.</param>
		/// <param name="Variables">Refernce to variables.</param>
		/// <param name="DoubleColon">If double-colon was used to create subset.</param>
		public ImplicitSet(ScriptNode Pattern, ISet SuperSet, In[] SetConditions, ScriptNode[] OtherConditions, 
			Variables Variables, bool DoubleColon)
		{
			this.pattern = Pattern;
			this.superSet = SuperSet;
			this.doubleColon = DoubleColon;
			this.setConditions = SetConditions;
			this.otherConditions = OtherConditions;

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
			if (!(this.finiteSet is null))
				return this.finiteSet.Contains(Element);
			else
			{
				if (!(this.superSet is null) && !this.superSet.Contains(Element))
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

							return SatisfiesConditions(this.setConditions, this.otherConditions, this.variables);
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

		private static bool SatisfiesConditions(ScriptNode[] SetConditions, ScriptNode[] OtherConditions, Variables Variables)
		{
			if (!(SetConditions is null))
			{
				foreach (ScriptNode Condition in SetConditions)
				{
					if (!SatisfiesCondition(Condition, Variables))
						return false;
				}
			}

			if (!(OtherConditions is null))
			{
				foreach (ScriptNode Condition in OtherConditions)
				{
					if (!SatisfiesCondition(Condition, Variables))
						return false;
				}
			}

			return true;
		}

		private static bool SatisfiesCondition(ScriptNode Condition, Variables Variables)
		{
			IElement Result = Condition.Evaluate(Variables);

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
				if (!(this.finiteSet is null))
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
				if (!(this.finiteSet is null))
					return this.finiteSet.Size;

				if (!this.CalcSubset())
					return null;

				return this.finiteSet.Size;
			}
		}

		private bool CalcSubset()
		{
			IEnumerable<IElement> Elements;

			if (!(this.superSet is null) && this.superSet.Size.HasValue)
				Elements = CalculateElements(this.pattern, this.superSet.ChildElements, this.setConditions, this.otherConditions, this.variables);
			else
				Elements = CalculateElements(this.pattern, null, this.setConditions, this.otherConditions, this.variables);

			if (Elements is null)
				return false;
			else
			{
				this.finiteSet = new FiniteSet(Elements);
				return true;
			}
		}

		/// <summary>
		/// Calculates elements specified using implicit notation.
		/// </summary>
		/// <param name="Pattern">Pattern forming elements.</param>
		/// <param name="SuperSetElements">Optional super-set of elements. Can be null.</param>
		/// <param name="SetConditions">Set membership conditions that need to be fulfilled.</param>
		/// <param name="OtherConditions">Other conditions that need to be fulfilled.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Enumerable set of elements, or null if not able to calculate element set.</returns>
		public static IEnumerable<IElement> CalculateElements(ScriptNode Pattern, IEnumerable<IElement> SuperSetElements,
			In[] SetConditions, ScriptNode[] OtherConditions, Variables Variables)
		{
			if (!(SuperSetElements is null))
			{
				Dictionary<string, IElement> LocalVariables = new Dictionary<string, IElement>();
				LinkedList<IElement> Items = new LinkedList<IElement>();

				Variables.Push();
				try
				{
					foreach (IElement Element in SuperSetElements)
					{
						LocalVariables.Clear();
						switch (Pattern.PatternMatch(Element, LocalVariables))
						{
							case PatternMatchResult.Match:
								foreach (KeyValuePair<string, IElement> P in LocalVariables)
									Variables[P.Key] = P.Value;

								try
								{
									if (!SatisfiesConditions(SetConditions, OtherConditions, Variables))
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
								return null;
						}

						Items.AddLast(Element);
					}
				}
				finally
				{
					Variables.Pop();
				}

				return Items;
			}
			else if (SuperSetElements is null && !(SetConditions is null))
			{
				int i, c = SetConditions.Length;
				IEnumerator<IElement>[] Enumerators = new IEnumerator<IElement>[c];
				string[][] AffectedVariables = new string[c][];

				for (i = 0; i < c; i++)
				{
					IEnumerable<IElement> Members = GetSetMembers(SetConditions[i].RightOperand.Evaluate(Variables));
					if (Members is null)
						return null;

					Enumerators[i] = Members.GetEnumerator();
					if (!Enumerators[i].MoveNext())
						return null;
				}

				Variables.Push();
				try
				{
					LinkedList<IElement> Items = new LinkedList<IElement>();
					Dictionary<string, IElement> LocalVariables = new Dictionary<string, IElement>();
					IEnumerator<IElement> e;
					bool Collision = false;
					bool Match;
					int j = c - 1;

					do
					{
						if (Collision)
							LocalVariables.Clear();
						else
						{
							for (i = 0; i <= j; i++)
							{
								if (!(AffectedVariables[i] is null))
								{
									foreach (string s in AffectedVariables[i])
										LocalVariables.Remove(s);
								}
							}
						}

						Match = true;

						for (i = 0; Match && (i <= j || (Collision && i < c)); i++)
						{
							e = Enumerators[i];

							if (AffectedVariables[i] is null)
							{
								Dictionary<string, IElement> v = new Dictionary<string, IElement>();

								switch (SetConditions[i].LeftOperand.PatternMatch(e.Current, v))
								{
									case PatternMatchResult.Match:
										string[] v2 = new string[v.Count];
										v.Keys.CopyTo(v2, 0);
										AffectedVariables[i] = v2;

										foreach (KeyValuePair<string, IElement> P in v)
										{
											if (LocalVariables.TryGetValue(P.Key, out IElement E))
											{
												Collision = true;

												if (!e.Current.Equals(E))
													Match = false;
											}
											else
												LocalVariables[P.Key] = P.Value;
										}
										break;

									case PatternMatchResult.NoMatch:
										Match = false;
										break;

									case PatternMatchResult.Unknown:
									default:
										return null;
								}
							}
							else
							{
								switch (SetConditions[i].LeftOperand.PatternMatch(e.Current, LocalVariables))
								{
									case PatternMatchResult.Match:
										break;

									case PatternMatchResult.NoMatch:
										Match = false;
										break;

									case PatternMatchResult.Unknown:
									default:
										return null;
								}
							}
						}

						if (Match)
						{
							foreach (KeyValuePair<string, IElement> P in LocalVariables)
								Variables[P.Key] = P.Value;

							if (SatisfiesConditions(null, OtherConditions, Variables))
								Items.AddLast(Pattern.Evaluate(Variables));
						}

						for (j = 0; j < c; j++)
						{
							e = Enumerators[j];
							if (e.MoveNext())
								break;

							e.Reset();
							e.MoveNext();
						}
					}
					while (j < c);

					return Items;
				}
				finally
				{
					Variables.Pop();
				}

			}
			else
				return null;
		}

		/// <summary>
		/// Gets the elements of a (supposed) set.
		/// </summary>
		/// <param name="E">Element</param>
		/// <returns>Elements, or null if not possible to get elements.</returns>
		public static IEnumerable<IElement> GetSetMembers(IElement E)
		{
			if (E is ISet Set)
			{
				if (!Set.Size.HasValue)
					return null;
				else
					return Set.ChildElements;
			}

			if (E is IVector Vector)
				return Vector.VectorElements;

			object Obj = E.AssociatedObjectValue;
			if (Obj is ISet Set2)
			{
				if (!Set2.Size.HasValue)
					return null;
				else
					return Set2.ChildElements;
			}

			if (Obj is IEnumerable<IElement> Elements)
				return Elements;

			if (Obj is IEnumerable<object> Objects)
			{
				LinkedList<IElement> List = new LinkedList<IElement>();

				foreach (object x in Objects)
					List.AddLast(Expression.Encapsulate(x));

				return List;
			}

			return null;
		}

		/// <summary>
		/// <see cref="object.Equals(object)"/>
		/// </summary>
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
				ScriptNode.AreEqual(this.otherConditions, S.otherConditions) &&
				ScriptNode.AreEqual(this.setConditions, S.setConditions);
		}

		/// <summary>
		/// <see cref="object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = this.pattern.GetHashCode();

			if (!(this.superSet is null))
				Result ^= Result << 5 ^ this.superSet.GetHashCode();

			Result ^= Result << 5 ^ ScriptNode.GetHashCode(this.otherConditions);
			Result ^= Result << 5 ^ ScriptNode.GetHashCode(this.setConditions);
			Result ^= Result << 5 ^ this.doubleColon.GetHashCode();

			return Result;
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			if (!(this.finiteSet is null))
				return this.finiteSet.ToString();

			StringBuilder sb = new StringBuilder();

			sb.Append('{');
			sb.Append(this.pattern.SubExpression);

			if (!(this.superSet is null))
			{
				sb.Append('∈');
				sb.Append(this.superSet.ToString());
			}

			sb.Append(':');

			if (this.doubleColon)
				sb.Append(':');

			bool First = true;

			if (!(this.setConditions is null))
			{
				foreach (ScriptNode Condition in this.setConditions)
				{
					if (First)
						First = false;
					else
						sb.Append(',');

					sb.Append(Condition.SubExpression);
				}
			}

			if (!(this.otherConditions is null))
			{
				foreach (ScriptNode Condition in this.otherConditions)
				{
					if (First)
						First = false;
					else
						sb.Append(',');

					sb.Append(Condition.SubExpression);
				}
			}

			sb.Append('}');

			return sb.ToString();
		}
	}
}
