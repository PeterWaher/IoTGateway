using System;
using System.Text;
using Waher.Content;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Numeric search operator.
	/// </summary>
	public abstract class SearchOperatorNumeric : SearchOperator
	{
		private readonly double value;

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

		/// <summary>
		/// Operator value.
		/// </summary>
		public double Value => this.value;

		internal override void SerializeValue(StringBuilder Request)
		{
			Request.Append("' value='");
			Request.Append(CommonTypes.Encode(this.value));
		}

	}
}
