using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.SettingObjects
{
	/// <summary>
	/// Base abstract class for settings.
	/// </summary>
	[TypeName(TypeNameSerialization.LocalName)]
	[CollectionName("Settings")]
	[Index("Key")]
	public abstract class Setting
	{
		private string objectId = null;
		private string key = string.Empty;

		/// <summary>
		/// Base abstract class for settings.
		/// </summary>
		public Setting()
		{
		}

		/// <summary>
		/// Base abstract class for settings.
		/// </summary>
		/// <param name="Key">Key name.</param>
		public Setting(string Key)
		{
			this.key = Key;
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// Key name.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Key
		{
			get { return this.key; }
			set { this.key = value; }
		}
	}
}
