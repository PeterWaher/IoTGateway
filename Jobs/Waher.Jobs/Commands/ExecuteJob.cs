using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Queries;

namespace Waher.Jobs.Commands
{
	/// <summary>
	/// Executes a job.
	/// </summary>
	public class ExecuteJob : ICommand
	{
		private readonly JobNode node;

		/// <summary>
		/// Executes a job.
		/// </summary>
		/// <param name="Node">Job node.</param>
		public ExecuteJob(JobNode Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => nameof(ExecuteJob);

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Simple;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "Job";

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		public string SortKey => nameof(ExecuteJob);

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
			return new ExecuteJob(this.node);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public Task ExecuteCommandAsync()
		{
			Task.Run(async () =>
			{
				try
				{
					try
					{
						await this.node.RemoveErrorAsync("ExecutionError");
					}
					catch (Exception ex)
					{
						await this.node.LogErrorAsync("ExecutionError", ex.Message);
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			});

			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ExecuteJob), 13, "Execute Job once.");
		}

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ExecuteJob), 14, "Job successfully started.");
		}

		/// <summary>
		/// Gets a confirmation string, if any, of the command. If no confirmation is necessary, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetConfirmationStringAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// Gets a failure string, if any, of the command. If no specific failure string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ExecuteJob), 15, "Unable to start the execution of the job.");
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
