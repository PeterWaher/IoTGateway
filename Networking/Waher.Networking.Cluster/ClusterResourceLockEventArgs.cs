using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Event arguments for cluster resource lock events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event handler.</param>
	public delegate void ClusterResourceLockEventHandler(object Sender, ClusterResourceLockEventArgs e);

	/// <summary>
	/// Event arguments for cluster resource lock events.
	/// </summary>
	public class ClusterResourceLockEventArgs : ClusterResourceEventArgs
	{
		private readonly IPEndPoint lockedBy;
		private readonly bool lockSuccessful;

		/// <summary>
		/// Event arguments for cluster resource lock events.
		/// </summary>
		/// <param name="Resource">Resource name</param>
		/// <param name="LockSuccessful">If lock-operation was successful.</param>
		/// <param name="LockedBy">Identity of endpoint having the resource locked, if not successful.</param>
		/// <param name="State">State object passed on to the original request.</param>
		public ClusterResourceLockEventArgs(string Resource, bool LockSuccessful,
			IPEndPoint LockedBy, object State)
			: base(Resource, State)
		{
			this.lockedBy = LockedBy;
			this.lockSuccessful = LockSuccessful;
		}

		/// <summary>
		/// If the lock operation was successful or not. If not, 
		/// <see cref="LockedBy"/> contains information about what
		/// endpoint has the resource locked.
		/// </summary>
		public bool LockSuccessful => this.lockSuccessful;

		/// <summary>
		/// If lock was not successful, this is the endpoint that has the
		/// resource locked. If null, and <see cref="LockSuccessful"/> is false,
		/// it is the same machine that has the resource already locked.
		/// </summary>
		public IPEndPoint LockedBy => this.lockedBy;
	}
}
