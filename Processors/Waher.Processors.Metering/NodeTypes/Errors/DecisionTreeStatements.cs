using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Things;

namespace Waher.Processors.Metering.NodeTypes.Errors
{
	/// <summary>
	/// Abstract base class for decision tree nodes containing its own set of statements.
	/// </summary>
	public abstract class DecisionTreeStatements : ProcessorNode, IDecisionTreeStatements
	{
		/// <summary>
		/// Abstract base class for decision tree nodes containing its own set of statements.
		/// </summary>
		public DecisionTreeStatements()
			: base()
		{
		}

		/// <summary>
		/// If the children of the node have an intrinsic order (true), or if the order is not important (false).
		/// </summary>
		public override bool ChildrenOrdered => true;

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is IDecisionTreeStatement);
		}

		/// <summary>
		/// Process a collection of thing errors.
		/// </summary>
		/// <param name="Device">Thing reporting the errors.</param>
		/// <param name="Errors">Errors to process.</param>
		/// <returns>Processed set of errors. Can be null if no errors pass processing.</returns>
		public async Task<ThingError[]> ProcessErrors(INode Device, ThingError[] Errors)
		{
			ChunkedList<ThingError> Step = null;

			foreach (INode Child in await this.ChildNodes)
			{
				if (!(Child is IDecisionTreeStatement DecisionTreeStatement))
					continue;

				foreach (ThingError Error in Errors)
				{
					ThingError[] Processed = await DecisionTreeStatement.ProcessError(Device, Error);
					if (Processed is null)
						continue;

					Step ??= new ChunkedList<ThingError>();
					Step.AddRange(Processed);
				}

				if (Step is null)
					return null;

				Errors = Step.ToArray();
			}

			return Errors;
		}

		/// <summary>
		/// Processes a single thing error.
		/// </summary>
		/// <param name="Device">Thing reporting the errors.</param>
		/// <param name="Error">Error to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public virtual Task<ThingError[]> ProcessError(INode Device, ThingError Error)
		{
			ThingError[] Errors = new ThingError[] { Error };
			return this.ProcessErrors(Device, Errors);
		}
	}
}
