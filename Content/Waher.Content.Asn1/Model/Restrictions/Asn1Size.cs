using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// SIZE()
	/// </summary>
	public class Asn1Size : Asn1SetRestriction
	{
		/// <summary>
		/// SIZE()
		/// </summary>
		/// <param name="Set">Set</param>
		public Asn1Size(Asn1Set Set)
			: base(Set)
		{
		}
	}
}
