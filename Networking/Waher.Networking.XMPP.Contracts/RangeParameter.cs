using System;
using System.Threading.Tasks;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Numerical contractual parameter
	/// </summary>
	public abstract class RangeParameter<T> : Parameter
		where T : struct, IComparable<T>
	{
		private T? value;
		private T? min = null;
		private T? max = null;
		private bool minIncluded = true;
		private bool maxIncluded = true;

		/// <summary>
		/// Parameter value
		/// </summary>
		public T? Value
		{
			get => this.value;
			set => this.value = value;
		}

		/// <summary>
		/// Optional minimum value.
		/// </summary>
		public T? Min
		{
			get => this.min;
			set => this.min = value;
		}

		/// <summary>
		/// Optional maximum value.
		/// </summary>
		public T? Max
		{
			get => this.max;
			set => this.max = value;
		}

		/// <summary>
		/// If the optional minimum value is included in the allowed range.
		/// </summary>
		public bool MinIncluded
		{
			get => this.minIncluded;
			set => this.minIncluded = value;
		}

		/// <summary>
		/// If the optional maximum value is included in the allowed range.
		/// </summary>
		public bool MaxIncluded
		{
			get => this.maxIncluded;
			set => this.maxIncluded = value;
		}

		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.value;

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public override void Populate(Variables Variables)
		{
			Variables[this.Name] = this.value;
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <param name="Client">Connected contracts client. If offline or null, partial validation in certain cases will be performed.</param>
		/// <returns>If parameter value is valid.</returns>
		public override Task<bool> IsParameterValid(Variables Variables, ContractsClient Client)
		{
			int Diff;

			if (!(this.Value.HasValue))
				return Task.FromResult(false);

			IComparable<T> Compare = this.Value.Value;

			if (this.Min.HasValue)
			{
				Diff = Compare.CompareTo(this.Min.Value);

				if (Diff < 0 || (Diff == 0 && !this.MinIncluded))
					return Task.FromResult(false);
			}

			if (this.Max.HasValue)
			{
				Diff = Compare.CompareTo(this.Max.Value);

				if (Diff > 0 || (Diff == 0 && !this.MaxIncluded))
					return Task.FromResult(false);
			}

			return base.IsParameterValid(Variables, Client);
		}

	}
}
