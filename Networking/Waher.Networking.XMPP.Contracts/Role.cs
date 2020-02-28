using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Contracts.HumanReadable;

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

	}
}
