using System;

namespace Waher.Security.Users
{
	/// <summary>
	/// Event arguments for events raised when a Legal ID is being updated on a
	/// user object.
	/// </summary>
	public class UpdatingLegalIdEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for events raised when a Legal ID is being updated on a
		/// user object.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="OldLegalId">Old Legal ID</param>
		/// <param name="NewLegalId">New Legal ID</param>
		public UpdatingLegalIdEventArgs(User User, string OldLegalId, string NewLegalId)
		{
			this.OldLegalId = OldLegalId;
			this.NewLegalId = NewLegalId;
			this.User = User;
		}

		/// <summary>
		/// Old Legal ID
		/// </summary>
		public string OldLegalId { get; }

		/// <summary>
		/// New Legal ID
		/// </summary>
		public string NewLegalId { get; }

		/// <summary>
		/// User object.
		/// </summary>
		public User User { get; }
	}
}
