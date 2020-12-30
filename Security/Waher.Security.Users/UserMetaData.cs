using System;
using Waher.Persistence.Attributes;

namespace Waher.Security.Users
{
	/// <summary>
	/// Contains a piece of meta-data information about a user.
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	public class UserMetaData
	{
		private string name = string.Empty;
		private string value = string.Empty;

		/// <summary>
		/// Contains a piece of meta-data information about a user.
		/// </summary>
		public UserMetaData()
		{
		}

		/// <summary>
		/// Name of meta-data tag.
		/// </summary>
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Value of meta-data tag.
		/// </summary>
		public string Value
		{
			get => this.value;
			set => this.value = value;
		}
	}
}
