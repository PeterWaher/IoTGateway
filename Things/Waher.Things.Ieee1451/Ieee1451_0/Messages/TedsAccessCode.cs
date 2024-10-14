namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// TEDS Access Code
	/// </summary>
	public enum TedsAccessCode
	{
		/// <summary>
		/// Meta-TEDS
		/// </summary>
		MetaTEDS = 1,

		/// <summary>
		/// MetaIdentification TEDS
		/// </summary>
		MetaIdTEDS = 2,

		/// <summary>
		/// TransducerChannel TEDS
		/// </summary>
		ChanTEDS = 3,

		/// <summary>
		/// TransducerChannel Identification TEDS
		/// </summary>
		ChanIdTEDS = 4,

		/// <summary>
		/// Calibration TEDS
		/// </summary>
		CalTEDS = 5,

		/// <summary>
		/// Calibration identification TEDS
		/// </summary>
		CalIdTEDS = 6,

		/// <summary>
		/// End users’ application-specific TEDS
		/// </summary>
		EUASTEDS = 7,

		/// <summary>
		/// Frequency response TEDS
		/// </summary>
		FreqRespTEDS = 8,

		/// <summary>
		/// Transfer function TEDS
		/// </summary>
		TransferTEDS = 9,

		/// <summary>
		/// Commands TEDS
		/// </summary>
		CommandTEDS = 10,

		/// <summary>
		/// Location and title TEDS
		/// </summary>
		TitleTEDS = 11,

		/// <summary>
		/// User’s transducer name TEDS
		/// </summary>
		XdcrName = 12,

		/// <summary>
		/// PHY TEDS
		/// </summary>
		PHYTEDS = 13,

		/// <summary>
		/// Geographic location TEDS
		/// </summary>
		GeoLocTEDS = 14,

		/// <summary>
		/// Units extension TEDS
		/// </summary>
		UnitsExtension = 15,

		/// <summary>
		/// Security TEDS
		/// </summary>
		SecurityTEDS = 16,

		/// <summary>
		/// Time Synchronous TEDS
		/// </summary>
		TimeSyncTEDS = 17
	}
}
