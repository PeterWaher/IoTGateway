using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Contains information about a range in a search operation.
	/// </summary>
	public class RangeInfo
	{
		private readonly string fieldName;
		private object min;
		private object max;
		private object point;
		private bool minInclusive;
		private bool maxInclusive;
		private bool isRange;
		private bool isPoint;

		/// <summary>
		/// Contains information about a range in a search operation.
		/// </summary>
		/// <param name="FieldName">Field name being searched.</param>
		public RangeInfo(string FieldName)
		{
			this.fieldName = FieldName;
		}

		/// <summary>
		/// Field Name
		/// </summary>
		public string FieldName
		{
			get { return this.fieldName; }
		}

		/// <summary>
		/// Minimum endpoint, if a range.
		/// </summary>
		public object Min
		{
			get { return this.min; }
		}

		/// <summary>
		/// Maximum endpoint, if a range.
		/// </summary>
		public object Max
		{
			get { return this.max; }
		}

		/// <summary>
		/// Point value, if not a range.
		/// </summary>
		public object Point
		{
			get { return this.point; }
		}

		/// <summary>
		/// If the minimum endpoint is included in the range.
		/// </summary>
		public bool MinInclusive
		{
			get { return this.minInclusive; }
		}

		/// <summary>
		/// If the maximum endpoint is included in the range.
		/// </summary>
		public bool MaxInclusive
		{
			get { return this.maxInclusive; }
		}

		/// <summary>
		/// If the object specifies a range (true) or a single point (false).
		/// </summary>
		public bool IsRange
		{
			get { return this.isRange; }
		}

		/// <summary>
		/// If the object specifies a point (true) or a range (false).
		/// </summary>
		public bool IsPoint
		{
			get { return this.isPoint; }
		}

		/// <summary>
		/// If the range is open-ended.
		/// </summary>
		public bool IsOpenEndedRange
		{
			get { return this.IsRange && (this.min == null || this.max == null); }
		}

		/// <summary>
		/// If the range has a minimum endpoint.
		/// </summary>
		public bool HasMin
		{
			get { return (this.isRange && this.min != null) || (this.isPoint && this.point != null); }
		}

		/// <summary>
		/// If the range has a maximum endpoint.
		/// </summary>
		public bool HasMax
		{
			get { return (this.isRange && this.max != null) || (this.isPoint && this.point != null); }
		}

		/// <summary>
		/// Range consists of a single point value.
		/// </summary>
		/// <param name="Value">Point value.</param>
		/// <returns>If range is consistent.</returns>
		public bool SetPoint(object Value)
		{
			int? i;

			if (this.isRange)
			{
				if (this.min != null)
				{
					i = Comparison.Compare(this.min, Value);

					if (!i.HasValue || i.Value > 0 || (i.Value == 0 && !this.minInclusive))
						return false;

					this.min = null;
				}

				if (this.max != null)
				{
					i = Comparison.Compare(this.max, Value);

					if (!i.HasValue || i.Value < 0 || (i.Value == 0 && !this.maxInclusive))
						return false;

					this.max = null;
				}

				this.point = Value;
				this.isRange = false;
				this.isPoint = true;
				return true;
			}
			else if (this.isPoint)
			{
				i = Comparison.Compare(this.point, Value);
				return (i.HasValue && i.Value == 0);
			}
			else
			{
				this.isPoint = true;
				this.point = Value;
				return true;
			}
		}

		/// <summary>
		/// Sets minimum endpoint of range.
		/// </summary>
		/// <param name="Value">Endpoint value.</param>
		/// <param name="Inclusive">If endpoint is included in range or not.</param>
		/// <param name="Smaller">If the range became smaller.</param>
		/// <returns>If range is consistent.</returns>
		public bool SetMin(object Value, bool Inclusive, out bool Smaller)
		{
			int? i;

			Smaller = false;

			if (this.isRange)
			{
				if (this.min != null)
				{
					i = Comparison.Compare(this.min, Value);

					if (!i.HasValue)
						return false;

					if (i.Value < 0)
					{
						Smaller = true;

						if (!this.minInclusive && Inclusive)
						{
							if (Comparison.Increment(ref this.min))
							{
								i = Comparison.Compare(this.min, Value);
								if (i.HasValue && i.Value == 0)
									Smaller = false;
							}
						}

						this.min = Value;
						this.minInclusive = Inclusive;
					}
					else if (i.Value == 0)
					{
						if (!Inclusive && this.minInclusive)
						{
							this.minInclusive = false;
							Smaller = true;
						}
					}
				}
				else
				{
					this.min = Value;
					this.minInclusive = Inclusive;
					Smaller = true;
				}

				if (this.max != null)
				{
					i = Comparison.Compare(this.max, this.min);
					if (!i.HasValue || i.Value < 0)
						return false;
					else if (i.Value == 0)
					{
						if (!(this.minInclusive && this.maxInclusive))
							return false;

						this.point = Value;
						this.isRange = false;
						this.min = null;
						this.max = null;
					}
				}

				return true;
			}
			else if (this.isPoint)
			{
				i = Comparison.Compare(this.point, Value);

				return !(!i.HasValue || i.Value < 0 || (i.Value == 0 && !Inclusive));
			}
			else
			{
				this.min = Value;
				this.minInclusive = Inclusive;
				this.isRange = true;
				Smaller = true;
				return true;
			}
		}

		/// <summary>
		/// Sets maximum endpoint of range.
		/// </summary>
		/// <param name="Value">Endpoint value.</param>
		/// <param name="Inclusive">If endpoint is included in range or not.</param>
		/// <param name="Smaller">If the range became smaller.</param>
		/// <returns>If range is consistent.</returns>
		public bool SetMax(object Value, bool Inclusive, out bool Smaller)
		{
			int? i;

			Smaller = false;

			if (this.isRange)
			{
				if (this.max != null)
				{
					i = Comparison.Compare(this.max, Value);

					if (!i.HasValue)
						return false;

					if (i.Value > 0)
					{
						Smaller = true;

						if (!this.maxInclusive && Inclusive)
						{
							if (Comparison.Decrement(ref this.max))
							{
								i = Comparison.Compare(this.max, Value);
								if (i.HasValue && i.Value == 0)
									Smaller = false;
							}
						}

						this.max = Value;
						this.maxInclusive = Inclusive;
					}
					else if (i.Value == 0)
					{
						if (!Inclusive && this.maxInclusive)
						{
							this.maxInclusive = false;
							Smaller = true;
						}
					}
				}
				else
				{
					this.max = Value;
					this.maxInclusive = Inclusive;
					Smaller = true;
				}

				if (this.min != null)
				{
					i = Comparison.Compare(this.max, this.min);
					if (!i.HasValue || i.Value < 0)
						return false;
					else if (i.Value == 0)
					{
						if (!(this.minInclusive && this.maxInclusive))
							return false;

						this.point = Value;
						this.isRange = false;
						this.min = null;
						this.max = null;
					}
				}

				return true;
			}
			else if (this.isPoint)
			{
				i = Comparison.Compare(this.point, Value);

				return !(!i.HasValue || i.Value > 0 || (i.Value == 0 && !Inclusive));
			}
			else
			{
				this.max = Value;
				this.maxInclusive = Inclusive;
				this.isRange = true;
				Smaller = true;
				return true;
			}
		}

		/// <summary>
		/// Creates a copy of the range information.
		/// </summary>
		/// <returns>Copy</returns>
		public RangeInfo Copy()
		{
			RangeInfo Result = new RangeInfo(this.fieldName);
			this.CopyTo(Result);
			return Result;
		}

		/// <summary>
		/// Copies the range information to another range object.
		/// </summary>
		/// <param name="Destination">Destination object.</param>
		public void CopyTo(RangeInfo Destination)
		{ 
			Destination.min = this.min;
			Destination.max = this.max;
			Destination.point = this.point;
			Destination.minInclusive = this.minInclusive;
			Destination.maxInclusive = this.maxInclusive;
			Destination.isRange = this.isRange;
			Destination.isPoint = this.isPoint;
		}

	}
}
