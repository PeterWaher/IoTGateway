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
		[DefaultValueStringEmpty]
		public string LastProtocol
		{
			get => this.lastProtocol;
			set => this.lastProtocol = value;
		}

		/// <summary>
		/// If endpoint is blocked or not.
		/// </summary>
		[DefaultValue(false)]
		public bool Blocked
		{
			get => this.blocked;
			set => this.blocked = value;
		}

		/// <summary>
		/// Current login state. Null represents no login attempts have been made, or last one successfull.
		/// </summary>
		[DefaultValueNull]
		public int[] State
		{
			get => this.state;
			set => this.state = value;
		}

		/// <summary>
		/// Timestamps of first attempt in each interval. Null represents no login attempts have been made, or last one successfull.
		/// </summary>
		[DefaultValueNull]
		public DateTime[] Timestamps
		{
			get => this.timestamps;
			set => this.timestamps = value;
		}
	}
}
