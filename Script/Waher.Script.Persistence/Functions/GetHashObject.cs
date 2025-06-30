using System.Threading.Tasks;
using Waher.Runtime.HashStore;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Tries to get the associated object value from a persisted hash value
	/// </summary>
	public class GetHashObject : FunctionMultiVariate
	{
		/// <summary>
		/// Tries to get the associated object value from a persisted hash value
		/// </summary>
		/// <param name="Hash">Counter Key.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetHashObject(ScriptNode Hash, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Hash }, argumentTypes1Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Tries to get the associated object value from a persisted hash value
		/// </summary>
		/// <param name="Realm">Realm</param>
		/// <param name="Hash">Counter Key.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetHashObject(ScriptNode Realm, ScriptNode Hash, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Realm, Hash },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(GetHashObject);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Realm", "Hash" };

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
			int c = Arguments.Length;
			object Result;

			if (!(Arguments[c - 1].AssociatedObjectValue is byte[] Hash))
				throw new ScriptRuntimeException("Expected last argument to be a hash digest.", this);

			if (Arguments.Length > 1)
			{
				if (!(Arguments[0].AssociatedObjectValue is string Realm))
					throw new ScriptRuntimeException("Expected first argument to be string.", this);

				Result = await PersistedHashes.TryGetAssociatedObject(Realm, Hash);
			}
			else
				Result = await PersistedHashes.TryGetAssociatedObject(Hash);

			return new ObjectValue(Result);
		}
	}
}
