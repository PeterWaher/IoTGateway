using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Runtime.Language;
using Waher.Things.Queries;

namespace Waher.Things.Xmpp
{
	internal class ReconnectCommand : ICommand
	{
		public XmppClient client;

		public string CommandID => "Reconnect";
		public CommandType Type => CommandType.Simple;
		public string SortCategory => "XMPP";
		public string SortKey => "1";

		public ReconnectCommand(XmppClient Client)
		{
			this.client = Client;
		}

		public Task<bool> CanExecuteAsync(RequestOrigin Caller)
		{
			return Task.FromResult(true);
		}

		public ICommand Copy()
		{
			return new ReconnectCommand(this.client);
		}

		public Task ExecuteCommandAsync()
		{
			this.client.Reconnect();
			return Task.CompletedTask;
		}

		public Task<string> GetConfirmationStringAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		public Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(XmppBrokerNode), 24, "Unable to reconnect to broker.");
		}

		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(XmppBrokerNode), 25, "Reconnect");
		}

		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(XmppBrokerNode), 26, "Reconneting to broker.");
		}

		public Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			return Task.CompletedTask;
		}
	}
}
