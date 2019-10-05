using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// ALL EXCEPT
	/// </summary>
	public class Asn1Except : Asn1Restriction
	{
		private readonly Asn1Node exception;

		/// <summary>
		/// ALL EXCEPT
		/// </summary>
		/// <param name="Exception">Exception</param>
		public Asn1Except(Asn1Node Exception)
		{
			this.exception = Exception;
		}

		/// <summary>
		/// Exception
		/// </summary>
		public Asn1Node Exception => this.exception;
	}
}
