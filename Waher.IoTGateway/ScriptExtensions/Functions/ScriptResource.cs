using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Defines a script resource on the web server hosted by the gateway. Script resources are persisted, and will be 
	/// available after the gateway is restarted, until they are removed by calling `RemoveScriptResource`. If a non-script 
	/// resource already exists with the given name, the new resource is not added. The function returns a Boolean value 
	/// showing if the script resource was added or not.
	/// </summary>
	public class ScriptResource : FunctionTwoVariables
	{
		/// <summary>
		/// Defines a script resource on the web server hosted by the gateway. Script resources are persisted, and will be 
		/// available after the gateway is restarted, until they are removed by calling `RemoveScriptResource`. If a non-script 
		/// resource already exists with the given name, the new resource is not added. The function returns a Boolean value 
		/// showing if the script resource was added or not.
		/// </summary>
		/// <param name="Resource">Resource.</param>
		/// <param name="Exp">Script resource expression.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ScriptResource(ScriptNode Resource, ScriptNode Exp, int Start, int Length, Expression Expression)
			: base(Resource, Exp, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(ScriptResource);

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "Resource", "Expression" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.EvaluateAsync(Variables).Result;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			IElement E;

			if (this.Argument1.IsAsynchronous)
				E = await this.Argument1.EvaluateAsync(Variables);
			else
				E = this.Argument1.Evaluate(Variables);

			if (!(E is StringValue S))
				throw new ScriptRuntimeException("Expected string resource name.", this);

			string ResourceName = S.Value;

			return new BooleanValue(await Gateway.AddScriptResource(ResourceName, this.Argument2, this.Expression.Source ?? string.Empty));
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument1, IElement Argument2, Variables Variables)
		{
			return BooleanValue.False;
		}

	}
}
