using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.Sets;
using Waher.Script.Operators.Membership;

namespace Waher.Script.Operators.Sets
{
	/// <summary>
	/// Defines a set, by implicitly limiting its members to members of an optional superset, matching given conditions.
	/// </summary>
	public class ImplicitSetDefinition : BinaryOperator
	{
		private readonly In[] setConditions;
		private readonly ScriptNode[] otherConditions;
		private readonly bool doubleColon;

		/// <summary>
		/// Defines a set, by implicitly limiting its members to members of an optional superset, matching given conditions.
		/// </summary>
		/// <param name="Pattern">Pattern defining elements in the set.</param>
		/// <param name="SuperSet">Optional superset, if defining subset.</param>
		/// <param name="Conditions">Conditions of elements in the set.</param>
		/// <param name="DoubleColon">If double-colon was used to define the set.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ImplicitSetDefinition(ScriptNode Pattern, ScriptNode SuperSet, ScriptNode[] Conditions, bool DoubleColon,
			int Start, int Length, Expression Expression)
			: base(Pattern, SuperSet, Start, Length, Expression)
		{
			this.doubleColon = DoubleColon;

			SeparateConditions(Conditions, out this.setConditions, out this.otherConditions);
		}

		/// <summary>
		/// Separates conditions into set membership conditions and other types of conditions.
		/// </summary>
		/// <param name="Conditions">Conditions</param>
		/// <param name="SetConditions">Set membership conditions. Can be set to null, if none found.</param>
		/// <param name="OtherConditions">Other conditions</param>
		public static void SeparateConditions(ScriptNode[] Conditions, out In[] SetConditions, out ScriptNode[] OtherConditions)
		{
			List<In> SetConditionList = null;
			List<ScriptNode> OtherConditionList = null;
			int i, j, c = Conditions.Length;

			for (i = 0; i < c; i++)
			{
				ScriptNode Condition = Conditions[i];
				if (Condition is In In)
				{
					if (SetConditionList is null)
					{
						SetConditionList = new List<In>();

						if (i > 0)
						{
							OtherConditionList = new List<ScriptNode>();

							for (j = 0; j < i; j++)
								OtherConditionList.Add(Conditions[j]);
						}
					}

					SetConditionList.Add(In);
				}
				else if (!(SetConditionList is null))
				{
					if (OtherConditionList is null)
						OtherConditionList = new List<ScriptNode>();

					OtherConditionList.Add(Condition);
				}
			}

			if (!(SetConditionList is null))
			{
				OtherConditions = OtherConditionList?.ToArray();
				SetConditions = SetConditionList.ToArray();
			}
			else
			{
				OtherConditions = Conditions;
				SetConditions = null;
			}
		}

		public override IElement Evaluate(Variables Variables)
		{
			ISet SuperSet;

			if (this.right is null)
				SuperSet = null;
			else
			{
				IElement E = this.right.Evaluate(Variables);
				SuperSet = Set.ToSet(E);
				if (SuperSet == null)
					throw new ScriptRuntimeException("Unable to evaluate superset into a set.", this.right);
			}

			return new ImplicitSet(this.left, SuperSet, this.setConditions, this.otherConditions, Variables, this.doubleColon);
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is ImplicitSetDefinition O &&
				this.doubleColon.Equals(O.doubleColon) &&
				AreEqual(this.setConditions, O.setConditions) &&
				AreEqual(this.otherConditions, O.otherConditions) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.doubleColon.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.setConditions);
			Result ^= Result << 5 ^ GetHashCode(this.otherConditions);
			return Result;
		}
	}
}
