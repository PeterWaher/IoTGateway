using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Functions.Things
{
	/// <summary>
	/// Creates a reference object to a thing.
	/// </summary>
	public class ThingReference : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a reference object to a thing.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ThingReference(int Start, int Length, Expression Expression)
			: base(new ScriptNode[0], argumentTypes0, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a reference object to a thing.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ThingReference(ScriptNode NodeId, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { NodeId }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a reference object to a thing.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="SourceId">Source ID</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ThingReference(ScriptNode NodeId, ScriptNode SourceId, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { NodeId, SourceId }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a reference object to a thing.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="SourceId">Source ID</param>
		/// <param name="Partition">Partition</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ThingReference(ScriptNode NodeId, ScriptNode SourceId, ScriptNode Partition, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { NodeId, SourceId, Partition }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(ThingReference);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "NodeId", "SourceId", "Partition" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			switch (Arguments.Length)
			{
				case 0:
					return new ObjectValue(Waher.Things.ThingReference.Empty);

				case 1:
					return new ObjectValue(new Waher.Things.ThingReference(Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty));

				case 2:
					return new ObjectValue(new Waher.Things.ThingReference(
						Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty,
						Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty));

				case 3:
				default:
					return new ObjectValue(new Waher.Things.ThingReference(
						Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty,
						Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty,
						Arguments[2].AssociatedObjectValue?.ToString() ?? string.Empty));

			}
		}
	}
}
