using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Server Selection
	/// </summary>
	public class SRV : ResourceRecord
	{
		private readonly string service;
		private readonly string protocol;
		private readonly ushort priority;
		private readonly ushort weight;
		private readonly ushort port;
		private readonly string targetHost;

		/// <summary>
		/// Mailbox eXchange
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		public SRV(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data)
			: base(Name, Type, Class, Ttl)
		{
			this.service = Service;
			this.protocol = Protocol;
			this.priority = Priority;
			this.weight = Weight;
			this.port = Port;
			this.targetHost = TargetHost;
		}

		/// <summary>
		/// Service name.
		/// </summary>
		public string Service => this.service;

		/// <summary>
		/// Protocol name
		/// </summary>
		public string Protocol => this.protocol;

		/// <summary>
		/// Priority
		/// </summary>
		public ushort Priority => this.priority;

		/// <summary>
		/// Weight
		/// </summary>
		public ushort Weight => this.weight;

		/// <summary>
		/// Port
		/// </summary>
		public ushort Port => this.port;

		/// <summary>
		/// Target Host
		/// </summary>
		public string TargetHost => this.targetHost;

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
