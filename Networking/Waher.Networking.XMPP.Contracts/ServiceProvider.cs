namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contains information about a service provider.
	/// </summary>
	public class ServiceProvider : IServiceProvider
	{
		/// <summary>
		/// Contains information about a service provider.
		/// </summary>
		public ServiceProvider()
			: this(string.Empty, string.Empty, string.Empty, string.Empty, 0, 0)
		{
		}

		/// <summary>
		/// Contains information about a service provider.
		/// </summary>
		/// <param name="Id">ID of service provider.</param>
		/// <param name="Type">Type of service provider.</param>
		/// <param name="Name">Displayable name of service provider.</param>
		public ServiceProvider(string Id, string Type, string Name)
			: this(Id, Type, Name, string.Empty, 0, 0)
		{
		}

		/// <summary>
		/// Contains information about a service provider.
		/// </summary>
		/// <param name="Id">ID of service provider.</param>
		/// <param name="Type">Type of service provider.</param>
		/// <param name="Name">Displayable name of service provider.</param>
		/// <param name="IconUrl">Optional URL to icon of service provider.</param>
		/// <param name="IconWidth">Width of icon, if available.</param>
		/// <param name="IconHeight">Height of icon, if available.</param>
		public ServiceProvider(string Id, string Type, string Name, string IconUrl, int IconWidth, int IconHeight)
		{
			this.Id = Id;
			this.Type = Type;
			this.Name = Name;
			this.IconUrl = IconUrl;
			this.IconWidth = IconWidth;
			this.IconHeight = IconHeight;
		}

		/// <summary>
		/// ID of service provider.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Type of service provider.
		/// </summary>
		public string Type { get; }

		/// <summary>
		/// Displayable name of service provider.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Optional URL to icon of service provider.
		/// </summary>
		public string IconUrl { get; }

		/// <summary>
		/// Width of icon, if available.
		/// </summary>
		public int IconWidth { get; }

		/// <summary>
		/// Height of icon, if available.
		/// </summary>
		public int IconHeight { get; }

		/// <summary>
		/// Creates a new instance of the service provider.
		/// </summary>
		/// <param name="Id">ID of service provider.</param>
		/// <param name="Type">Type of service provider.</param>
		/// <param name="Name">Displayable name of service provider.</param>
		public virtual IServiceProvider Create(string Id, string Type, string Name)
		{
			return new ServiceProvider(Id, Type, Name);
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
		public virtual IServiceProvider Create(string Id, string Type, string Name, string IconUrl, int IconWidth, int IconHeight)
		{
			return new ServiceProvider(Id, Type, Name, IconUrl, IconWidth, IconHeight);
		}
	}
}
