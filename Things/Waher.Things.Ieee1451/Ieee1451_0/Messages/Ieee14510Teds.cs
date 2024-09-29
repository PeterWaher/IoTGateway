namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 TEDS
	/// </summary>
	public class Ieee14510Teds
	{
		/// <summary>
		/// IEEE 1451.0 TEDS
		/// </summary>
		/// <param name="Records">TLV records available in TEDS</param>
		public Ieee14510Teds(TedsRecord[] Records)
		{
			this.Records = Records;
		}

		/// <summary>
		/// TLV records available in TEDS
		/// </summary>
		public TedsRecord[] Records { get; }
	}
}
