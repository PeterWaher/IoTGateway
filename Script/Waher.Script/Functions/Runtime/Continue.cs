using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Continues the next iteration of a loop, with or without providing a loop value.
	/// </summary>
	public class Continue : FunctionOneVariable
	{
		private readonly bool hasValue;

		/// <summary>
		/// Continues the next iteration of a loop, without providing a loop value.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Continue(int Start, int Length, Expression Expression)
			: base(new ConstantElement(ObjectValue.Null, Start, Length, Expression), 
				  Start, Length, Expression)
		{
			this.hasValue = false;
		}

		/// <summary>
		/// Continues the next iteration of a loop, providing a loop value.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Continue(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
			this.hasValue = true;
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Continue);

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (this.hasValue)
				return base.Evaluate(Variables);
			else
			{
				//ScriptContinueLoopException.TryThrowReused();
				throw new ScriptContinueLoopException();
			}
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (this.hasValue)
				return base.EvaluateAsync(Variables);
			else
			{
				//ScriptContinueLoopException.TryThrowReused();
				throw new ScriptContinueLoopException();
			}
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			//ScriptContinueLoopException.TryThrowReused(Argument);
			throw new ScriptContinueLoopException(Argument);
		}
	}
}
