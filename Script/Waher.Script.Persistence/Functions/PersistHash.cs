using System;
using System.Threading.Tasks;
using Waher.Runtime.HashStore;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Persists a hash value
	/// </summary>
	public class PersistHash : FunctionMultiVariate
	{
		/// <summary>
		/// Persists a hash value
		/// </summary>
		/// <param name="Hash">Counter Key.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PersistHash(ScriptNode Hash, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Hash }, argumentTypes1Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Persists a hash value
		/// </summary>
		/// <param name="Realm">Realm</param>
		/// <param name="Hash">Counter Key.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PersistHash(ScriptNode Realm, ScriptNode Hash, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Realm, Hash },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Persists a hash value
		/// </summary>
		/// <param name="Realm">Realm</param>
		/// <param name="Expires">When hash expires.</param>
		/// <param name="Hash">Counter Key.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PersistHash(ScriptNode Realm, ScriptNode Expires, ScriptNode Hash, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Realm, Expires, Hash },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Persists a hash value
		/// </summary>
		/// <param name="Realm">Realm</param>
		/// <param name="Expires">When hash expires.</param>
		/// <param name="Object">Associated object.</param>
		/// <param name="Hash">Counter Key.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PersistHash(ScriptNode Realm, ScriptNode Expires, ScriptNode Object, 
			ScriptNode Hash, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Realm, Expires, Object, Hash },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(PersistHash);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Realm", "Expires", "Object", "Hash" };

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
			bool Result;

			if (!(Arguments[c - 1].AssociatedObjectValue is byte[] Hash))
				throw new ScriptRuntimeException("Expected last argument to be a hash digest.", this);

			if (Arguments.Length > 1)
			{
				if (!(Arguments[0].AssociatedObjectValue is string Realm))
					throw new ScriptRuntimeException("Expected first argument to be a string.", this);

				if (Arguments.Length > 2)
				{
					if (!(Arguments[1].AssociatedObjectValue is DateTime Expires))
						throw new ScriptRuntimeException("Expected second argument to be a date and time value.", this);

					if (Arguments.Length > 3)
					{
						object Object = Arguments[2].AssociatedObjectValue;
						Result = await PersistedHashes.AddHash(Realm, Expires.ToUniversalTime(), Hash, Object);
					}
					else
						Result = await PersistedHashes.AddHash(Realm, Expires.ToUniversalTime(), Hash);
				}
				else
					Result = await PersistedHashes.AddHash(Realm, Hash);
			}
			else
				Result = await PersistedHashes.AddHash(Hash);

			return Result ? BooleanValue.True : BooleanValue.False;
		}
	}
}
