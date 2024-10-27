using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Runtime.Language;
using Waher.Things.Queries;

namespace Waher.Things.Xmpp.Commands
{
    /// <summary>
    /// Reconnects to the XMPP broker.
    /// </summary>
    public class ReconnectCommand : ICommand
    {
        private readonly XmppClient client;

		/// <summary>
		/// Reconnects to the XMPP broker.
		/// </summary>
        /// <param name="Client">XMPP Client</param>
		public ReconnectCommand(XmppClient Client)
		{
			this.client = Client;
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "Reconnect";

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Simple;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "XMPP";

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
			return Language.GetStringAsync(typeof(XmppBrokerNode), 25, "Reconnect");
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
			return Language.GetStringAsync(typeof(XmppBrokerNode), 24, "Unable to reconnect to broker.");
		}

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(XmppBrokerNode), 26, "Reconneting to broker.");
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
		public Task ExecuteCommandAsync()
		{
			client.Reconnect();
			return Task.CompletedTask;
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

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public ICommand Copy()
        {
            return new ReconnectCommand(client);
        }
    }
}
