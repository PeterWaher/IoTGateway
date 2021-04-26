using System;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Class defining a role
	/// </summary>
	public class Role : LocalizableDescription
	{
		private string name = string.Empty;
		private int minCount = 0;
		private int maxCount = 0;
		private bool canRevoke = false;

		/// <summary>
		/// Name of the role.
		/// </summary>
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Smallest amount of signatures of this role required for a legally binding contract.
		/// </summary>
		public int MinCount
		{
			get => this.minCount;
			set => this.minCount = value;
		}

		/// <summary>
		/// Largest amount of signatures of this role required for a legally binding contract.
		/// </summary>
		public int MaxCount
		{
			get => this.maxCount;
			set => this.maxCount = value;
		}

		/// <summary>
		/// If parts having this role, can revoke their signature, once signed.
		/// </summary>
		public bool CanRevoke
		{
			get => this.canRevoke;
			set => this.canRevoke = value;
		}

	}
}
