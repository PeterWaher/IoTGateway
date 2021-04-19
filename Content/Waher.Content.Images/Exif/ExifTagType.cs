namespace Waher.Content.Images.Exif
{
	/// <summary>
	/// Data types of meta-data tags
	/// </summary>
	public enum ExifTagType
	{
		/// <summary>
		/// An 8-bit unsigned integer.
		/// </summary>
		BYTE = 1,

		/// <summary>
		/// An 8-bit byte containing one 7-bit ASCII code. The final byte is terminated with NULL.
		/// </summary>
		ASCII = 2,

		/// <summary>
		/// A 16-bit (2-byte) unsigned integer
		/// </summary>
		SHORT = 3,

		/// <summary>
		/// A 32-bit (4-byte) unsigned integer
		/// </summary>
		LONG = 4,

		/// <summary>
		/// Two LONGs. The first LONG is the numerator and the second LONG expresses the denominator.
		/// </summary>
		RATIONAL = 5,

		/// <summary>
		/// An 8-bit byte that can take any value depending on the field definition
		/// </summary>
		UNDEFINED = 7,

		/// <summary>
		/// A 32-bit (4-byte) signed integer (2's complement notation)
		/// </summary>
		SLONG = 9,

		/// <summary>
		/// Two SLONGs. The first SLONG is the numerator and the second SLONG is the denominator.
		/// </summary>
		SRATIONAL = 10
	}
}
