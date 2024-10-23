using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Runtime.Language;

namespace Waher.Things.Xmpp.Commands
{
	/// <summary>
	/// Sends a presence subscription request.
	/// </summary>
	public class SubscribeToPresence : ConnectedDeviceCommand
	{
		/// <summary>
		/// Sends a presence subscription request.
		/// </summary>
		/// <param name="ConnectedDevice">Connected device.</param>
		public SubscribeToPresence(ConnectedDevice ConnectedDevice)
			: base(ConnectedDevice, "1")
		{
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public override string CommandID => nameof(SubscribeToPresence);

		/// <summary>
		/// Type of command.
		/// </summary>
		public override CommandType Type => CommandType.Simple;

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ConcentratorDevice), 45, "Subscribe");
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public override async Task ExecuteCommandAsync()
		{
			XmppClient Client = await this.GetXmppClient();
			Client.RequestPresenceSubscription(this.ConnectedDevice.JID);
		}

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public override ICommand Copy()
		{
			return new SubscribeToPresence(this.ConnectedDevice);
		}
	}
}
