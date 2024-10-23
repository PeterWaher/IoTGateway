using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator;
using Waher.Runtime.Language;
using Waher.Things.Queries;

namespace Waher.Things.Xmpp.Commands
{
	/// <summary>
	/// Scans a concentrator node for its root sources.
	/// </summary>
	public class ScanRootSources : ICommand
	{
		private readonly ConcentratorDevice concentrator;

		/// <summary>
		/// Scans a concentrator node for its root sources.
		/// </summary>
		/// <param name="Concentrator">Concentrator node.</param>
		public ScanRootSources(ConcentratorDevice Concentrator)
		{
			this.concentrator = Concentrator;
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "ScanRootSources";

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Simple;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "Concentrator";

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		public string SortKey => "1";

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ConcentratorDevice), 52, "Scan Root Sources");
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
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// If the command can be executed by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the command can be executed by the caller.</returns>
		public Task<bool> CanExecuteAsync(RequestOrigin Caller)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public async Task ExecuteCommandAsync()
		{
			ConcentratorClient Client = await this.concentrator.GetConcentratorClient()
				?? throw new Exception("Concentrator client not found.");

			DataSourceReference[] Sources = await Client.GetRootDataSourcesAsync(this.concentrator.JID);

		}

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		public Task StartQueryExecutionAsync(Query Query, Language Language) => Task.CompletedTask;

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public ICommand Copy()
		{
			return new ScanRootSources(this.concentrator);
		}
	}
}
