using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Matter.Clusters;
using Waher.Things.Matter.Tlv;
using Waher.Things.Queries;

namespace Waher.Things.Matter.Commands
{
	/// <summary>
	/// Simple command that invokes a Matter command on a cluster of an endpoint.
	/// </summary>
	public class ClusterCommand : ICommand
	{
		private readonly ClusterContext context;
		private readonly uint commandId;
		private readonly string name;
		private readonly TlvElement fields;
		private readonly bool timed;

		/// <summary>
		/// Simple command that invokes a Matter command on a cluster of an endpoint.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="CommandId">Command ID.</param>
		/// <param name="Name">Displayable name of command.</param>
		/// <param name="Fields">Command fields, or null if none.</param>
		/// <param name="Timed">If a timed interaction is required.</param>
		public ClusterCommand(ClusterContext Context, uint CommandId, string Name, TlvElement Fields, bool Timed)
		{
			this.context = Context;
			this.commandId = CommandId;
			this.name = Name;
			this.fields = Fields;
			this.timed = Timed;
		}

		/// <summary>
		/// Cluster context.
		/// </summary>
		public ClusterContext Context => this.context;

		/// <summary>
		/// Matter command ID.
		/// </summary>
		public uint MatterCommandId => this.commandId;

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "Matter." + this.context.ClusterId.ToString("X4") + "." + this.commandId.ToString("X2");

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Simple;

		/// <summary>
		/// Sort category.
		/// </summary>
		public string SortCategory => this.context.ClusterName;

		/// <summary>
		/// Sort key.
		/// </summary>
		public string SortKey => this.commandId.ToString("X2");

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
			return new ClusterCommand(this.context, this.commandId, this.name, this.fields, this.timed);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public async Task ExecuteCommandAsync()
		{
			await this.context.InvokeAsync(this.commandId, this.fields, this.timed);
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
			return Language.GetStringAsync(typeof(MatterControllerNode), 300, "Unable to execute command on device.");
		}

		/// <summary>
		/// Gets the name of the command.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>Name of command.</returns>
		public Task<string> GetNameAsync(Language Language)
		{
			return Task.FromResult(this.name);
		}

		/// <summary>
		/// Gets the success string.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>Success string.</returns>
		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 301, "Command executed.");
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
