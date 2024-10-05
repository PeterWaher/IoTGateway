namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// Sample definition data models.
	/// </summary>
	public enum Ieee1451_0SampleDefinitionDataModel
	{
		/// <summary>
		/// N-octet integer (unsigned)
		/// </summary>
		NOctetInteger = 0,

		/// <summary>
		/// Single-precision real
		/// </summary> 
		Float32 = 1,

		/// <summary>
		/// Double-precision real
		/// </summary>
		Double = 2,

		/// <summary>
		/// N-octet fraction (unsigned) 
		/// </summary>
		NOctetFraction = 3,

		/// <summary>
		/// Bit sequence  
		/// </summary>
		BitSequence = 4,

		/// <summary>
		/// Long integer (unsigned) 
		/// </summary>
		LongInteger = 5,

		/// <summary>
		/// Long fraction (unsigned) 
		/// </summary>
		LongFraction = 6,

		/// <summary>
		/// Time of day</summary>			
		TimeInstance = 7
	}

	/// <summary>
	/// Physical Unit exponents.
	/// </summary>
	public struct Ieee1451_0SampleDefinition
	{
		/// <summary>
		/// Data model. (§6.5.2.24)
		/// </summary>
		public Ieee1451_0SampleDefinitionDataModel DataModel;

		/// <summary>
		/// Data model length. (§6.5.2.25)
		/// </summary>
		public byte DataModelLength;

		/// <summary>
		/// Data model significant bits. (§6.5.2.26)
		/// </summary>
		public ushort DataModelSignificantBits;
	}
}
