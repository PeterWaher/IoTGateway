using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Operators;
using Waher.Script.Persistence.SQL;

namespace Waher.Persistence.FullTextSearch.PropertyEvaluators
{
	/// <summary>
	/// Evaluates property values where properties are defined as script expressions.
	/// </summary>
	public class LambdaEvaluator : IPropertyEvaluator
	{
		private LambdaDefinition lambda;
		private ObjectProperties variables;

		/// <summary>
		/// Evaluates property values where properties are defined as script expressions.
		/// </summary>
		public LambdaEvaluator()
		{
		}

		/// <summary>
		/// Parses a property definition.
		/// </summary>
		/// <param name="Definition">Property definition</param>
		/// <returns>Parsed definition.</returns>
		public async Task Prepare(string Definition)
		{
			Expression Exp = new Expression(Definition);
			this.lambda = await Exp.EvaluateAsync(new Variables()) as LambdaDefinition;
		}

		/// <summary>
		/// Evaluates the parsed definition, on an object instance.
		/// </summary>
		/// <param name="Instance">Object instance being indexed.</param>
		/// <returns>Property value.</returns>
		public async Task<object> Evaluate(object Instance)
		{
			if (!(this.lambda is null))
			{
				if (this.variables is null)
					this.variables = new ObjectProperties(Instance, new Variables());
				else
					this.variables.Object = Instance;

				return await this.lambda.EvaluateAsync(new IElement[] { Expression.Encapsulate(Instance) }, this.variables);
			}
			else
				return null;
		}
	}
}
