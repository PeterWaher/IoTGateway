namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contains information about a service provider with a legal identity.
	/// </summary>
	public class ServiceProviderWithLegalId : ServiceProvider
	{
		/// <summary>
		/// Contains information about a service provider with a legal identity.
		/// </summary>
		public ServiceProviderWithLegalId()
			: base()
		{
		}

		/// <summary>
		/// Contains information about a service provider with a legal identity.
		/// </summary>
		/// <param name="Id">ID of service provider.</param>
		/// <param name="Type">Type of service provider.</param>
		/// <param name="Name">Displayable name of service provider.</param>
		/// <param name="LegalId">Legal Identity</param>
		/// <param name="External">If legal identity is external (true) or belongs to the server (false).</param>
		public ServiceProviderWithLegalId(string Id, string Type, string Name, string LegalId, bool External)
			: base(Id, Type, Name)
		{
			this.LegalId = LegalId;
			this.External = External;
		}

		/// <summary>
		/// Contains information about a service provider with a legal identity.
		/// </summary>
		/// <param name="Id">ID of service provider.</param>
		/// <param name="Type">Type of service provider.</param>
		/// <param name="Name">Displayable name of service provider.</param>
		/// <param name="IconUrl">Optional URL to icon of service provider.</param>
		/// <param name="IconWidth">Width of icon, if available.</param>
		/// <param name="IconHeight">Height of icon, if available.</param>
		/// <param name="LegalId">Legal Identity</param>
		/// <param name="External">If legal identity is external (true) or belongs to the server (false).</param>
		public ServiceProviderWithLegalId(string Id, string Type, string Name, string LegalId, bool External,
			string IconUrl, int IconWidth, int IconHeight)
			: base(Id, Type, Name, IconUrl, IconWidth, IconHeight)
		{
			this.LegalId = LegalId;
			this.External = External;
		}

		/// <summary>
		/// Legal identity
		/// </summary>
		public string LegalId { get; }

		/// <summary>
		/// If legal identity is external (true) or belongs to the server (false).
		/// </summary>
		public bool External { get; }

		/// <summary>
		/// Creates a new instance of the service provider.
		/// </summary>
		/// <param name="Id">ID of service provider.</param>
		/// <param name="Type">Type of service provider.</param>
		/// <param name="Name">Displayable name of service provider.</param>
		/// <param name="LegalId">Legal Identity</param>
		/// <param name="External">If legal identity is external (true) or belongs to the server (false).</param>
		public virtual IServiceProvider Create(string Id, string Type, string Name, string LegalId, bool External)
		{
			return new ServiceProviderWithLegalId(Id, Type, Name, LegalId, External);
		}

		/// <summary>
		/// Creates a new instance of the service provider.
		/// </summary>
		/// <param name="Id">ID of service provider.</param>
		/// <param name="Type">Type of service provider.</param>
		/// <param name="Name">Displayable name of service provider.</param>
		/// <param name="IconUrl">Optional URL to icon of service provider.</param>
		/// <param name="IconWidth">Width of icon, if available.</param>
		/// <param name="IconHeight">Height of icon, if available.</param>
		/// <param name="LegalId">Legal Identity</param>
		/// <param name="External">If legal identity is external (true) or belongs to the server (false).</param>
		public virtual IServiceProvider Create(string Id, string Type, string Name, string LegalId, bool External, 
			string IconUrl, int IconWidth, int IconHeight)
		{
			return new ServiceProviderWithLegalId(Id, Type, Name, LegalId, External, IconUrl, IconWidth, IconHeight);
		}
	}
}
