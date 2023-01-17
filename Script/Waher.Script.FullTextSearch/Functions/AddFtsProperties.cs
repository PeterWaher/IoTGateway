using System.Threading.Tasks;
using Waher.Persistence.FullTextSearch;
using Waher.Persistence.FullTextSearch.PropertyEvaluators;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators;

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
			PropertyDefinition[] Properties = GetPropertyDefinitions(Arguments[1], this);

			bool Result = await Waher.Persistence.FullTextSearch.Search.AddFullTextSearch(Collection, Properties);

			return Result ? BooleanValue.True : BooleanValue.False;
		}

		/// <summary>
		/// Gets property definitions.
		/// </summary>
		/// <param name="Argument">Argument provided in function.</param>
		/// <param name="Node">Node evaluating method.</param>
		/// <returns>Array of property definitions.</returns>
		public static PropertyDefinition[] GetPropertyDefinitions(IElement Argument, ScriptNode Node)
		{
			object Obj = Argument.AssociatedObjectValue;

			if (Obj is PropertyDefinition[] Result)
				return Result;

			if (Obj is string[] Labels)
				return PropertyDefinition.ToArray(Labels);

			if (Obj is PropertyDefinition Def)
				return new PropertyDefinition[] { Def };

			if (Obj is string s)
				return new PropertyDefinition[] { new PropertyDefinition(s) };

			if (Obj is Expression Exp)
				return new PropertyDefinition[] { new PropertyDefinition(typeof(ExpressionEvaluator).FullName, Exp.Script) };

			if (Obj is LambdaDefinition Lambda0 && Lambda0.NrArguments == 1)
				return new PropertyDefinition[] { new PropertyDefinition(typeof(LambdaEvaluator).FullName, Lambda0.SubExpression) };

			if (!(Argument is IVector V))
				throw new ScriptRuntimeException("Expected a vector of property definitions.", Node);

			int i, c = V.Dimension;

			Result = new PropertyDefinition[c];

			for (i = 0; i < c; i++)
			{
				Obj = V.GetElement(i);

				if (Obj is PropertyDefinition Def2)
					Result[i] = Def2;
				else if (Obj is string s2)
					Result[i] = new PropertyDefinition(s2);
				else if (Obj is Expression Expression)
					Result[i] = new PropertyDefinition(typeof(ExpressionEvaluator).FullName, Expression.Script);
				else if (Obj is LambdaDefinition Lambda && Lambda.NrArguments == 1)
					Result[i] = new PropertyDefinition(typeof(LambdaEvaluator).FullName, Lambda.SubExpression);
				else
					throw new ScriptRuntimeException("Unrecognized property definition: " + Obj.GetType().FullName, Node);
			}

			return Result;
		}

	}
}
