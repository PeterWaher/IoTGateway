using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ip.Model;

namespace Waher.Things.Ip
{
	public class IpHostPortProxy : IpHostPort
	{
		private int listeningPort = 0;
		private string proxyKey = null;
		private bool trustServer = false;

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(false);
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(IpHostPort), 24, "Proxy Port");
		}

		/// <summary>
		/// If connection is encrypted using TLS or not.
		/// </summary>
		[Page(1, "IP")]
		[Header(28, "Trust Server", 80)]
		[ToolTip(29, "Check if Server Certificate should be trusted.")]
		public bool TrustServer
		{
			get { return this.trustServer; }
			set { this.trustServer = value; }
		}

		/// <summary>
		/// Incoming Port number.
		/// </summary>
		[Page(1, "IP")]
		[Header(25, "Listening Port Number:", 90)]
		[ToolTip(26, "Port number to listen on for incoming conncetions.")]
		[DefaultValue(0)]
		[Range(0, 65535)]
		public int ListeningPort
		{
			get { return this.listeningPort; }
			set { this.listeningPort = value; }
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public async override Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new Int32Parameter("ListeningPort", await Language.GetStringAsync(typeof(IpHost), 27, "Listening Port"), this.listeningPort));

			ProxyPort Proxy = await this.GetProxy();

			Result.AddLast(new Int32Parameter("NrConnections", await Language.GetStringAsync(typeof(IpHost), 32, "#Connections"), Proxy.NrConnctions));
			Result.AddLast(new Int64Parameter("NrUplink", await Language.GetStringAsync(typeof(IpHost), 30, "#Bytes Up"), Proxy.NrBytesUplink));
			Result.AddLast(new Int64Parameter("NrDownlink", await Language.GetStringAsync(typeof(IpHost), 31, "#Bytes Down"), Proxy.NrBytesDownlink));

			return Result;
		}
		
		public override Task DestroyAsync()
		{
			if (!string.IsNullOrEmpty(this.proxyKey))
				ProxyPorts.DestroyProxy(this.proxyKey);

			return base.DestroyAsync();
		}

		[IgnoreMember]
		public string Key
		{
			get
			{
				string PrevKey = this.proxyKey;
				this.proxyKey = ProxyPorts.GetKey(this.Host, this.Port, this.Tls, this.trustServer, this.listeningPort);

				if (PrevKey != this.proxyKey && !string.IsNullOrEmpty(PrevKey))
					ProxyPorts.DestroyProxy(PrevKey);

				return this.proxyKey;
			}
		}

		protected override async Task NodeUpdated()
		{
			await this.GetProxy();
			await base.NodeUpdated();
		}

		internal Task<ProxyPort> GetProxy()
		{
			return ProxyPorts.GetProxy(this, this.Key, this.Host, this.Port, this.Tls, this.trustServer, this.listeningPort);
		}

	}
}
