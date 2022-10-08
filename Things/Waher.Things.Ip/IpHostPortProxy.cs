using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content;
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
		private string remoteIpsExpression = string.Empty;
		private string proxyKey = null;
		private bool trustServer = false;
		private bool authorizedAccess = false;
		private IpCidr[] remoteIps = null;

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
		/// If connection is encrypted using TLS or not.
		/// </summary>
		[Page(33, "Security")]
		[Header(34, "Authorized Access Only (mTLS)", 80)]
		[ToolTip(35, "If checked, mTLS will be used, requiring connecting clients to provide a client certificate authorizing access to port.")]
		[DefaultValue(false)]
		public bool AuthorizedAccess
		{
			get { return this.authorizedAccess; }
			set { this.authorizedAccess = value; }
		}

		/// <summary>
		/// If connections should be restricted to the Remote IPs defined by this expression.
		/// IP Addresses are specified using a comma-delimited list of IP Addresses or IP CIDR ranges.
		/// </summary>
		[Page(33, "Security")]
		[Header(36, "Restrict to Remote IPs", 80)]
		[ToolTip(37, "IP Addresses are specified using a comma-delimited list of IP Addresses or IP CIDR ranges.")]
		[RegularExpression(@"\s*\d+\.\d+\.\d+\.\d+(\/\d+)?(\s*,\s*\d+\.\d+\.\d+\.\d+(\/\d+)?)*")]
		[DefaultValueStringEmpty]
		public string RemoteIps
		{
			get { return this.remoteIpsExpression; }
			set 
			{
				if (string.IsNullOrEmpty(value))
					this.remoteIps = null;
				else
				{
					List<IpCidr> Ranges = new List<IpCidr>();
					string[] Parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

					foreach (string Part in Parts)
					{
						if (!IpCidr.TryParse(Part, out IpCidr Parsed))
							throw new Exception("Invalid IP Address or CIDR specification.");

						Ranges.Add(Parsed);
					}

					if (Ranges.Count == 0)
						this.remoteIps = null;
					else
						this.remoteIps = Ranges.ToArray();
				}

				this.remoteIpsExpression = value; 
			}
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
				this.proxyKey = ProxyPorts.GetKey(this.Host, this.Port, this.Tls, this.trustServer, this.listeningPort, this.authorizedAccess, this.remoteIps);

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
			return ProxyPorts.GetProxy(this, this.Key, this.Host, this.Port, this.Tls, this.trustServer, this.listeningPort, this.authorizedAccess, this.remoteIps);
		}

		/// <summary>
		/// Gets names (subject and alternative) encoded in a certificate.
		/// </summary>
		/// <param name="Certificate">Ceertificate</param>
		/// <returns>Encoded names (subject & alternative).</returns>
		public static string[] GetCertificateIdentities(X509Certificate Certificate)
		{
			List<string> Domains = new List<string>();
			bool HasAlternativeNames = false;

			foreach (string Part in Certificate.Subject.Split(certificateSubjectSeparator, StringSplitOptions.None))
			{
				if (Part.StartsWith("CN="))
					Domains.Add(Part.Substring(3));
				else if (Part.StartsWith("SAN="))
				{
					Domains.Add(Part.Substring(4));
					HasAlternativeNames = true;
				}
			}

			if (!HasAlternativeNames)
			{
				if (!(Certificate is X509Certificate2 Cert2))
				{
					byte[] Bin = Certificate.GetRawCertData();
					Cert2 = new X509Certificate2(Bin);
				}

				foreach (X509Extension Extension in Cert2.Extensions)
				{
					if (Extension.Oid.Value == "2.5.29.17")     // Subject Alternative Name
					{
						AsnEncodedData Parsed = new AsnEncodedData(Extension.Oid, Extension.RawData);
						string[] SAN = Parsed.Format(true).Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries);

						foreach (string Name in SAN)
						{
							int i = Name.LastIndexOf('=');
							if (i > 0)
								Domains.Add(Name.Substring(i + 1));
						}
					}
				}
			}

			return Domains.ToArray();
		}

		private readonly static string[] certificateSubjectSeparator = new string[] { ", " };

	}
}
