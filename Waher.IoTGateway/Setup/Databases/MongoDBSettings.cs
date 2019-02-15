using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.Setup.Databases
{
	/// <summary>
	/// MongoDB Settings.
	/// </summary>
	[TypeName(TypeNameSerialization.FullName)]
	public class MongoDBSettings : IDatabaseSettings
	{
		private string host = string.Empty;
		private string database = "IoTGateway";
		private string defaultCollection = "Default";
		private string userName = string.Empty;
		private string password = string.Empty;
		private int? port = null;

		/// <summary>
		/// MongoDB Settings.
		/// </summary>
		public MongoDBSettings()
		{
		}

		/// <summary>
		/// Name of database host, or empty if running on local machine.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Host
		{
			get => this.host;
			set => this.host = value;
		}

		/// <summary>
		/// Port number, or null if standard MongoDB service port.
		/// </summary>
		[DefaultValueNull]
		public int? Port
		{
			get => this.port;
			set => this.port = value;
		}

		/// <summary>
		/// Database name
		/// </summary>
		[DefaultValue("IoTGateway")]
		public string Database
		{
			get => this.database;
			set => this.database = value;
		}

		/// <summary>
		/// Default collection
		/// </summary>
		[DefaultValue("Default")]
		public string DefaultCollection
		{
			get => this.defaultCollection;
			set => this.defaultCollection = value;
		}

		/// <summary>
		/// User Name
		/// </summary>
		[DefaultValueStringEmpty]
		public string UserName
		{
			get => this.userName;
			set => this.userName = value;
		}

		/// <summary>
		/// User Name
		/// </summary>
		[DefaultValueStringEmpty]
		public string Password
		{
			get => this.password;
			set => this.password = value;
		}
	}
}
