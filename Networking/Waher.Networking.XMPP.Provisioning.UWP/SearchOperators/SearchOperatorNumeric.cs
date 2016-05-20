using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Numeric search operator.
	/// </summary>
	public abstract class SearchOperatorNumeric : SearchOperator
	{
		private double value;

		/// <summary>
		/// Numeric search operator.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public SearchOperatorNumeric(string Name, double Value)
			: base(Name)
		{
			this.value = Value;
		}

		internal override void SerializeValue(StringBuilder Request)
		{
			Request.Append("' value='");
			Request.Append(CommonTypes.Encode(this.value));
		}

	}
}
