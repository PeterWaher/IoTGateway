using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Domain Configuration
	/// </summary>
	public abstract class DomainConfiguration : SystemConfiguration 
	{
		private static DomainConfiguration instance = null;

		private string domain = string.Empty;
		private bool localMachine = true;

		/// <summary>
		/// Current instance of configuration.
		/// </summary>
		public static DomainConfiguration Instance => instance;

		/// <summary>
		/// Host to connect to
		/// </summary>
		[DefaultValueStringEmpty]
		public string Domain
		{
			get { return this.domain; }
			set { this.domain = value; }
		}

		/// <summary>
		/// If the server is to be a local machine (true) or a public machine with a domain (false).
		/// </summary>
		[DefaultValue(true)]
		public bool LocalMachine
		{
			get { return this.localMachine; }
			set { this.localMachine = value; }
		}

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/Domain.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => 100;

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public override Task ConfigureSystem()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public override void SetStaticInstance(ISystemConfiguration Configuration)
		{
			instance = Configuration as DomainConfiguration;
		}

	}
}
