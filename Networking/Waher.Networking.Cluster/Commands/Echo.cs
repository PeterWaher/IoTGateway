using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.Cluster.Commands
{
	/// <summary>
	/// Command echoing incoming text string.
	/// </summary>
	public class Echo : IClusterCommand
	{
		/// <summary>
		/// Text to echo.
		/// </summary>
		public string Text
		{
			get;
			set;
		}

		/// <summary>
		/// Method called when the command has been received and is to be executed.
		/// </summary>
		/// <param name="LocalEndpoint">Cluster endpoint that received the message.</param>
		/// <param name="RemoteEndpoint">Endpoint sending the message.</param>
		/// <returns>Result of the command.</returns>
		public Task<object> Execute(ClusterEndpoint LocalEndpoint, IPEndPoint RemoteEndpoint)
		{
			return Task.FromResult<object>(this.Text);
		}
	}
}
