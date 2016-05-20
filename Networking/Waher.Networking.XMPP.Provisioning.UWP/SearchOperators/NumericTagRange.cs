using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Abstract base class for ranged numeric operators.
	/// </summary>
	public abstract class NumericTagRange : SearchOperator
	{
		private double min;
		private double max;
		private bool minIncluded;
		private bool maxIncluded;

		/// <summary>
		/// Abstract base class for ranged numeric operators.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Min">Minimum value.</param>
		/// <param name="MinIncluded">If the minimum value is included in the range.</param>
		/// <param name="Max">Maximum value.</param>
		/// <param name="MaxIncluded">If the maximum value is included in the range.</param>
		public NumericTagRange(string Name, double Min, bool MinIncluded, double Max, bool MaxIncluded)
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
		public double Min
		{
			get { return this.min; }
		}

		/// <summary>
		/// If the minimum value is included in the range.
		/// </summary>
		public bool MinIncluded
		{
			get { return this.minIncluded; }
		}

		/// <summary>
		/// Maximum value.
		/// </summary>
		public double Max
		{
			get { return this.max; }
		}

		/// <summary>
		/// If the maximum value is included in the range.
		/// </summary>
		public bool MaxIncluded
		{
			get { return this.maxIncluded; }
		}

		internal override void SerializeValue(StringBuilder Request)
		{
			Request.Append("' min='");
			Request.Append(CommonTypes.Encode(this.min));
			Request.Append("' minIncluded='");
			Request.Append(CommonTypes.Encode(this.minIncluded));
			Request.Append("' max='");
			Request.Append(CommonTypes.Encode(this.max));
			Request.Append("' maxIncluded='");
			Request.Append(CommonTypes.Encode(this.maxIncluded));
		}
	}
}
