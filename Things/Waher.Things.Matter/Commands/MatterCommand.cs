using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Metering;
using Waher.Things.Queries;

namespace Waher.Things.Matter.Commands
{
	/// <summary>
	/// Abstract base class for Matter commands.
	/// </summary>
	public abstract class MatterCommand : ICommand
	{
		private readonly MeteringNode node;
		private readonly string commandId;
		private readonly CommandType type;
		private readonly string sortKey;

		/// <summary>
		/// Abstract base class for Matter commands.
		/// </summary>
		/// <param name="Node">Node on which the command is executed.</param>
		/// <param name="CommandId">Command ID.</param>
		/// <param name="Type">Command type.</param>
		/// <param name="SortKey">Sort key.</param>
		public MatterCommand(MeteringNode Node, string CommandId, CommandType Type, string SortKey)
		{
			this.node = Node;
			this.commandId = CommandId;
			this.type = Type;
			this.sortKey = SortKey;
		}

		/// <summary>
		/// Node on which the command is executed.
		/// </summary>
		public MeteringNode Node => this.node;

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => this.commandId;

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => this.type;

		/// <summary>
		/// Sort category.
		/// </summary>
		public string SortCategory => "Matter";

		/// <summary>
		/// Sort key.
		/// </summary>
		public string SortKey => this.sortKey;

		/// <summary>
		/// If the command can be executed by the caller. By default, the caller must be able to edit the node.
		/// </summary>
		/// <param name="Caller">Caller.</param>
		/// <returns>If command can be executed.</returns>
		public virtual Task<bool> CanExecuteAsync(RequestOrigin Caller)
		{
			return this.node.CanEditAsync(Caller);
		}

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy.</returns>
		public abstract ICommand Copy();

		/// <summary>
		/// Executes the command.
		/// </summary>
		public virtual Task ExecuteCommandAsync()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets the name of the command.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>Name of command.</returns>
		public abstract Task<string> GetNameAsync(Language Language);

		/// <summary>
		/// Gets the confirmation string.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>Confirmation string.</returns>
		public virtual Task<string> GetConfirmationStringAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// Gets the failure string.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>Failure string.</returns>
		public virtual Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 305, "Unable to execute command.");
		}

		/// <summary>
		/// Gets the success string.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>Success string.</returns>
		public virtual Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 301, "Command executed.");
		}

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="Language">Language.</param>
		public virtual Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			return Task.CompletedTask;
		}
	}
}
