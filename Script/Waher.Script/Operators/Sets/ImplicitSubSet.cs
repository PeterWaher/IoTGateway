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
	/// Defines a Subset, by implicitly limiting its members to members of an optional superset, matching a given condition.
	/// </summary>
	public class ImplicitSubSet : TernaryOperator
	{
		public ImplicitSubSet(ScriptNode Pattern, ScriptNode SuperSet, ScriptNode Condition, int Start, int Length, Expression Expression)
			: base(Pattern, SuperSet, Condition, Start, Length, Expression)
		{
		}

		public override IElement Evaluate(Variables Variables)
		{
			ISet SuperSet;

			if (this.middle == null)
				SuperSet = null;
			else
			{
				IElement E = this.middle.Evaluate(Variables);
				SuperSet = E.AssociatedObjectValue as ISet;
				if (SuperSet == null)
					throw new ScriptRuntimeException("Superset did not evaluate to a set.", this);
			}

			return new SubSet(this.left, SuperSet, this.right, Variables);
		}
	}
}
