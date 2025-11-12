using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Processors.Metering
{
	/// <summary>
	/// Base Interface for all thing error processor nodes.
	/// </summary>
	public interface IThingErrorProcessor : IProcessorNode
	{
		/// <summary>
		/// Process a collection of thing errors.
		/// </summary>
		/// <param name="Device">Thing reporting the errors.</param>
		/// <param name="Errors">Errors to process.</param>
		/// <returns>Processed set of errors. Can be null if no errors pass processing.</returns>
		Task<ThingError[]> ProcessErrors(INode Device, ThingError[] Errors);
	}
}
