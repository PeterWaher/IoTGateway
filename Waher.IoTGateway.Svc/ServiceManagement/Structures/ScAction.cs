using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;

namespace Waher.IoTGateway.Svc.ServiceManagement.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ScAction : IEquatable<ScAction>
	{
		private ScActionType _Type;
		private uint _Delay;

		public ScActionType Type
		{
			get => _Type;
			set => _Type = value;
		}

		public TimeSpan Delay
		{
			get => TimeSpan.FromMilliseconds(_Delay);
			set => _Delay = (uint)Math.Round(value.TotalMilliseconds);
		}

		public bool Equals(ScAction other)
		{
			return _Type == other._Type && _Delay == other._Delay;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is ScAction && Equals((ScAction)obj);
		}

		public override int GetHashCode()
		{
			int h1 = this.Delay.GetHashCode();
			int h2 = this.Type.GetHashCode();

			return ((h1 << 5) + h1) ^ h2;
		}
	}
}
