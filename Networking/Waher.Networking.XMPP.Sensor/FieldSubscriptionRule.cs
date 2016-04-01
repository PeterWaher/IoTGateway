using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Maintains the status of a field subscription rule.
	/// </summary>
	public class FieldSubscriptionRule
	{
		private string fieldName;
		private object currentValue;
		private double? changedBy;
		private double? changedUp;
		private double? changedDown;
		private bool hasChangeRule;

		/// <summary>
		/// Maintains the status of a field subscription rule.
		/// </summary>
		/// <param name="FieldName">Name of the field.</param>
		/// <param name="CurrentValue">Current value of the field, held by the subscribing party.</param>
		/// <param name="ChangedBy">Trigger event if it changes more than this value, in any direction.</param>
		/// <param name="ChangedUp">Trigger event if it changes upwards more than this value.</param>
		/// <param name="ChangedDown">Trigger event if it changes downwards more than this value.</param>
		public FieldSubscriptionRule(string FieldName, object CurrentValue, double? ChangedBy, double? ChangedUp, double? ChangedDown)
		{
			this.fieldName = FieldName;
			this.currentValue = CurrentValue;
			this.changedBy = ChangedBy;
			this.changedUp = ChangedUp;
			this.changedDown = ChangedDown;

			this.hasChangeRule = this.changedBy.HasValue || this.changedUp.HasValue || this.changedDown.HasValue;
		}

		/// <summary>
		/// Name of the field.
		/// </summary>
		public string FieldName
		{
			get { return this.fieldName; }
		}

		/// <summary>
		/// Current value of the field, held by the subscribing party.
		/// </summary>
		public object CurrentValue
		{
			get { return this.currentValue; }
		}

		/// <summary>
		/// Trigger event if it changes more than this value, in any direction.
		/// </summary>
		public double? ChangedBy
		{
			get { return this.changedBy; }
		}

		/// <summary>
		/// Trigger event if it changes upwards more than this value.
		/// </summary>
		public double? ChangedUp
		{
			get { return this.changedUp; }
		}

		/// <summary>
		/// Trigger event if it changes downwards more than this value.
		/// </summary>
		public double? ChangedDown
		{
			get { return this.changedDown; }
		}

		/// <summary>
		/// Checks if a new value triggers an event to the subscribing party.
		/// </summary>
		/// <param name="NewValue">Newly measured value.</param>
		/// <returns>If the rule triggers an event.</returns>
		public bool TriggerEvent(object NewValue)
		{
			if (this.currentValue != null && this.currentValue.GetType() == NewValue.GetType())
			{
				double Diff;

				if (this.currentValue is string)
					Diff = ((string)this.currentValue).CompareTo((string)NewValue);
				else if (this.currentValue is double)
					Diff = ((double)NewValue) - ((double)this.currentValue);
				else
				{
					this.currentValue = NewValue;
					return false;
				}

				if ((this.changedBy.HasValue && Math.Abs(Diff) >= this.changedBy.Value) ||
					(this.changedUp.HasValue && Diff >= this.changedUp.Value) ||
					(this.changedDown.HasValue && -Diff >= this.changedDown) ||
					(!this.changedBy.HasValue && !this.changedUp.HasValue && !this.changedDown.HasValue && !this.currentValue.Equals(NewValue)))
				{
					this.currentValue = NewValue;
					return true;
				}
				else
					return false;
			}
			else
			{
				this.currentValue = NewValue;
				return true;
			}
		}
	}
}
