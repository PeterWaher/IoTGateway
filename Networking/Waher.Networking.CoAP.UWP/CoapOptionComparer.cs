using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP Option Comparer. Can be used to sort options in ascending number order.
	/// </summary>
	public class CoapOptionComparer : IComparer<CoapOption>
	{
		/// <summary>
		/// CoAP Option Comparer. Can be used to sort options in ascending number order.
		/// </summary>
		public CoapOptionComparer()
		{
		}

		/// <summary>
		/// <see cref="IComparer{CoapOption}.Compare(CoapOption, CoapOption)"/>
		/// </summary>
		public int Compare(CoapOption x, CoapOption y)
		{
			int i = x.OptionNumber - y.OptionNumber;
			if (i != 0)
				return i;

			return x.originalOrder - y.originalOrder;
		}
	}
}
