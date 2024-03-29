﻿using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// DO-WHILE operator.
	/// </summary>
	public class DoWhile : BinaryOperator
	{
		/// <summary>
		/// DO-WHILE operator.
		/// </summary>
		/// <param name="Statement">Statement.</param>
		/// <param name="Condition">Condition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DoWhile(ScriptNode Statement, ScriptNode Condition, int Start, int Length, Expression Expression)
			: base(Statement, Condition, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement Last;
            BooleanValue Condition;

            do
            {
                Last = this.left.Evaluate(Variables);

                Condition = this.right.Evaluate(Variables) as BooleanValue;
                if (Condition is null)
                    throw new ScriptRuntimeException("Condition must evaluate to a boolean value.", this);
            }
            while (Condition.Value);

            return Last;
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

			IElement Last;
			BooleanValue Condition;

			do
			{
				Last = await this.left.EvaluateAsync(Variables);

				Condition = await this.right.EvaluateAsync(Variables) as BooleanValue;
				if (Condition is null)
					throw new ScriptRuntimeException("Condition must evaluate to a boolean value.", this);
			}
			while (Condition.Value);

			return Last;
		}
	}
}
