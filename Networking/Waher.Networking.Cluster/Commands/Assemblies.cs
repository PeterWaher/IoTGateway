using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Networking.Cluster.Commands
{
	public class Assemblies : IClusterCommand
	{
		/// <summary>
		/// Method called when the command has been received and is to be executed.
		/// </summary>
		/// <param name="LocalEndpoint">Cluster endpoint that received the message.</param>
		/// <param name="RemoteEndpoint">Endpoint sending the message.</param>
		/// <returns>Result of the command.</returns>
		public Task<object> Execute(ClusterEndpoint LocalEndpoint, IPEndPoint RemoteEndpoint)
		{
			List<string> Result = new List<string>();

			foreach (Assembly A in Types.Assemblies)
				Result.Add(A.FullName);

			return Task.FromResult<object>(Result.ToArray());
		}
	}
}
