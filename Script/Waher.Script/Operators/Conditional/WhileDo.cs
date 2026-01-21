using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Conditional
{
    /// <summary>
    /// WHILE-DO operator.
    /// </summary>
    public class WhileDo : BinaryOperator
    {
        /// <summary>
        /// WHILE-DO operator.
        /// </summary>
        /// <param name="Condition">Condition.</param>
        /// <param name="Statement">Statement.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public WhileDo(ScriptNode Condition, ScriptNode Statement, int Start, int Length, Expression Expression)
            : base(Condition, Statement, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement Last = ObjectValue.Null;
            BooleanValue Condition;

            Condition = this.left.Evaluate(Variables) as BooleanValue;
            if (Condition is null)
                throw new ConditionNotBooleanScriptException(this);

            while (Condition.Value)
            {
                try
                {
                    Last = this.right.Evaluate(Variables);
				}
				catch (ScriptBreakLoopException ex)
				{
					if (ex.HasLoopValue)
						Last = ex.LoopValue;

					//ScriptBreakLoopException.Reuse(ex);
					break;
				}
				catch (ScriptContinueLoopException ex)
				{
					if (ex.HasLoopValue)
						Last = ex.LoopValue;
				
					//ScriptContinueLoopException.Reuse(ex);
				}

				Condition = this.left.Evaluate(Variables) as BooleanValue;
                if (Condition is null)
                    throw new ConditionNotBooleanScriptException(this);
            }

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

            IElement Last = ObjectValue.Null;
            BooleanValue Condition;

            Condition = await this.left.EvaluateAsync(Variables) as BooleanValue;
            if (Condition is null)
                throw new ConditionNotBooleanScriptException(this);

            while (Condition.Value)
            {
                try
                {
                    Last = await this.right.EvaluateAsync(Variables);
                }
				catch (ScriptBreakLoopException ex)
				{
					if (ex.HasLoopValue)
						Last = ex.LoopValue;

					//ScriptBreakLoopException.Reuse(ex);
					break;
				}
				catch (ScriptContinueLoopException ex)
				{
					if (ex.HasLoopValue)
						Last = ex.LoopValue;
				
					//ScriptContinueLoopException.Reuse(ex);
				}

				Condition = await this.left.EvaluateAsync(Variables) as BooleanValue;
                if (Condition is null)
                    throw new ConditionNotBooleanScriptException(this);
            }

            return Last;
        }
    }
}
