using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Queries;
using Waher.Things.SourceEvents;
using Waher.Persistence;
using Waher.Persistence.Filters;

namespace Waher.Things.Metering.Commands
{
	/// <summary>
	/// Clears all messages for a node.
	/// </summary>
	public class ClearMessages : ICommand
	{
		private MeteringNode node;

		/// <summary>
		/// Clears all messages for a node.
		/// </summary>
		/// <param name="Node">Metering node.</param>
		public ClearMessages(MeteringNode Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "ClearMessages";

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Simple;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "Node";

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		public string SortKey => "ClearMessages";

		/// <summary>
		/// If the command can be executed by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the command can be executed by the caller.</returns>
		public Task<bool> CanExecuteAsync(RequestOrigin Caller)
		{
			return this.node.CanEditAsync(Caller);
		}

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public ICommand Copy()
		{
			return new ClearMessages(this.node);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public async Task ExecuteCommandAsync()
		{
			foreach (MeteringMessage Msg in await Database.Find<MeteringMessage>(new FilterFieldEqualTo("NodeId", this.node.NodeId)))
				await Database.Delete(Msg);

			if (this.node.State != NodeState.None)
				this.node.State = NodeState.None;

			await this.node.NodeStateChanged();
		}

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MeteringTopology), 96, "Erase messages");
		}

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MeteringTopology), 97, "Messages successfully erased.");
		}

		/// <summary>
		/// Gets a confirmation string, if any, of the command. If no confirmation is necessary, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetConfirmationStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MeteringTopology), 98, "Are you sure you want to erase all messages for the selected node(s)?");
		}

		/// <summary>
		/// Gets a failure string, if any, of the command. If no specific failure string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MeteringTopology), 99, "Unable to erase messages.");
		}

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		public Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			return Task.CompletedTask;
		}
	}
}
