﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Conditional;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Creates a vector using a DO-WHILE statement.
	/// </summary>
	public class VectorDoWhileDefinition : BinaryOperator
	{
		/// <summary>
		/// Creates a vector using a DO-WHILE statement.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VectorDoWhileDefinition(DoWhile Elements, int Start, int Length, Expression Expression)
			: base(Elements.LeftOperand, Elements.RightOperand, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            LinkedList<IElement> Elements = new LinkedList<IElement>();
            BooleanValue Condition;

            do
            {
				try
				{
					Elements.AddLast(this.left.Evaluate(Variables));
				}
				catch (ScriptBreakLoopException ex)
				{
					if (ex.HasLoopValue)
						Elements.AddLast(ex.LoopValue);

					//ScriptBreakLoopException.Reuse(ex);
					break;
				}
				catch (ScriptContinueLoopException ex)
				{
					if (ex.HasLoopValue)
						Elements.AddLast(ex.LoopValue);

					//ScriptContinueLoopException.Reuse(ex);
				}
				
                Condition = this.right.Evaluate(Variables) as BooleanValue;
                if (Condition is null)
                    throw new ScriptRuntimeException("Condition must evaluate to a boolean value.", this);
            }
            while (Condition.Value);

            return this.Encapsulate(Elements);
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

            LinkedList<IElement> Elements = new LinkedList<IElement>();
            BooleanValue Condition;

            do
            {
				try
				{
					Elements.AddLast(await this.left.EvaluateAsync(Variables));
				}
				catch (ScriptBreakLoopException ex)
				{
					if (ex.HasLoopValue)
						Elements.AddLast(ex.LoopValue);

					//ScriptBreakLoopException.Reuse(ex);
					break;
				}
				catch (ScriptContinueLoopException ex)
				{
					if (ex.HasLoopValue)
						Elements.AddLast(ex.LoopValue);

					//ScriptContinueLoopException.Reuse(ex);
				}

				Condition = await this.right.EvaluateAsync(Variables) as BooleanValue;
                if (Condition is null)
                    throw new ScriptRuntimeException("Condition must evaluate to a boolean value.", this);
            }
            while (Condition.Value);

            return this.Encapsulate(Elements);
        }

        /// <summary>
        /// Encapsulates the calculated elements.
        /// </summary>
        /// <param name="Elements">Elements to encapsulate.</param>
        /// <returns>Encapsulated element.</returns>
        protected virtual IElement Encapsulate(LinkedList<IElement> Elements)
        {
            return VectorDefinition.Encapsulate(Elements, true, this);
        }

    }
}
