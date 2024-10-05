namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Discovery Message
	/// </summary>
	public class DiscoveryMessage : Ieee1451_0Message
	{
		/// <summary>
		/// IEEE 1451.0 Discovery Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="DiscoveryService">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public DiscoveryMessage(NetworkServiceType NetworkServiceType, DiscoveryService DiscoveryService, 
			MessageType MessageType, byte[] Body, byte[] Tail)
			: base(NetworkServiceType, (byte)DiscoveryService, MessageType, Body, Tail)
		{
			this.DiscoveryService = DiscoveryService;
		}

		/// <summary>
		/// Discovery Service
		/// </summary>
		public DiscoveryService DiscoveryService { get; }

		/// <summary>
		/// Name of <see cref="Ieee1451_0Message.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.DiscoveryService.ToString();
	}
}
