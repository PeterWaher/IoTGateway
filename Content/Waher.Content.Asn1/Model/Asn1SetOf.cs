namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SET OF construct.
	/// </summary>
	public class Asn1SetOf : Asn1Type
	{
		private readonly Asn1Values size;
		private readonly Asn1Type type;

		/// <summary>
		/// Represents a ASN.1 SET OF construct.
		/// </summary>
		/// <param name="Size">Optional SIZE</param>
		/// <param name="Type">Type</param>
		public Asn1SetOf(Asn1Values Size, Asn1Type Type)
			: base()
		{
			this.size = Size;
			this.type = Type;
		}

		/// <summary>
		/// Optional SIZE
		/// </summary>
		public Asn1Values Size => this.size;

		/// <summary>
		/// Type.
		/// </summary>
		public Asn1Type Type => this.type;
	}
}
