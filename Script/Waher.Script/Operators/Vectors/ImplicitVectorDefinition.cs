using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.Sets;
using Waher.Script.Operators.Membership;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Defines a vector, by implicitly limiting its members to members of an optional vector, matching given conditions.
	/// </summary>
	public class ImplicitVectorDefinition : BinaryOperator
	{
		private readonly In[] setConditions;
		private readonly ScriptNode[] otherConditions;

		/// <summary>
		/// Defines a vector, by implicitly limiting its members to members of an optional vector, matching given conditions.
		/// </summary>
		/// <param name="Pattern">Pattern defining elements in the set.</param>
		/// <param name="Vector">Optional vector, if defining vector from members of a previous vector.</param>
		/// <param name="Conditions">Conditions of elements in the set.</param>
		/// <param name="DoubleColon">If double-colon was used to define the set.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ImplicitVectorDefinition(ScriptNode Pattern, ScriptNode Vector, ScriptNode[] Conditions,
			int Start, int Length, Expression Expression)
			: base(Pattern, Vector, Start, Length, Expression)
		{
			Sets.ImplicitSetDefinition.SeparateConditions(Conditions, out this.setConditions, out this.otherConditions);
		}

		public override IElement Evaluate(Variables Variables)
		{
			IEnumerable<IElement> VectorElements;
			bool CanEncapsulateAsMatrix;

			if (this.right is null)
			{
				VectorElements = null;
				CanEncapsulateAsMatrix = true;
			}
			else
			{
				IElement E = this.right.Evaluate(Variables);
				CanEncapsulateAsMatrix = (E is IMatrix);
				VectorElements = ImplicitSet.GetSetMembers(E);

				if (VectorElements is null)
					throw new ScriptRuntimeException("Unable to evaluate vector elements.", this.right);
			}

			IEnumerable<IElement> Elements = ImplicitSet.CalculateElements(this.left, VectorElements,
				this.setConditions, this.otherConditions, Variables);

			if (!(Elements is ICollection<IElement> Elements2))
			{
				Elements2 = new List<IElement>();

				foreach (IElement E in Elements)
					Elements2.Add(E);
			}

			return VectorDefinition.Encapsulate(Elements2, CanEncapsulateAsMatrix, this);
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is ImplicitVectorDefinition O &&
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
			Result ^= Result << 5 ^ GetHashCode(this.setConditions);
			Result ^= Result << 5 ^ GetHashCode(this.otherConditions);
			return Result;
		}
	}
}
