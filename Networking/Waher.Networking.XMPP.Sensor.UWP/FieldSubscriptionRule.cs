using System;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Maintains the status of a field subscription rule.
	/// </summary>
	public class FieldSubscriptionRule
	{
		private readonly string fieldName;
		private object currentValue;
		private readonly double? changedBy;
		private readonly double? changedUp;
		private readonly double? changedDown;
		private readonly bool hasChangeRule;

		/// <summary>
		/// Maintains the status of a field subscription rule.
		/// </summary>
		/// <param name="FieldName">Name of the field.</param>
		public FieldSubscriptionRule(string FieldName)
			: this(FieldName, null, null, null, null)
		{
		}

		/// <summary>
		/// Maintains the status of a field subscription rule.
		/// </summary>
		/// <param name="FieldName">Name of the field.</param>
		/// <param name="CurrentValue">Optional current value of the field, held by the subscribing party.</param>
		/// <param name="ChangedBy">Trigger event if it changes more than this value, in any direction.</param>
		public FieldSubscriptionRule(string FieldName, double? CurrentValue, double? ChangedBy)
			: this(FieldName, CurrentValue.HasValue ? (object)CurrentValue.Value : null, ChangedBy, null, null)
		{
		}

		/// <summary>
		/// Maintains the status of a field subscription rule.
		/// </summary>
		/// <param name="FieldName">Name of the field.</param>
		/// <param name="CurrentValue">Optional current value of the field, held by the subscribing party.</param>
		/// <param name="ChangedBy">Trigger event if it changes more than this value, in any direction.</param>
		public FieldSubscriptionRule(string FieldName, string CurrentValue, double? ChangedBy)
			: this(FieldName, (object)CurrentValue, ChangedBy, null, null)
		{
		}

		/// <summary>
		/// Maintains the status of a field subscription rule.
		/// </summary>
		/// <param name="FieldName">Name of the field.</param>
		/// <param name="CurrentValue">Optional current value of the field, held by the subscribing party.</param>
		/// <param name="ChangedUp">Trigger event if it changes upwards more than this value.</param>
		/// <param name="ChangedDown">Trigger event if it changes downwards more than this value.</param>
		public FieldSubscriptionRule(string FieldName, double? CurrentValue, double? ChangedUp, double? ChangedDown)
			: this(FieldName, CurrentValue.HasValue ? (object)CurrentValue.Value : null, null, ChangedUp, ChangedDown)
		{
		}

		/// <summary>
		/// Maintains the status of a field subscription rule.
		/// </summary>
		/// <param name="FieldName">Name of the field.</param>
		/// <param name="CurrentValue">Optional current value of the field, held by the subscribing party.</param>
		/// <param name="ChangedUp">Trigger event if it changes upwards more than this value.</param>
		/// <param name="ChangedDown">Trigger event if it changes downwards more than this value.</param>
		public FieldSubscriptionRule(string FieldName, string CurrentValue, double? ChangedUp, double? ChangedDown)
			: this(FieldName, (object)CurrentValue, null, ChangedUp, ChangedDown)
		{
		}

		/// <summary>
		/// Maintains the status of a field subscription rule.
		/// </summary>
		/// <param name="FieldName">Name of the field.</param>
		/// <param name="CurrentValue">Optional current value of the field, held by the subscribing party.</param>
		/// <param name="ChangedBy">Trigger event if it changes more than this value, in any direction.</param>
		/// <param name="ChangedUp">Trigger event if it changes upwards more than this value.</param>
		/// <param name="ChangedDown">Trigger event if it changes downwards more than this value.</param>
		internal FieldSubscriptionRule(string FieldName, object CurrentValue, double? ChangedBy, double? ChangedUp, double? ChangedDown)
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
		public string FieldName => this.fieldName;

		/// <summary>
		/// Current value of the field, held by the subscribing party.
		/// </summary>
		public object CurrentValue => this.currentValue;

		/// <summary>
		/// Trigger event if it changes more than this value, in any direction.
		/// </summary>
		public double? ChangedBy => this.changedBy;

		/// <summary>
		/// Trigger event if it changes upwards more than this value.
		/// </summary>
		public double? ChangedUp => this.changedUp;

		/// <summary>
		/// Trigger event if it changes downwards more than this value.
		/// </summary>
		public double? ChangedDown => this.changedDown;

		/// <summary>
		/// If a change rule is defined.
		/// </summary>
		public bool HasChangedRule => this.hasChangeRule;

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

				if (this.currentValue is string s)
					Diff = s.CompareTo((string)NewValue);
				else if (this.currentValue is double d)
					Diff = ((double)NewValue) - d;
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
