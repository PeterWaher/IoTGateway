using System;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.Sets;

namespace Waher.Script.Operators.Sets
{
	/// <summary>
	/// Defines a set, by implicitly limiting its members to members of an optional superset, matching given conditions.
	/// </summary>
	public class ImplicitSetDefinition : BinaryOperator
	{
		private readonly ScriptNode[] conditions;
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
			this.conditions = Conditions;
			this.doubleColon = DoubleColon;
		}

		public override IElement Evaluate(Variables Variables)
		{
			ISet SuperSet;

			if (this.right is null)
				SuperSet = null;
			else
			{
				IElement E = this.right.Evaluate(Variables);
				SuperSet = E.AssociatedObjectValue as ISet;
				if (SuperSet is null)
					throw new ScriptRuntimeException("Superset did not evaluate to a set.", this);
			}

			return new ImplicitSet(this.left, SuperSet, this.conditions, Variables, this.doubleColon);
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is ImplicitSetDefinition O &&
				this.doubleColon.Equals(O.doubleColon) &&
				AreEqual(this.conditions, O.conditions) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.doubleColon.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.conditions);
			return Result;
		}
	}
}
