using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;

namespace Waher.Networking.DNS.Communication
{
	/// <summary>
	/// Represents a DNS message
	/// </summary>
	public class DnsMessage
	{
		private readonly ushort id;
		private readonly bool response;
		private readonly OpCode opCode;
		private readonly bool authoritativeAnswer;
		private readonly bool truncation;
		private readonly bool recursionDesired;
		private readonly bool recursionAvailable;
		private readonly RCode rCode;
		private readonly Question[] questions;
		private readonly ResourceRecord[] answer;
		private readonly ResourceRecord[] authority;
		private readonly ResourceRecord[] additional;

		/// <summary>
		/// Represents a DNS message
		/// </summary>
		/// <param name="Data">Binary representation of a DNS message.</param>
		public DnsMessage(byte[] Data)
		{
			using (MemoryStream ms = new MemoryStream(Data))
			{
				this.id = DnsResolver.ReadUInt16(ms);

				byte b = (byte)ms.ReadByte();
				this.response = (b & 0x80) != 0;
				this.opCode = (OpCode)((b >> 3) & 15);
				this.authoritativeAnswer = (b & 4) != 0;
				this.truncation = (b & 2) != 0;
				this.recursionDesired = (b & 1) != 0;

				b = (byte)ms.ReadByte();
				this.recursionAvailable = (b & 128) != 0;
				this.rCode = (RCode)(b & 31);

				ushort QDCOUNT = DnsResolver.ReadUInt16(ms);
				ushort ANCOUNT = DnsResolver.ReadUInt16(ms);
				ushort NSCOUNT = DnsResolver.ReadUInt16(ms);
				ushort ARCOUNT = DnsResolver.ReadUInt16(ms);

				this.questions = new Question[QDCOUNT];
				ushort i;

				for (i = 0; i < QDCOUNT; i++)
				{
					string QNAME = DnsResolver.ReadName(ms);
					QTYPE QTYPE = (QTYPE)DnsResolver.ReadUInt16(ms);
					QCLASS QCLASS = (QCLASS)DnsResolver.ReadUInt16(ms);

					this.questions[i] = new Question(QNAME, QTYPE, QCLASS);
				}

				this.answer = DnsResolver.ReadResourceRecords(ms, ANCOUNT);
				this.authority = DnsResolver.ReadResourceRecords(ms, NSCOUNT);
				this.additional = DnsResolver.ReadResourceRecords(ms, ARCOUNT);
			}
		}

		/// <summary>
		/// Message identifier
		/// </summary>
		public ushort ID => this.id;

		/// <summary>
		/// If a Response (true) or a query (false)
		/// </summary>
		public bool Response => this.response;

		/// <summary>
		/// Operation code
		/// </summary>
		public OpCode OpCode => this.opCode;

		/// <summary>
		/// Responding name server is an authority for the domain name in question section
		/// </summary>
		public bool AuthoritativeAnswer => this.authoritativeAnswer;

		/// <summary>
		/// Message was truncated due to length greater than that permitted on the transmission channel
		/// </summary>
		public bool Truncation => this.truncation;

		/// <summary>
		/// Directs the name server to pursue the query recursively
		/// </summary>
		public bool RecursionDesired => this.recursionDesired;

		/// <summary>
		/// Denotes whether recursive query support is available in the name server
		/// </summary>
		public bool RecursionAvailable => this.recursionAvailable;

		/// <summary>
		/// Response code
		/// </summary>
		public RCode RCode => this.rCode;

		/// <summary>
		/// Question section
		/// </summary>
		public Question[] Questions => this.questions;

		/// <summary>
		/// Answer section
		/// </summary>
		public ResourceRecord[] Answer => this.answer;

		/// <summary>
		/// Authority section
		/// </summary>
		public ResourceRecord[] Authority => this.authority;

		/// <summary>
		/// Additional section
		/// </summary>
		public ResourceRecord[] Additional => this.additional;

	}
}
