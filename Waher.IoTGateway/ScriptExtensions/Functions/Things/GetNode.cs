using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Things;
using Waher.Things.Metering;

namespace Waher.IoTGateway.ScriptExtensions.Functions.Things
{
	/// <summary>
	/// Gets a node object on the gateway.
	/// </summary>
	public class GetNode : FunctionMultiVariate
	{
		/// <summary>
		/// Gets a node object on the gateway.
		/// Gets a node object on the gateway.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public GetNode(ScriptNode NodeId, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { NodeId }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets a node object on the gateway.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="SourceId">Source ID</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public GetNode(ScriptNode NodeId, ScriptNode SourceId, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { NodeId, SourceId }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets a node object on the gateway.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="SourceId">Source ID</param>
		/// <param name="Partition">Partition</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public GetNode(ScriptNode NodeId, ScriptNode SourceId, ScriptNode Partition, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { NodeId, SourceId, Partition }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(GetNode);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "NodeId", "SourceId", "Partition" };

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
			INode Result;

			switch (Arguments.Length)
			{
				case 1:
					IDataSource Source;

					if (Arguments[0] is IThingReference ThingReference)
					{
						if (!Gateway.ConcentratorServer.TryGetDataSource(ThingReference.SourceId, out Source))
							return ObjectValue.Null;

						Result = await Source.GetNodeAsync(ThingReference);
					}
					else
					{
						if (!Gateway.ConcentratorServer.TryGetDataSource(MeteringTopology.SourceID, out Source))
							return ObjectValue.Null;

						Result = await Source.GetNodeAsync(new Waher.Things.ThingReference(Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty, MeteringTopology.SourceID));
					}
					break;

				case 2:
					string SourceId = Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty;
					if (!Gateway.ConcentratorServer.TryGetDataSource(SourceId, out Source))
						return ObjectValue.Null;

					Result = await Source.GetNodeAsync(new Waher.Things.ThingReference(
						Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty, SourceId));
					break;

				case 3:
				default:
					SourceId = Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty;
					if (!Gateway.ConcentratorServer.TryGetDataSource(SourceId, out Source))
						return ObjectValue.Null;

					Result = await Source.GetNodeAsync(new Waher.Things.ThingReference(
						Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty, SourceId,
						Arguments[2].AssociatedObjectValue?.ToString() ?? string.Empty));
					break;
			}

			if (Result is null)
				return ObjectValue.Null;
			else
				return new ObjectValue(Result);
		}
	}
}
