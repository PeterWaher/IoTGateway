namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Interval of elements.
	/// </summary>
	public class Asn1Interval : Asn1Values
	{
		private readonly Asn1Value from;
		private readonly Asn1Value to;

		/// <summary>
		/// Interval of elements.
		/// </summary>
		/// <param name="From">From</param>
		/// <param name="To">To</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Interval(Asn1Value From, Asn1Value To, Asn1Document Document)
			: base(Document)
		{
			this.from = From;
			this.to = To;
	
			this.SetAsChild(this.from);
			this.SetAsChild(this.to);
		}

		/// <summary>
		/// From
		/// </summary>
		public Asn1Value From => this.from;

		/// <summary>
		/// To
		/// </summary>
		public Asn1Value To => this.to;
	}
}
