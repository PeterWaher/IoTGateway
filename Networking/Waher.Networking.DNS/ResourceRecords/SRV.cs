using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Waher.Networking.DNS.Communication;
using Waher.Networking.DNS.Enumerations;
using Waher.Persistence.Attributes;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Server Selection
	/// </summary>
	public class SRV : ResourceRecord
	{
		private static readonly Regex srvName = new Regex("^_(?'Service'[^.]*)[.]_(?'Protocol'[^.]*)[.](?'Name'.*)$", RegexOptions.Singleline | RegexOptions.Compiled);

		private string service;
		private string protocol;
		private ushort priority;
		private ushort weight;
		private ushort port;
		private string targetHost;

		/// <summary>
		/// Server Selection
		/// </summary>
		public SRV()
			: base()
		{
			this.service = string.Empty;
			this.protocol = string.Empty;
			this.priority = 0;
			this.weight = 0;
			this.port = 0;
			this.targetHost = string.Empty;
		}

		/// <summary>
		/// Server Selection
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		public SRV(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data)
			: base(Name, Type, Class, Ttl)
		{
			Match M;

			lock (srvName)
			{
				M = srvName.Match(Name);
			}

			if (!M.Success || M.Index > 0 || M.Length < Name.Length)
				throw new ArgumentException("Invalid service name.", nameof(Name));

			this.service = M.Groups["Service"].Value;
			this.protocol = M.Groups["Protocol"].Value;
			this.Name = M.Groups["Name"].Value;

			this.priority = DnsClient.ReadUInt16(Data);
			this.weight = DnsClient.ReadUInt16(Data);
			this.port = DnsClient.ReadUInt16(Data);
			this.targetHost = DnsClient.ReadName(Data);
		}

		/// <summary>
		/// Service name.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Service
		{
			get => this.service;
			set => this.service = value;
		}

		/// <summary>
		/// Protocol name
		/// </summary>
		[DefaultValueStringEmpty]
		public string Protocol
		{
			get => this.protocol;
			set => this.protocol = value;
		}

		/// <summary>
		/// Priority
		/// </summary>
		[DefaultValue((ushort)0)]
		public ushort Priority
		{
			get => this.priority;
			set => this.priority = value;
		}

		/// <summary>
		/// Weight
		/// </summary>
		[DefaultValue((ushort)0)]
		public ushort Weight
		{
			get => this.weight;
			set => this.weight = value;
		}

		/// <summary>
		/// Port
		/// </summary>
		[DefaultValue((ushort)0)]
		public ushort Port
		{
			get => this.port;
			set => this.port = value;
		}

		/// <summary>
		/// Target Host
		/// </summary>
		[DefaultValueStringEmpty]
		public string TargetHost
		{
			get => this.targetHost;
			set => this.targetHost = value;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.service + "\t" + this.protocol +
				"\t" + this.priority.ToString() + "\t" + this.weight.ToString() +
				"\t" + this.port.ToString() + "\t" + this.targetHost;
		}
	}
}
