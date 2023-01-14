using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.FullTextSearch.Functions
{
	/// <summary>
	/// Adds collection properties to the corresponding index for full-text-searching.
	/// </summary>
	public class AddFtsProperties : FunctionMultiVariate
	{
		/// <summary>
		/// Adds collection properties to the corresponding index for full-text-searching.
		/// </summary>
		/// <param name="Collection">Name of database collection.</param>
		/// <param name="Properties">Properties of objects in collection to index.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public AddFtsProperties(ScriptNode Collection, ScriptNode Properties,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Collection, Properties },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(AddFtsProperties);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[]
		{
			"Collection", "Properties"
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
			string Collection = Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty;
			string[] Properties = Arguments[1].AssociatedObjectValue as string[];

			if (Properties is null)
				throw new ScriptRuntimeException("Expected array of strings for the Properties argument.", this);

			bool Result = await Persistence.FullTextSearch.Search.AddFullTextSearch(Collection, Properties);

			return Result ? BooleanValue.True : BooleanValue.False;
		}
	}
}
