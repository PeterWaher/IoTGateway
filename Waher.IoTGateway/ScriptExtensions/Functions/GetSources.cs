using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Things;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Gets available sources of things.
	/// </summary>
	public class GetSources : FunctionZeroVariables
	{

		/// <summary>
		/// Gets available sources of things.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetSources(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(GetSources);

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// This method should be used for nodes whose <see cref="ScriptNode.IsAsynchronous"/> is false.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (Types.TryGetModuleParameter("Sources", out object Obj) && Obj is IDataSource[] Sources)
				return new ObjectValue(Sources);
			else
				return ObjectValue.Null;
		}
	}
}
