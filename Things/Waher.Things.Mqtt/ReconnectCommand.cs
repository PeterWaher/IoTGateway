using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Runtime.Language;
using Waher.Things.Queries;

namespace Waher.Things.Mqtt
{
	internal class ReconnectCommand : ICommand
	{
		public MqttClient client;

		public string CommandID => "Reconnect";
		public CommandType Type => CommandType.Simple;
		public string SortCategory => "MQTT";
		public string SortKey => "1";

		public ReconnectCommand(MqttClient Client)
		{
			this.client = Client;
		}

		public Task<bool> CanExecuteAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(true);
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
			return Task.FromResult<string>(string.Empty);
		}

		public Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttBrokerNode), 27, "Unable to reconnect to broker.");
		}

		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttBrokerNode), 29, "Reconnect");
		}

		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttBrokerNode), 28, "Reconneting to broker.");
		}

		public Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			return Task.CompletedTask;
		}
	}
}
