using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// FROM()
	/// </summary>
	public class Asn1From : Asn1SetRestriction
	{
		/// <summary>
		/// FROM()
		/// </summary>
		/// <param name="Set">Set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1From(Asn1SetOfValues Set, Asn1Document Document)
			: base(Set, Document)
		{
		}
	}
}
