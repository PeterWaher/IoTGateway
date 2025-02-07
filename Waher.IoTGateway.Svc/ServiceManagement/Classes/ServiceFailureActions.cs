using System;
using System.Collections.Generic;
using System.Text;
using Waher.IoTGateway.Svc.ServiceManagement.Structures;

namespace Waher.IoTGateway.Svc.ServiceManagement.Classes
{
	/// <summary>
	/// A managed class that holds data referring to a <see cref="T:DasMulli.Win32.ServiceUtils.ServiceFailureActionsInfo" /> class which has unmanaged resources
	/// </summary>
	/// <param name="resetPeriod">Reset period</param>
	/// <param name="rebootMessage">Reboot message</param>
	/// <param name="restartCommand">Restart command</param>
	/// <param name="actions">Actions</param>
	public class ServiceFailureActions(TimeSpan resetPeriod, string rebootMessage,
		string restartCommand, IReadOnlyCollection<ScAction> actions) 
		: IEquatable<ServiceFailureActions>
	{
		/// <summary>
		/// Reset period
		/// </summary>
		public TimeSpan ResetPeriod { get; } = resetPeriod;

		/// <summary>
		/// Reboot message
		/// </summary>
		public string RebootMessage { get; } = rebootMessage;

		/// <summary>
		/// Restart command
		/// </summary>
		public string RestartCommand { get; } = restartCommand;

		/// <summary>
		/// Actions
		/// </summary>
		public IReadOnlyCollection<ScAction> Actions { get; } = actions;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;

			return obj is ServiceFailureActions Typed && this.Equals(Typed);
		}


		/// <inheritdoc/>
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

		/// <summary>
		/// Compares the instance with another.
		/// </summary>
		/// <param name="Other">Other instance</param>
		/// <returns>If they are equal</returns>
		public bool Equals(ServiceFailureActions Other)
		{
			if (Other is null)
				return false;

			return this.GetHashCode() == Other.GetHashCode();
		}
	}
}
