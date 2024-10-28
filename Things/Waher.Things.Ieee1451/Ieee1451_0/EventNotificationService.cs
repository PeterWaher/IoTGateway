namespace Waher.Things.Ieee1451.Ieee1451_0
{
	/// <summary>
	/// Event Notification Service
	/// </summary>
	public enum EventNotificationService
	{
		/// <summary>
		/// SubscribeTransducerEventFromOneChannelOfOneTIM (1)
		/// </summary>
		SubscribeTransducerEventFromOneChannelOfOneTIM = 1,

		/// <summary>
		/// NotifyTransducerEventFromOneChannelOfOneTIM (2)
		/// </summary>
		NotifyTransducerEventFromOneChannelOfOneTIM = 2,

		/// <summary>
		/// UnsubscribeTransducerEventFromOneChannelOfOneTIM (3)
		/// </summary>
		UnsubscribeTransducerEventFromOneChannelOfOneTIM = 3,

		/// <summary>
		/// SubscribeTransducerEventFromMultipleChannelsOfOneTIM (4)
		/// </summary>
		SubscribeTransducerEventFromMultipleChannelsOfOneTIM = 4,

		/// <summary>
		/// NotifyTransducerEventFromMultipleChannelsOfOneTIM (5)
		/// </summary>
		NotifyTransducerEventFromMultipleChannelsOfOneTIM = 5,

		/// <summary>
		/// UnsubscribeTransducerEventFromMultipleChannelsOfOneTIM (6)
		/// </summary>
		UnsubscribeTransducerEventFromMultipleChannelsOfOneTIM = 6,

		/// <summary>
		/// SubscribeTransducerEventFromMultipleChannelsOfMultipleTIMs (7)
		/// </summary>
		SubscribeTransducerEventFromMultipleChannelsOfMultipleTIMs = 7,

		/// <summary>
		/// NotifyTransducerEventFromMultipleChannelsOfMultipleTIMs (8)
		/// </summary>
		NotifyTransducerEventFromMultipleChannelsOfMultipleTIMs = 8,

		/// <summary>
		/// UnsubscribeTransducerEventFromMultipleChannelsOfMultipleTIMs (9)
		/// </summary>
		UnsubscribeTransducerEventFromMultipleChannelsOfMultipleTIMs = 9,

		/// <summary>
		/// SubscribeNCAPHeartbeat (10)
		/// </summary>
		SubscribeNCAPHeartbeat = 10,

		/// <summary>
		/// NotifyNCAPHeartbeat (11)
		/// </summary>
		NotifyNCAPHeartbeat = 11,

		/// <summary>
		/// UnsubscribeNCAPHeartbeat (12)
		/// </summary>
		UnsubscribeNCAPHeartbeat = 12
	}
}
