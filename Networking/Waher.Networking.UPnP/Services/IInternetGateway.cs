namespace Waher.Networking.UPnP.Services
{
	/// <summary>
	/// Interface for Internet Gateway Services.
	/// </summary>
	public interface IInternetGateway
	{
		/// <summary>
		/// Gets the external public IP address.
		/// </summary>
		/// <param name="NewExternalIPAddress">External IP address.</param>
		void GetExternalIPAddress(out string NewExternalIPAddress);

		/// <summary>
		/// Gets a port mapping entry.
		/// </summary>
		/// <param name="PortMappingIndex">Index of port mapping.</param>
		/// <param name="NewRemoteHost">Remote host.</param>
		/// <param name="NewExternalPort">External port.</param>
		/// <param name="NewProtocol">Protocol.</param>
		/// <param name="NewInternalPort">Internal port.</param>
		/// <param name="NewInternalClient">Internal client.</param>
		/// <param name="NewEnabled">If the port mapping is enabled.</param>
		/// <param name="NewPortMappingDescription">Description of the port mapping.</param>
		/// <param name="NewLeaseDuration">Duration of the lease.</param>
		void GetGenericPortMappingEntry(ushort PortMappingIndex, out string NewRemoteHost,
			out ushort NewExternalPort, out string NewProtocol, out ushort NewInternalPort,
			out string NewInternalClient, out bool NewEnabled,
			out string NewPortMappingDescription, out uint NewLeaseDuration);

		/// <summary>
		/// Deletes a port mapping entry.
		/// </summary>
		/// <param name="NewRemoteHost">Remote host.</param>
		/// <param name="NewExternalPort">External port.</param>
		/// <param name="NewProtocol">Protocol.</param>
		void DeletePortMapping(string NewRemoteHost, ushort NewExternalPort, 
			string NewProtocol);

		/// <summary>
		/// Adds a port mapping entry.
		/// </summary>
		/// <param name="NewRemoteHost">Remote host.</param>
		/// <param name="NewExternalPort">External port.</param>
		/// <param name="NewProtocol">Protocol.</param>
		/// <param name="NewInternalPort">Internal port.</param>
		/// <param name="NewInternalClient">Internal client.</param>
		/// <param name="NewEnabled">If the port mapping is enabled.</param>
		/// <param name="NewPortMappingDescription">Description of the port mapping.</param>
		/// <param name="NewLeaseDuration">Duration of the lease.</param>
		void AddPortMapping(string NewRemoteHost, ushort NewExternalPort,
			string NewProtocol, ushort NewInternalPort, string NewInternalClient,
			bool NewEnabled, string NewPortMappingDescription, uint NewLeaseDuration);

		/// <summary>
		/// Gets a specific port mapping entry.
		/// </summary>
		/// <param name="NewRemoteHost">Remote host.</param>
		/// <param name="NewExternalPort">External port.</param>
		/// <param name="NewProtocol">Protocol.</param>
		/// <param name="NewInternalPort">Internal port.</param>
		/// <param name="NewInternalClient">Internal client.</param>
		/// <param name="NewEnabled">If the port mapping is enabled.</param>
		/// <param name="NewPortMappingDescription">Description of the port mapping.</param>
		/// <param name="NewLeaseDuration">Duration of the lease.</param>
		void GetSpecificPortMappingEntry(string NewRemoteHost, ushort NewExternalPort,
			string NewProtocol, out ushort NewInternalPort, out string NewInternalClient,
			out bool NewEnabled, out string NewPortMappingDescription,
			out uint NewLeaseDuration);
	}
}
