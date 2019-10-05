using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Interval of elements.
	/// </summary>
	public class Asn1Interval : Asn1Set
	{
		private readonly Asn1Value from;
		private readonly Asn1Value to;

		/// <summary>
		/// Interval of elements.
		/// </summary>
		/// <param name="From">From</param>
		/// <param name="To">To</param>
		public Asn1Interval(Asn1Value From, Asn1Value To)
		{
			this.from = From;
			this.to = To;
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
