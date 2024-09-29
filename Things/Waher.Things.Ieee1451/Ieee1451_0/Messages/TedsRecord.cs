namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// Represents one record in a TEDS
	/// </summary>
	public class TedsRecord
	{
		/// <summary>
		/// Represents one record in a TEDS
		/// </summary>
		/// <param name="Type">Record Type</param>
		/// <param name="RawValue">Raw binary value</param>
		public TedsRecord(byte Type, byte[] RawValue)
		{
			this.Type = Type;
			this.RawValue = RawValue;
		}

		/// <summary>
		/// TEDS Record Type
		/// </summary>
		public byte Type { get; set; }

		/// <summary>
		/// TEDS Record value
		/// </summary>
		public byte[] RawValue { get; set; }
	}
}
