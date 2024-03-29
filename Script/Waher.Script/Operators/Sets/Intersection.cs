﻿using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects.Sets;

namespace Waher.Script.Operators.Sets
{
	/// <summary>
	/// Intersection operator
	/// </summary>
	public class Intersection : BinaryOperator 
	{
		/// <summary>
		/// Intersection operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Intersection(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement L = this.left.Evaluate(Variables);
			if (!(L is ISet S1))
				S1 = new FiniteSet(new IElement[] { L });

			IElement R = this.right.Evaluate(Variables);
			if (!(R is ISet S2))
				S2 = new FiniteSet(new IElement[] { R });

			return new IntersectionSet(S1, S2);
        }

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return this.Evaluate(Variables);

			IElement L = await this.left.EvaluateAsync(Variables);
			if (!(L is ISet S1))
				S1 = new FiniteSet(new IElement[] { L });

			IElement R = await this.right.EvaluateAsync(Variables);
			if (!(R is ISet S2))
				S2 = new FiniteSet(new IElement[] { R });

			return new IntersectionSet(S1, S2);
		}
	}
}
