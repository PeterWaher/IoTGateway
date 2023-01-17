using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Persistence.SQL;

namespace Waher.Persistence.FullTextSearch.PropertyEvaluators
{
	/// <summary>
	/// Evaluates property values where properties are defined as script expressions.
	/// </summary>
	public class ExpressionEvaluator : IPropertyEvaluator
	{
		private Expression expression;
		private ObjectProperties variables;

		/// <summary>
		/// Evaluates property values where properties are defined as script expressions.
		/// </summary>
		public ExpressionEvaluator()
		{
		}

		/// <summary>
		/// Parses a property definition.
		/// </summary>
		/// <param name="Definition">Property definition</param>
		/// <returns>Parsed definition.</returns>
		public Task Prepare(string Definition)
		{
			this.expression = new Expression(Definition);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Evaluates the parsed definition, on an object instance.
		/// </summary>
		/// <param name="Instance">Object instance being indexed.</param>
		/// <returns>Property value.</returns>
		public Task<object> Evaluate(object Instance)
		{
			if (this.variables is null)
				this.variables = new ObjectProperties(Instance, new Variables());
			else
				this.variables.Object = Instance;

			return this.expression.EvaluateAsync(this.variables);
		}
	}
}
