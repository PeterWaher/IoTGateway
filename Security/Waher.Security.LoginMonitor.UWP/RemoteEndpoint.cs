using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Security.LoginMonitor
{
	/// <summary>
	/// Login state information relating to a remote endpoint
	/// </summary>
	[CollectionName("RemoteEndpoints")]
	[TypeName(TypeNameSerialization.None)]
	[ArchivingTime("ArchiveDays")]
	[Index("Endpoint", "Created")]
	public class RemoteEndpoint
	{
		private string objectId = null;
		private string endpoint = null;
		private string lastProtocol = string.Empty;
		private string reason = string.Empty;
		private string whois = string.Empty;
		private string city = string.Empty;
		private string region = string.Empty;
		private string country = string.Empty;
		private string code = string.Empty;
		private string flag = string.Empty;
		private bool blocked = false;
		private int[] state = null;
		private DateTime[] timestamps = null;
		private DateTime created = DateTime.MinValue;

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
		public DateTime Created
		{
			get => this.created;
			set => this.created = value;
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
		/// WHOIS information about the remote endpoint.
		/// </summary>
		[DefaultValueStringEmpty]
		public string WhoIs
		{
			get => this.whois;
			set => this.whois = value;
		}

		/// <summary>
		/// City related to the endpoint.
		/// </summary>
		[DefaultValueStringEmpty]
		public string City
		{
			get => this.city;
			set => this.city = value;
		}

		/// <summary>
		/// Region related to the endpoint.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Region
		{
			get => this.region;
			set => this.region = value;
		}

		/// <summary>
		/// Country related to the endpoint.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Country
		{
			get => this.country;
			set => this.country = value;
		}

		/// <summary>
		/// Country Code related to the endpoint.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Code
		{
			get => this.code;
			set => this.code = value;
		}

		/// <summary>
		/// Flag related to the endpoint.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Flag
		{
			get => this.flag;
			set => this.flag = value;
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

		internal async Task<KeyValuePair<string, object>[]> Annotate(params KeyValuePair<string, object>[] Tags)
		{
			Tags = await LoginAuditor.Annotate(this.endpoint, Tags);

			foreach (KeyValuePair<string, object> Tag in Tags)
			{
				switch (Tag.Key)
				{
					case "City":
						this.city = Tag.Value.ToString();
						break;

					case "Region":
						this.region = Tag.Value.ToString();
						break;

					case "Country":
						this.country = Tag.Value.ToString();
						break;

					case "Code":
						this.code = Tag.Value.ToString();
						break;

					case "Flag":
						this.flag = Tag.Value.ToString();
						break;
				}
			}

			return Tags;
		}

		/// <summary>
		/// Number of days to archive field.
		/// </summary>
		public int ArchiveDays
		{
			get
			{
				return this.blocked ? int.MaxValue : 0;
			}
		}

	}
}
