using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.HostSettingObjects
{
	/// <summary>
	/// Base abstract class for host settings.
	/// </summary>
	[TypeName(TypeNameSerialization.LocalName)]
	[CollectionName("HostSettings")]
	[ArchivingTime]		// No Limit
	[Index("Host", "Key")]
	public abstract class HostSetting
	{
		private string objectId = null;
		private string host = string.Empty;
		private string key = string.Empty;

		/// <summary>
		/// Base abstract class for settings.
		/// </summary>
		public HostSetting()
		{
		}

		/// <summary>
		/// Base abstract class for settings.
		/// </summary>
		/// <param name="Host">Host name.</param>
		/// <param name="Key">Key name.</param>
		public HostSetting(string Host, string Key)
		{
			this.host = Host;
			this.key = Key;
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
		/// Host name.
		/// </summary>
		public string Host
		{
			get => this.host;
			set => this.host = value;
		}

		/// <summary>
		/// Key name.
		/// </summary>
		public string Key
		{
			get => this.key;
			set => this.key = value;
		}

		/// <summary>
		/// Gets the value of the setting, as an object.
		/// </summary>
		/// <returns>Value object.</returns>
		public abstract object GetValueObject();
	}
}
