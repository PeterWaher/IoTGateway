namespace Waher.Things.Ieee1451.Ieee1451_0
{
	/// <summary>
	/// Transducer access service
	/// </summary>
	public enum TransducerAccessService
	{
		/// <summary>
		/// SyncReadTransducerSampleDataFromAChannelOfATIM (1)
		/// </summary>
		SyncReadTransducerSampleDataFromAChannelOfATIM = 1,

		/// <summary>
		/// SyncReadTransducerBlockDataFromAChannelOfATIM (2)
		/// </summary>
		SyncReadTransducerBlockDataFromAChannelOfATIM = 2,

		/// <summary>
		/// SyncReadTransducerSampleDataFromMulitipleChannelsOfATIM (3)
		/// </summary>
		SyncReadTransducerSampleDataFromMulitipleChannelsOfATIM = 3,

		/// <summary>
		/// SyncReadTransducerBlockDataFromMulitipleChannelsOfATIM (4)
		/// </summary>
		SyncReadTransducerBlockDataFromMulitipleChannelsOfATIM = 4,

		/// <summary>
		/// SyncReadTransducerSampleDataFromMultipleChannelsOfMultipleTIMs (5)
		/// </summary>
		SyncReadTransducerSampleDataFromMultipleChannelsOfMultipleTIMs = 5,

		/// <summary>
		/// SyncReadTransducerBlockDataFromMultipleChannelsOfMultipleTIMs (6)
		/// </summary>
		SyncReadTransducerBlockDataFromMultipleChannelsOfMultipleTIMs = 6,

		/// <summary>
		/// SyncWriteTransducerSampleDataFromAChannelOfATIM (7)
		/// </summary>
		SyncWriteTransducerSampleDataFromAChannelOfATIM = 7,

		/// <summary>
		/// SyncWriteTransducerBlockDataFromAChannelOfATIM (8)
		/// </summary>
		SyncWriteTransducerBlockDataFromAChannelOfATIM = 8,

		/// <summary>
		/// SyncWriteTransducerSampleDataFromMulitipleChannelsOfATIM (9)
		/// </summary>
		SyncWriteTransducerSampleDataFromMulitipleChannelsOfATIM = 9,

		/// <summary>
		/// SyncWriteTransducerBlockDataFromMulitipleChannelsOfATIM (10)
		/// </summary>
		SyncWriteTransducerBlockDataFromMulitipleChannelsOfATIM = 10,

		/// <summary>
		/// SyncWriteTransducerSampleDataFromMultipleChannelsOfMultipleTIMs (11)
		/// </summary>
		SyncWriteTransducerSampleDataFromMultipleChannelsOfMultipleTIMs = 11,

		/// <summary>
		/// SyncWriteTransducerBlockDataFromMultipleChannelsOfMultipleTIMs (12)
		/// </summary>
		SyncWriteTransducerBlockDataFromMultipleChannelsOfMultipleTIMs = 12,

		/// <summary>
		/// AsyncReadTransducerBlockDataFromAChannelOfATIM (13)
		/// </summary>
		AsyncReadTransducerBlockDataFromAChannelOfATIM = 13,

		/// <summary>
		/// CallbackAsyncReadTransducerBlockDataFromAChannelOfATIM (14)
		/// </summary>
		CallbackAsyncReadTransducerBlockDataFromAChannelOfATIM = 14,

		/// <summary>
		/// AsyncReadTransducerStreamDataFromAChannelOfATIM (15)
		/// </summary>
		AsyncReadTransducerStreamDataFromAChannelOfATIM = 15,

		/// <summary>
		/// CallbackAsyncReadTransducerStreamDataFromAChannelOfATIM (16)
		/// </summary>
		CallbackAsyncReadTransducerStreamDataFromAChannelOfATIM = 16,

		/// <summary>
		/// AsyncReadTransducerBlockDataFromMultipleChannelsOfATIM (17)
		/// </summary>
		AsyncReadTransducerBlockDataFromMultipleChannelsOfATIM = 17,

		/// <summary>
		/// Callback (18)
		/// </summary>
		Callback = 18,

		/// <summary>
		/// AsyncReadTransducerBlockDataFromMultipleChannelOfMultipleTIMs (19)
		/// </summary>
		AsyncReadTransducerBlockDataFromMultipleChannelOfMultipleTIMs = 19,

		/// <summary>
		/// CallbackAsyncReadTransducerBlockDataFromMultipleChannelOfMultipleTIMs (20)
		/// </summary>
		CallbackAsyncReadTransducerBlockDataFromMultipleChannelOfMultipleTIMs = 20
	}
}
