using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Decrements a counter
	/// </summary>
	public class DecCounter : FunctionMultiVariate
	{
		/// <summary>
		/// Decrements a counter
		/// </summary>
		/// <param name="Key">Counter Key.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DecCounter(ScriptNode Key, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Key }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Decrements a counter
		/// </summary>
		/// <param name="Key">Counter Key.</param>
		/// <param name="Amount">Amount to increment</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DecCounter(ScriptNode Key, ScriptNode Amount, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Key, Amount }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(DecCounter);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Key", "Amount" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;	
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			long Count;

			if (!(Arguments[0].AssociatedObjectValue is string Key))
				throw new ScriptRuntimeException("Key must be a string.", this);

			if (Arguments.Length == 2)
			{
				double N = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
				long Amount = (long)N;

				if (Amount != N)
					throw new ScriptRuntimeException("Amounts must be integers.", this);

				Count = await Runtime.Counters.RuntimeCounters.DecrementCounter(Key, Amount);
			}
			else
			{
				Count = await Runtime.Counters.RuntimeCounters.DecrementCounter(Key);
			}
		
			return new DoubleNumber(Count);
		}
	}
}
