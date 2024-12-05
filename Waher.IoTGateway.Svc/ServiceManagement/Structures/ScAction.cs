using System;
using System.Runtime.InteropServices;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;

namespace Waher.IoTGateway.Svc.ServiceManagement.Structures
{
	/// <summary>
	/// Service controller action
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ScAction : IEquatable<ScAction>
	{
		private ScActionType _Type;
		private uint _Delay;

		/// <summary>
		/// Action type
		/// </summary>
		public ScActionType Type
		{
			get => this._Type;
			set => this._Type = value;
		}

		/// <summary>
		/// Delay
		/// </summary>
		public TimeSpan Delay
		{
			get => TimeSpan.FromMilliseconds(this._Delay);
			set => this._Delay = (uint)Math.Round(value.TotalMilliseconds);
		}

		/// <summary>
		/// Compares to instances
		/// </summary>
		/// <param name="Other">Other instance</param>
		/// <returns>If they are equal</returns>
		public bool Equals(ScAction Other)
		{
			return this._Type == Other._Type && 
				this._Delay == Other._Delay;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;

			return obj is ScAction Typed && this.Equals(Typed);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int h1 = this.Delay.GetHashCode();
			int h2 = this.Type.GetHashCode();

			return ((h1 << 5) + h1) ^ h2;
		}
	}
}
