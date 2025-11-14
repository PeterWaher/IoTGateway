using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things;

namespace Waher.Processors.Metering.NodeTypes.Errors.Actions
{
	/// <summary>
	/// Rejects a field.
	/// </summary>
	public class RejectField : DecisionTreeLeafStatement
	{
		/// <summary>
		/// Rejects a field.
		/// </summary>
		public RejectField()
			: base()
		{
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(RejectField), 19, "Reject Field");
		}

		/// <summary>
		/// Processes a single thing error.
		/// </summary>
		/// <param name="Device">Thing reporting the errors.</param>
		/// <param name="Error">Error to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public override Task<ThingError[]> ProcessError(INode Device, ThingError Error)
		{
			return Task.FromResult<ThingError[]>(null);
		}
	}
}
