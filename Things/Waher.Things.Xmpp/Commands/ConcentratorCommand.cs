using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator;

namespace Waher.Things.Xmpp.Commands
{
	/// <summary>
	/// Abstract base class for concentrator commands.
	/// </summary>
	public abstract class ConcentratorCommand : ConnectedDeviceCommand
	{
		private readonly ConcentratorDevice concentrator;

		/// <summary>
		/// Scans a concentrator node for its root sources.
		/// </summary>
		/// <param name="Concentrator">Concentrator node.</param>
		/// <param name="SortKey">Sort key</param>
		public ConcentratorCommand(ConcentratorDevice Concentrator, string SortKey)
			: base(Concentrator, SortKey)
		{
			this.concentrator = Concentrator;
		}

		/// <summary>
		/// Reference to the concentrator node.
		/// </summary>
		public ConcentratorDevice Concentrator => this.concentrator;

		/// <summary>
		/// Gets the concentrator client, if it exists.
		/// </summary>
		/// <returns>Reference to concentrator client.</returns>
		/// <exception cref="Exception">If the client is not found.</exception>
		public async Task<ConcentratorClient> GetConcentratorClient()
		{
			ConcentratorClient Client = await this.concentrator.GetConcentratorClient()
				?? throw new Exception("Concentrator client not found.");

			return Client;
		}
	}
}
