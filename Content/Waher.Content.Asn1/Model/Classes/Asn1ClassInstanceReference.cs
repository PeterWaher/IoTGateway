namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Instance of a class.
	/// </summary>
	public class Asn1ClassInstanceReference : Asn1ClassInstanceElement
	{
		private readonly string name;

		/// <summary>
		/// Instance of a class.
		/// </summary>
		/// <param name="Name">Class instance name.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassInstanceReference(string Name, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
		}

		/// <summary>
		/// Class instance name.
		/// </summary>
		public string Name => this.name;

		/// <inheritdoc/>
		public override string ToString() => this.name;
	}
}
