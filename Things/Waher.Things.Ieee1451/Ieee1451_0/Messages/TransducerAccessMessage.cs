namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Transducer Access Message
	/// </summary>
	public class TransducerAccessMessage : Ieee1451_0Message
	{
		/// <summary>
		/// IEEE 1451.0 Transducer Access Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="TransducerAccessService">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public TransducerAccessMessage(NetworkServiceType NetworkServiceType, TransducerAccessService TransducerAccessService, 
			MessageType MessageType, byte[] Body, byte[] Tail)
			: base(NetworkServiceType, (byte)TransducerAccessService, MessageType, Body, Tail)
		{
			this.TransducerAccessService = TransducerAccessService;
		}

		/// <summary>
		/// Transducer Access Service
		/// </summary>
		public TransducerAccessService TransducerAccessService { get; }

		/// <summary>
		/// Name of <see cref="Ieee1451_0Message.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.TransducerAccessService.ToString();
	}
}
