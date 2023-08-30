using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Things;
using Waher.Things.Metering;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Gets a node object on the gateway.
	/// </summary>
	public class GetNode : FunctionMultiVariate
	{
		/// <summary>
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
					string SourceId;

					if (Arguments[0] is IThingReference ThingReference)
					{
						if (!TryGetDataSource(ThingReference.SourceId, out Source))
							return ObjectValue.Null;

						Result = await Source.GetNodeAsync(ThingReference);
					}
					else
					{
						SourceId = MeteringTopology.SourceID;
						if (!TryGetDataSource(SourceId, out Source))
							return ObjectValue.Null;

						Result = await Source.GetNodeAsync(new Things.ThingReference(Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty, SourceId));
					}
					break;

				case 2:
					SourceId = Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty;
					if (!TryGetDataSource(SourceId, out Source))
						return ObjectValue.Null;

					Result = await Source.GetNodeAsync(new Things.ThingReference(
						Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty, SourceId));
					break;

				case 3:
				default:
					SourceId = Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty;
					if (!TryGetDataSource(SourceId, out Source))
						return ObjectValue.Null;

					Result = await Source.GetNodeAsync(new Things.ThingReference(
						Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty, SourceId,
						Arguments[2].AssociatedObjectValue?.ToString() ?? string.Empty));
					break;
			}

			if (Result is null)
				return ObjectValue.Null;
			else
				return new ObjectValue(Result);
		}

		/// <summary>
		/// Tries to get a data source, from its data source ID.
		/// </summary>
		/// <param name="SourceId">Data Source ID</param>
		/// <param name="Source">Source, if found, null otherwise.</param>
		/// <returns>If a source with the given ID was found.</returns>
		public static bool TryGetDataSource(string SourceId, out IDataSource Source)
		{
			if (!Types.TryGetModuleParameter("Sources", out object Obj) || !(Obj is IDataSource[] Sources))
			{
				Source = null;
				return false;
			}

			if (Sources != sources)
			{
				Dictionary<string, IDataSource> SourceById = new Dictionary<string, IDataSource>();
				Add(SourceById, Sources);
				sourceById = SourceById;
				sources = Sources;
			}

			return sourceById.TryGetValue(SourceId, out Source);
		}

		private static void Add(Dictionary<string, IDataSource> SourceById, IEnumerable<IDataSource> Sources)
		{
			foreach (IDataSource Source in Sources)
			{
				SourceById[Source.SourceID] = Source;

				if (Source.HasChildren)
					Add(SourceById, Source.ChildSources);
			}
		}

		private static IDataSource[] sources = null;
		private static Dictionary<string, IDataSource> sourceById = null;
	}
}
