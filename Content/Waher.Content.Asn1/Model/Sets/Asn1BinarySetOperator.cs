namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Abstract base class of binary set operators
	/// </summary>
	public abstract class Asn1BinarySetOperator : Asn1Values
	{
		private readonly Asn1Values left;
		private readonly Asn1Values right;

		/// <summary>
		/// Abstract base class of binary set operators
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		public Asn1BinarySetOperator(Asn1Values Left, Asn1Values Right)
			: base()
		{
			this.left = Left;
			this.right = Right;
		
			this.SetAsChild(this.left);
			this.SetAsChild(this.right);
		}

		/// <summary>
		/// Left set
		/// </summary>
		public Asn1Values Left => this.left;

		/// <summary>
		/// Right set
		/// </summary>
		public Asn1Values Right => this.right;
	}
}
