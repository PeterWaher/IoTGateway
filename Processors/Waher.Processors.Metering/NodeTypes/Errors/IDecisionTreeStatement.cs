using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Processors.Metering.NodeTypes.Errors
{
	/// <summary>
	/// Interface for decision tree staement nodes.
	/// </summary>
	public interface IDecisionTreeStatement : IProcessorNode
	{
		/// <summary>
		/// Processes a single thing error.
		/// </summary>
		/// <param name="Device">Thing reporting the errors.</param>
		/// <param name="Error">Error to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		Task<ThingError[]> ProcessError(INode Device, ThingError Error);
	}
}
