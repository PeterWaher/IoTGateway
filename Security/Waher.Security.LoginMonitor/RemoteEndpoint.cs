using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.Security.LoginMonitor
{
	/// <summary>
	/// Login state information relating to a remote endpoint
	/// </summary>
	[CollectionName("RemoteEndpoints")]
	[TypeName(TypeNameSerialization.None)]
	[ArchivingTime]
	[Index("Endpoint", "Created")]
	public class RemoteEndpoint
	{
		private string objectId = null;
		private string endpoint = null;
		private string lastProtocol = string.Empty;
		private string reason = string.Empty;
		private bool blocked = false;
		private int[] state = null;
		private DateTime[] timestamps = null;
		private DateTime creted = DateTime.MinValue;

		/// <summary>
		/// Login state information relating to a remote endpoint
		/// </summary>
		public RemoteEndpoint()
		{
		}

		/* NOTE: No default values are reported in attributes. This is to assure serialized object size do not vary when changed.
		 * These objects might change a lot. By maintaining a constant object size over its lifetime, minimizes the risk of blocks 
		 * being split or joined in the object database during operation. */

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// String-representation of remote endpoint.
		/// </summary>
		public string Endpoint
		{
			get => this.endpoint;
			set => this.endpoint = value;
		}

		/// <summary>
		/// When record was created.
		/// </summary>
		public DateTime Creted
		{
			get => this.creted;
			set => this.creted = value;
		}

		/// <summary>
		/// Last protocol used.
		/// </summary>
		public string LastProtocol
		{
			get => this.lastProtocol;
			set => this.lastProtocol = value;
		}

		/// <summary>
		/// If endpoint is blocked or not.
		/// </summary>
		public bool Blocked
		{
			get => this.blocked;
			set => this.blocked = value;
		}

		/// <summary>
		/// Current login state. Null represents no login attempts have been made, or last one successfull.
		/// </summary>
		public int[] State
		{
			get => this.state;
			set => this.state = value;
		}

		/// <summary>
		/// Timestamps of first attempt in each interval. Null represents no login attempts have been made, or last one successfull.
		/// </summary>
		public DateTime[] Timestamps
		{
			get => this.timestamps;
			set => this.timestamps = value;
		}

		/// <summary>
		/// Reason for blocking the endpoint.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Reason
		{
			get => this.reason;
			set => this.reason = value;
		}

		/// <summary>
		/// Checks if last login attempt was a failed login attempt.
		/// </summary>
		public bool LastFailed
		{
			get
			{
				int i, c = this.state?.Length ?? 0;

				for (i = 0; i < c; i++)
				{
					if (this.state[i] > 0)
						return true;
				}

				return false;
			}
		}

		internal void Reset(bool Unblock)
		{
			int i, c;

			for (i = 0, c = this.state?.Length ?? 0; i < c; i++)
				this.state[i] = 0;

			for (i = 0, c = this.timestamps?.Length ?? 0; i < c; i++)
				this.timestamps[i] = DateTime.MinValue;

			if (Unblock)
			{
				this.blocked = false;
				this.reason = string.Empty;
			}
		}

	}
}
