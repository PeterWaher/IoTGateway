using System;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Abstract base class for ranged string operators.
	/// </summary>
	public abstract class StringTagRange : SearchOperator
	{
		private readonly string min;
		private readonly string max;
		private readonly bool minIncluded;
		private readonly bool maxIncluded;

		/// <summary>
		/// Abstract base class for ranged string operators.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Min">Minimum value.</param>
		/// <param name="MinIncluded">If the minimum value is included in the range.</param>
		/// <param name="Max">Maximum value.</param>
		/// <param name="MaxIncluded">If the maximum value is included in the range.</param>
		public StringTagRange(string Name, string Min, bool MinIncluded, string Max, bool MaxIncluded)
			: base(Name)
		{
			this.min = Min;
			this.minIncluded = MinIncluded;
			this.max = Max;
			this.maxIncluded = MaxIncluded;
		}

		/// <summary>
		/// Minimum value.
		/// </summary>
		public string Min => this.min;

		/// <summary>
		/// If the minimum value is included in the range.
		/// </summary>
		public bool MinIncluded => this.minIncluded;

		/// <summary>
		/// Maximum value.
		/// </summary>
		public string Max => this.max;

		/// <summary>
		/// If the maximum value is included in the range.
		/// </summary>
		public bool MaxIncluded => this.maxIncluded;

		internal override void SerializeValue(StringBuilder Request)
		{
			Request.Append("' min='");
			Request.Append(XML.Encode(this.min));
			Request.Append("' minIncluded='");
			Request.Append(CommonTypes.Encode(this.minIncluded));
			Request.Append("' max='");
			Request.Append(XML.Encode(this.max));
			Request.Append("' maxIncluded='");
			Request.Append(CommonTypes.Encode(this.maxIncluded));
		}
	}
}
