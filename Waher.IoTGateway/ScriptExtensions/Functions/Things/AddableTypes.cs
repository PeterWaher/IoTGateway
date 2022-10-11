using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;
using Waher.Things;

namespace Waher.IoTGateway.ScriptExtensions.Functions.Things
{
	/// <summary>
	/// Gets an array of types of nodes that can be added to an existing node.
	/// </summary>
	public class AddableTypes : FunctionOneVariable
	{
		/// <summary>
		/// Gets an array of types of nodes that can be added to an existing node.
		/// </summary>
		/// <param name="Node">Node.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public AddableTypes(ScriptNode Node, int Start, int Length, Expression Expression)
			: base(Node, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(AddableTypes);

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			return this.EvaluateAsync(Argument, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement Argument, Variables Variables)
		{
			if (!(Argument.AssociatedObjectValue is INode Node))
				throw new ScriptRuntimeException("Expected a node as argument.", this);

			return new ObjectVector(await ConcentratorServer.GetAddableTypes(Node));
		}
	}
}
