namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Interface for information about a service provider.
	/// </summary>
	public interface IServiceProvider
	{
		/// <summary>
		/// ID of service provider.
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Type of service provider.
		/// </summary>
		string Type { get; }

		/// <summary>
		/// Displayable name of service provider.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Optional URL to icon of service provider.
		/// </summary>
		string IconUrl { get; }

		/// <summary>
		/// Width of icon, if available.
		/// </summary>
		int IconWidth { get; }

		/// <summary>
		/// Height of icon, if available.
		/// </summary>
		int IconHeight { get; }

		/// <summary>
		/// Creates a new instance of the service provider.
		/// </summary>
		/// <param name="Id">ID of service provider.</param>
		/// <param name="Type">Type of service provider.</param>
		/// <param name="Name">Displayable name of service provider.</param>
		IServiceProvider Create(string Id, string Type, string Name);

		/// <summary>
		/// Creates a new instance of the service provider.
		/// </summary>
		/// <param name="Id">ID of service provider.</param>
		/// <param name="Type">Type of service provider.</param>
		/// <param name="Name">Displayable name of service provider.</param>
		/// <param name="IconUrl">Optional URL to icon of service provider.</param>
		/// <param name="IconWidth">Width of icon, if available.</param>
		/// <param name="IconHeight">Height of icon, if available.</param>
		IServiceProvider Create(string Id, string Type, string Name, string IconUrl, int IconWidth, int IconHeight);
	}
}
