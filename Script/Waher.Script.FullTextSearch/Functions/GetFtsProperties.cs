using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.FullTextSearch;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.FullTextSearch.Functions
{
	/// <summary>
	/// Gets properties indexed for full-text-search.
	/// </summary>
	public class GetFtsProperties : FunctionMultiVariate
	{
		/// <summary>
		/// Gets properties indexed for full-text-search.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public GetFtsProperties(int Start, int Length, Expression Expression)
			: base(Array.Empty<ScriptNode>(), argumentTypes0, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets properties indexed for full-text-search.
		/// </summary>
		/// <param name="Collection">Name of database collection.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public GetFtsProperties(ScriptNode Collection,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Collection }, argumentTypes1Scalar,
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(GetFtsProperties);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[]
		{
			"Collection"
		};

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
			if (Arguments.Length == 0)
			{
				Dictionary<string, PropertyDefinition[]> Properties = await Waher.Persistence.FullTextSearch.Search.GetFullTextSearchIndexedProperties();
				Dictionary<string, IElement> Result = new Dictionary<string, IElement>();

				foreach (KeyValuePair<string, PropertyDefinition[]> Rec in Properties)
					Result[Rec.Key] = new ObjectVector(Rec.Value);

				return new ObjectValue(Result);
			}
			else
			{
				string Collection = Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty;
				PropertyDefinition[] Properties = await Waher.Persistence.FullTextSearch.Search.GetFullTextSearchIndexedProperties(Collection);

				return new ObjectVector(Properties);
			}
		}
	}
}
