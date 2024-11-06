using System.Text;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Mqtt;
using Waher.Things.Mqtt.Model;
using Waher.Things.Queries;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Requests Ncaps to identify themselves.
	/// </summary>
	public class DiscoverNcapsRoot : ICommand
	{
		private readonly MqttBrokerNode brokerNode;

		/// <summary>
		/// Requests Ncaps to identify themselves.
		/// </summary>
		/// <param name="BrokerNode">Broker node.</param>
		public DiscoverNcapsRoot(MqttBrokerNode BrokerNode)
		{
			this.brokerNode = BrokerNode;
		}

		/// <summary>
		/// Search for NCAPs on this topic.
		/// </summary>
		[Page(1, "IEEE 1451")]
		[Header(21, "Topic:", 100)]
		[ToolTip(22, "Search for NCAPs on this topic.")]
		[Required]
		public string Topic { get; set; }

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "DiscoverNcaps";

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Parametrized;

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
			return Language.GetStringAsync(typeof(RootTopic), 20, "Discover NCAPs...");
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
			return new DiscoverNcapsRoot(this.brokerNode)
			{
				Topic = this.Topic
			};
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public async Task ExecuteCommandAsync()
		{
			StringBuilder ToSniffer = this.brokerNode.HasSniffers ? new StringBuilder() : null;
			byte[] Request = DiscoveryMessage.SerializeRequest(ToSniffer);
			MqttBroker Broker = await this.brokerNode.GetBroker();

			if (!(ToSniffer is null))
				await this.brokerNode.Information(ToSniffer.ToString());

			await Broker.Publish(this.Topic, MqttQualityOfService.AtLeastOnce, false, Request);
		}
	}
}
