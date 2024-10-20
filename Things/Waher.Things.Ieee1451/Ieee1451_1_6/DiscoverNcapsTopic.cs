using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Runtime.Language;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Mqtt;
using Waher.Things.Mqtt.Model;
using Waher.Things.Queries;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Requests Ncaps to identify themselves.
	/// </summary>
	public class DiscoverNcapsTopic : ICommand
	{
		private readonly MqttBrokerNode brokerNode;
		private readonly MqttTopicNode topicNode;

		/// <summary>
		/// Requests Ncaps to identify themselves.
		/// </summary>
		/// <param name="BrokerNode">Broker node.</param>
		/// <param name="TopicNode">Topic node.</param>
		public DiscoverNcapsTopic(MqttBrokerNode BrokerNode, MqttTopicNode TopicNode)
		{
			this.brokerNode = BrokerNode;
			this.topicNode = TopicNode;
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "DiscoverNcaps";

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Simple;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "IEEE1451.1.6";

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
			return Language.GetStringAsync(typeof(RootTopic), 24, "Discover NCAPs");
		}

		/// <summary>
		/// Gets a confirmation string, if any, of the command. If no confirmation is necessary, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetConfirmationStringAsync(Language Language) => Task.FromResult<string>(null);

		/// <summary>
		/// Gets a failure string, if any, of the command. If no specific failure string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetFailureStringAsync(Language Language) => Task.FromResult<string>(null);

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language) => Task.FromResult<string>(null);

		/// <summary>
		/// If the command can be executed by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the command can be executed by the caller.</returns>
		public Task<bool> CanExecuteAsync(RequestOrigin Caller) => Task.FromResult(true);

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		public Task StartQueryExecutionAsync(Query Query, Language Language) => this.ExecuteCommandAsync();

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public ICommand Copy()
		{
			return new DiscoverNcapsTopic(this.brokerNode, this.topicNode);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public Task ExecuteCommandAsync()
		{
			byte[] Request = DiscoveryMessage.SerializeRequest(DiscoveryService.NCAPDiscovery);
			MqttBroker Broker = this.brokerNode.GetBroker();
			return Broker.Publish(this.topicNode.FullTopic, MqttQualityOfService.AtLeastOnce, false, Request);
		}
	}
}
