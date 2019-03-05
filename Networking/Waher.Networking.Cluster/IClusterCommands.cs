using System;
using System.Net;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Interface for cluster commands.
	/// </summary>
	public interface IClusterCommand : IClusterObject
	{
		/// <summary>
		/// Method called when the command has been received and is to be executed.
		/// </summary>
		/// <param name="LocalEndpoint">Cluster endpoint that received the message.</param>
		/// <param name="RemoteEndpoint">Endpoint sending the message.</param>
		/// <returns>Result of the command.</returns>
		Task<object> Execute(ClusterEndpoint LocalEndpoint, IPEndPoint RemoteEndpoint);
	}
}
