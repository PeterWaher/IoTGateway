using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Set of one element.
	/// </summary>
	public class Asn1Element : Asn1Set
	{
		private readonly Asn1Value element;

		/// <summary>
		/// Set of one element.
		/// </summary>
		/// <param name="Element">Element</param>
		public Asn1Element(Asn1Value Element)
		{
			this.element = Element;
		}

		/// <summary>
		/// Element
		/// </summary>
		public Asn1Value Element => this.element;
	}
}
