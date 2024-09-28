namespace Waher.Things.Ieee1451.Ieee1451_0.Raw
{
	/// <summary>
	/// IEEE 1451.0 Raw Message
	/// </summary>
	public abstract class RawMessage
	{
		/// <summary>
		/// IEEE 1451.0 Raw Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="NetworkServiceId">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public RawMessage(NetworkServiceType NetworkServiceType, byte NetworkServiceId, 
			MessageType MessageType, byte[] Body, byte[] Tail)
		{
			this.NetworkServiceType = NetworkServiceType;
			this.NetworkServiceId = NetworkServiceId;
			this.MessageType = MessageType;
			this.Body = Body;
			this.Tail = Tail;
		}

		/// <summary>
		/// Network Service Type
		/// </summary>
		public NetworkServiceType NetworkServiceType { get; }

		/// <summary>
		/// Network Service ID
		/// </summary>
		public byte NetworkServiceId { get; }
		
		/// <summary>
		/// Name of <see cref="NetworkServiceId"/>
		/// </summary>
		public abstract string NetworkServiceIdName { get; }

		/// <summary>
		/// Message Type
		/// </summary>
		public MessageType MessageType { get; }

		/// <summary>
		/// Message Body
		/// </summary>
		public byte[] Body { get; }

		/// <summary>
		/// Bytes that are received after the body.
		/// </summary>
		public byte[] Tail { get; }
	}
}
