using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Counters;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Arithmetics;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Gets a set of counters and returns an object with keys as properties and counter
	/// values the values. The function can also create a tree structure, if provided a
	/// delimiter.
	/// </summary>
	public class GetCounters : FunctionMultiVariate
	{
		/// <summary>
		/// Gets a set of counters and returns an object with keys as properties and counter
		/// values the values.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetCounters(int Start, int Length, Expression Expression)
			: base(Array.Empty<ScriptNode>(), argumentTypes0,
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets a set of counters and returns an object with keys as properties and counter
		/// values the values.
		/// </summary>
		/// <param name="KeyPrefix">Counter Key prefix.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetCounters(ScriptNode KeyPrefix, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { KeyPrefix }, argumentTypes1Scalar,
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets a set of counters and returns an object with keys as properties and counter
		/// values the values. The function can also create a tree structure, if provided a
		/// delimiter.
		/// </summary>
		/// <param name="KeyPrefix">Counter Key prefix.</param>
		/// <param name="Delimiter">Delimiter for creating a tree structure.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetCounters(ScriptNode KeyPrefix, ScriptNode Delimiter,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { KeyPrefix, Delimiter }, argumentTypes2Scalar,
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets a set of counters and returns an object with keys as properties and counter
		/// values the values. The function can also create a tree structure, if provided a
		/// delimiter.
		/// </summary>
		/// <param name="KeyPrefix">Counter Key prefix.</param>
		/// <param name="Delimiter">Delimiter for creating a tree structure.</param>
		/// <param name="Wildcard">Wildcard character used in key prefix.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetCounters(ScriptNode KeyPrefix, ScriptNode Delimiter, ScriptNode Wildcard,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { KeyPrefix, Delimiter, Wildcard }, argumentTypes3Scalar,
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(GetCounters);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "KeyPrefix", "Delimiter", "Wildcard" };

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
			string KeyPrefix = Arguments.Length > 0 ? Arguments[0].AssociatedObjectValue?.ToString() : null;
			string Delimiter = Arguments.Length > 1 ? Arguments[1].AssociatedObjectValue?.ToString() : null;
			string Wildcard = Arguments.Length > 2 ? Arguments[2].AssociatedObjectValue?.ToString() : null;
			Dictionary<string, long> Counters;

			if (string.IsNullOrEmpty(KeyPrefix))
				Counters = await RuntimeCounters.GetWhereAsync();
			else
			{
				if (string.IsNullOrEmpty(Wildcard))
				{
					Wildcard = "*";
					while (KeyPrefix.Contains(Wildcard))
						Wildcard += "*";

					KeyPrefix += Wildcard;
				}

				Counters = await RuntimeCounters.GetWhereKeyLikeAsync(KeyPrefix, Wildcard);
			}

			Dictionary<string, IElement> Result = new Dictionary<string, IElement>();

			if (string.IsNullOrEmpty(Delimiter))
			{
				foreach (KeyValuePair<string, long> P in Counters)
					Result[P.Key] = new DoubleNumber(P.Value);
			}
			else
			{
				string[] Delimiters = new string[] { Delimiter };

				foreach (KeyValuePair<string, long> P in Counters)
				{
					Dictionary<string, IElement> Next, Loop = Result;
					string Key = P.Key;
					string[] Parts = Key.Split(Delimiters, StringSplitOptions.None);
					int i, c = Parts.Length;

					for (i = 0; i < c; i++)
					{
						Key = Parts[i];

						if (Loop.TryGetValue(Key, out IElement E))
						{
							if (i < c - 1)
							{
								if (E.AssociatedObjectValue is Dictionary<string, IElement> Next2)
									Next = Next2;
								else
								{
									Next = new Dictionary<string, IElement>()
									{
										{ string.Empty, E }
									};

									Loop[Key] = new ObjectValue(Next);
								}
							}
							else
							{
								E = Add.EvaluateAddition(E, new DoubleNumber(P.Value), this);
								Loop[Key] = E;
								Next = null;
							}
						}
						else
						{
							if (i < c - 1)
							{
								Next = new Dictionary<string, IElement>();
								E = new ObjectValue(Next);
							}
							else
							{
								E = new DoubleNumber(P.Value);
								Next = null;
							}

							Loop[Key] = E;
						}

						Loop = Next;
					}
				}
			}

			return new ObjectValue(Result);
		}
	}
}
