namespace Waher.Things.Ieee1451.Ieee1451_0.Raw
{
	/// <summary>
	/// IEEE 1451.0 TEDS Access Message
	/// </summary>
	public class TedsAccessMessage : RawMessage
	{
		/// <summary>
		/// IEEE 1451.0 TEDS Access Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="TedsAccessService">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public TedsAccessMessage(NetworkServiceType NetworkServiceType, TedsAccessService TedsAccessService, 
			MessageType MessageType, byte[] Body, byte[] Tail)
			: base(NetworkServiceType, (byte)TedsAccessService, MessageType, Body, Tail)
		{
			this.TedsAccessService = TedsAccessService;
		}

		/// <summary>
		/// TEDS Access Service
		/// </summary>
		public TedsAccessService TedsAccessService { get; }

		/// <summary>
		/// Name of <see cref="RawMessage.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.TedsAccessService.ToString();
	}
}
