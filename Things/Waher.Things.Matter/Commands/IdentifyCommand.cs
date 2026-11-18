using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Matter.Clusters;
using Waher.Things.Matter.Tlv;
using Waher.Things.Queries;

namespace Waher.Things.Matter.Commands
{
	/// <summary>
	/// Makes an endpoint identify itself (for instance, by blinking a light), using the Identify cluster.
	/// </summary>
	public class IdentifyCommand : ICommand
	{
		private readonly ClusterContext context;
		private int seconds = 10;

		/// <summary>
		/// Makes an endpoint identify itself (for instance, by blinking a light), using the Identify cluster.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		public IdentifyCommand(ClusterContext Context)
		{
			this.context = Context;
		}

		/// <summary>
		/// Number of seconds to identify. 0 stops an ongoing identification.
		/// </summary>
		[Page(1, "Identify")]
		[Header(2, "Seconds:")]
		[ToolTip(3, "Number of seconds the device should identify itself. 0 stops identification.")]
		[Range(0, 65534)]
		[Required]
		public int Seconds
		{
			get => this.seconds;
			set => this.seconds = value;
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "Matter.Identify";

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Parametrized;

		/// <summary>
		/// Sort category.
		/// </summary>
		public string SortCategory => "Matter";

		/// <summary>
		/// Sort key.
		/// </summary>
		public string SortKey => "Identify";

		/// <summary>
		/// If the command can be executed by the caller.
		/// </summary>
		/// <param name="Caller">Caller.</param>
		/// <returns>If command can be executed.</returns>
		public Task<bool> CanExecuteAsync(RequestOrigin Caller)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy.</returns>
		public ICommand Copy()
		{
			return new IdentifyCommand(this.context)
			{
				seconds = this.seconds
			};
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public async Task ExecuteCommandAsync()
		{
			await this.context.InvokeAsync(0x00, ClusterHandler.Fields(TlvElement.UInt(0, (ushort)this.seconds)));
		}

		/// <summary>
		/// Gets the confirmation string.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>Confirmation string.</returns>
		public Task<string> GetConfirmationStringAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// Gets the failure string.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>Failure string.</returns>
		public Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 302, "Unable to make the device identify itself.");
		}

		/// <summary>
		/// Gets the name of the command.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>Name of command.</returns>
		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 303, "Identify...");
		}

		/// <summary>
		/// Gets the success string.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>Success string.</returns>
		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 304, "Identify command sent to device.");
		}

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="Language">Language.</param>
		public Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			return Task.CompletedTask;
		}
	}
}
