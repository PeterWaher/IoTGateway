using System;
using System.Collections.Generic;
using System.Text;
using Waher.IoTGateway.Svc.ServiceManagement.Structures;

namespace Waher.IoTGateway.Svc.ServiceManagement.Classes
{
	/// <inheritdoc />
	/// <summary>
	/// A managed class that holds data referring to a <see cref="T:DasMulli.Win32.ServiceUtils.ServiceFailureActionsInfo" /> class which has unmanaged resources
	/// </summary>
	public class ServiceFailureActions : IEquatable<ServiceFailureActions>
	{
		public TimeSpan ResetPeriod { get; }
		public string RebootMessage { get; }
		public string RestartCommand { get; }
		public IReadOnlyCollection<ScAction> Actions { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceFailureActions"/> class.
		/// </summary>
		public ServiceFailureActions(TimeSpan resetPeriod, string rebootMessage, string restartCommand, IReadOnlyCollection<ScAction> actions)
		{
			ResetPeriod = resetPeriod;
			RebootMessage = rebootMessage;
			RestartCommand = restartCommand;
			Actions = actions;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is ServiceFailureActions && Equals((ServiceFailureActions)obj);
		}


		public override int GetHashCode()
		{
			int h1 = this.ResetPeriod.GetHashCode();
			int h2 = this.RebootMessage.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			h2 = this.RestartCommand.GetHashCode();
			h1 = ((h1 << 5) + h1) ^ h2;

			foreach (ScAction Action in this.Actions)
			{
				h2 = Action.GetHashCode();
				h1 = ((h1 << 5) + h1) ^ h2;
			}

			return h1;
		}

		public bool Equals(ServiceFailureActions other)
		{
			if (other is null)
			{
				return false;
			}
			return this.GetHashCode() == other.GetHashCode();
		}
	}
}
