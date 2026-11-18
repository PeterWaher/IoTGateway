using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Waher.Things.Matter.Discovery
{
	/// <summary>
	/// DNS resource record types used by DNS-SD.
	/// </summary>
	public enum DnsType : ushort
	{
		/// <summary>IPv4 address.</summary>
		A = 1,
		/// <summary>Domain name pointer.</summary>
		PTR = 12,
		/// <summary>Text record.</summary>
		TXT = 16,
		/// <summary>IPv6 address.</summary>
		AAAA = 28,
		/// <summary>Service record.</summary>
		SRV = 33,
		/// <summary>NSEC record.</summary>
		NSEC = 47,
		/// <summary>Any record.</summary>
		ANY = 255
	}

	/// <summary>
	/// DNS question.
	/// </summary>
	public class DnsQuestion
	{
		/// <summary>
		/// DNS question.
		/// </summary>
		/// <param name="Name">Name.</param>
		/// <param name="Type">Type.</param>
		/// <param name="UnicastResponse">If a unicast response is requested (mDNS QU bit).</param>
		public DnsQuestion(string Name, DnsType Type, bool UnicastResponse = false)
		{
			this.Name = Name;
			this.Type = Type;
			this.UnicastResponse = UnicastResponse;
		}

		/// <summary>
		/// Name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Type.
		/// </summary>
		public DnsType Type { get; }

		/// <summary>
		/// If a unicast response is requested.
		/// </summary>
		public bool UnicastResponse { get; }
	}

	/// <summary>
	/// DNS resource record.
	/// </summary>
	public class DnsRecord
	{
		/// <summary>
		/// Name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Type.
		/// </summary>
		public DnsType Type { get; set; }

		/// <summary>
		/// Class (without cache-flush bit).
		/// </summary>
		public ushort Class { get; set; } = 1;

		/// <summary>
		/// If the cache-flush bit was set.
		/// </summary>
		public bool CacheFlush { get; set; }

		/// <summary>
		/// Time to live, in seconds.
		/// </summary>
		public uint Ttl { get; set; }

		/// <summary>
		/// Target name (PTR, SRV).
		/// </summary>
		public string Target { get; set; }

		/// <summary>
		/// Address (A, AAAA).
		/// </summary>
		public IPAddress Address { get; set; }

		/// <summary>
		/// Port (SRV).
		/// </summary>
		public ushort Port { get; set; }

		/// <summary>
		/// Priority (SRV).
		/// </summary>
		public ushort Priority { get; set; }

		/// <summary>
		/// Weight (SRV).
		/// </summary>
		public ushort Weight { get; set; }

		/// <summary>
		/// Text strings (TXT).
		/// </summary>
		public string[] Text { get; set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			switch (this.Type)
			{
				case DnsType.A:
				case DnsType.AAAA:
					return this.Name + " " + this.Type.ToString() + " " + this.Address?.ToString();

				case DnsType.PTR:
					return this.Name + " PTR " + this.Target;

				case DnsType.SRV:
					return this.Name + " SRV " + this.Target + ":" + this.Port.ToString();

				case DnsType.TXT:
					return this.Name + " TXT " + string.Join(" ", this.Text ?? Array.Empty<string>());

				default:
					return this.Name + " " + this.Type.ToString();
			}
		}
	}

	/// <summary>
	/// Minimal DNS message codec, for multicast DNS (RFC 6762) and DNS-SD (RFC 6763).
	/// </summary>
	public class DnsMessage
	{
		/// <summary>
		/// Message ID.
		/// </summary>
		public ushort Id { get; set; }

		/// <summary>
		/// If the message is a response.
		/// </summary>
		public bool IsResponse { get; set; }

		/// <summary>
		/// Questions.
		/// </summary>
		public List<DnsQuestion> Questions { get; } = new List<DnsQuestion>();

		/// <summary>
		/// Answer records.
		/// </summary>
		public List<DnsRecord> Answers { get; } = new List<DnsRecord>();

		/// <summary>
		/// Authority records.
		/// </summary>
		public List<DnsRecord> Authority { get; } = new List<DnsRecord>();

		/// <summary>
		/// Additional records.
		/// </summary>
		public List<DnsRecord> Additional { get; } = new List<DnsRecord>();

		/// <summary>
		/// All records in the message.
		/// </summary>
		public IEnumerable<DnsRecord> AllRecords
		{
			get
			{
				foreach (DnsRecord R in this.Answers)
					yield return R;

				foreach (DnsRecord R in this.Authority)
					yield return R;

				foreach (DnsRecord R in this.Additional)
					yield return R;
			}
		}

		/// <summary>
		/// Encodes the message.
		/// </summary>
		/// <returns>Encoded message.</returns>
		public byte[] Encode()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				WriteUInt16(ms, this.Id);
				WriteUInt16(ms, (ushort)(this.IsResponse ? 0x8400 : 0x0000));
				WriteUInt16(ms, (ushort)this.Questions.Count);
				WriteUInt16(ms, (ushort)this.Answers.Count);
				WriteUInt16(ms, (ushort)this.Authority.Count);
				WriteUInt16(ms, (ushort)this.Additional.Count);

				foreach (DnsQuestion Q in this.Questions)
				{
					WriteName(ms, Q.Name);
					WriteUInt16(ms, (ushort)Q.Type);
					WriteUInt16(ms, (ushort)(Q.UnicastResponse ? 0x8001 : 0x0001));
				}

				foreach (DnsRecord R in this.Answers)
					WriteRecord(ms, R);

				foreach (DnsRecord R in this.Authority)
					WriteRecord(ms, R);

				foreach (DnsRecord R in this.Additional)
					WriteRecord(ms, R);

				return ms.ToArray();
			}
		}

		private static void WriteRecord(MemoryStream ms, DnsRecord R)
		{
			WriteName(ms, R.Name);
			WriteUInt16(ms, (ushort)R.Type);
			WriteUInt16(ms, (ushort)(R.Class | (R.CacheFlush ? 0x8000 : 0)));
			WriteUInt32(ms, R.Ttl);

			using (MemoryStream Data = new MemoryStream())
			{
				switch (R.Type)
				{
					case DnsType.A:
					case DnsType.AAAA:
						byte[] Bin = R.Address.GetAddressBytes();
						Data.Write(Bin, 0, Bin.Length);
						break;

					case DnsType.PTR:
						WriteName(Data, R.Target);
						break;

					case DnsType.SRV:
						WriteUInt16(Data, R.Priority);
						WriteUInt16(Data, R.Weight);
						WriteUInt16(Data, R.Port);
						WriteName(Data, R.Target);
						break;

					case DnsType.TXT:
						if (R.Text is null || R.Text.Length == 0)
							Data.WriteByte(0);
						else
						{
							foreach (string s in R.Text)
							{
								byte[] b = Encoding.UTF8.GetBytes(s);
								Data.WriteByte((byte)Math.Min(255, b.Length));
								Data.Write(b, 0, Math.Min(255, b.Length));
							}
						}
						break;
				}

				byte[] RData = Data.ToArray();
				WriteUInt16(ms, (ushort)RData.Length);
				ms.Write(RData, 0, RData.Length);
			}
		}

		private static void WriteName(Stream Output, string Name)
		{
			foreach (string Label in Name.TrimEnd('.').Split('.'))
			{
				if (Label.Length == 0)
					continue;

				byte[] b = Encoding.UTF8.GetBytes(Label);
				if (b.Length > 63)
					throw new ArgumentException("DNS label too long.", nameof(Name));

				Output.WriteByte((byte)b.Length);
				Output.Write(b, 0, b.Length);
			}

			Output.WriteByte(0);
		}

		private static void WriteUInt16(Stream Output, ushort Value)
		{
			Output.WriteByte((byte)(Value >> 8));
			Output.WriteByte((byte)Value);
		}

		private static void WriteUInt32(Stream Output, uint Value)
		{
			Output.WriteByte((byte)(Value >> 24));
			Output.WriteByte((byte)(Value >> 16));
			Output.WriteByte((byte)(Value >> 8));
			Output.WriteByte((byte)Value);
		}

		/// <summary>
		/// Decodes a DNS message.
		/// </summary>
		/// <param name="Data">Encoded message.</param>
		/// <returns>Decoded message.</returns>
		public static DnsMessage Decode(byte[] Data)
		{
			if (Data.Length < 12)
				throw new MatterDecodingException("DNS message too short.");

			int Pos = 0;
			DnsMessage Result = new DnsMessage()
			{
				Id = ReadUInt16(Data, ref Pos)
			};

			ushort Flags = ReadUInt16(Data, ref Pos);
			Result.IsResponse = (Flags & 0x8000) != 0;

			int NrQuestions = ReadUInt16(Data, ref Pos);
			int NrAnswers = ReadUInt16(Data, ref Pos);
			int NrAuthority = ReadUInt16(Data, ref Pos);
			int NrAdditional = ReadUInt16(Data, ref Pos);

			while (NrQuestions-- > 0)
			{
				string Name = ReadName(Data, ref Pos);
				DnsType Type = (DnsType)ReadUInt16(Data, ref Pos);
				ushort Class = ReadUInt16(Data, ref Pos);

				Result.Questions.Add(new DnsQuestion(Name, Type, (Class & 0x8000) != 0));
			}

			ReadRecords(Data, ref Pos, NrAnswers, Result.Answers);
			ReadRecords(Data, ref Pos, NrAuthority, Result.Authority);
			ReadRecords(Data, ref Pos, NrAdditional, Result.Additional);

			return Result;
		}

		private static void ReadRecords(byte[] Data, ref int Pos, int Count, List<DnsRecord> Records)
		{
			while (Count-- > 0)
			{
				DnsRecord R = new DnsRecord()
				{
					Name = ReadName(Data, ref Pos),
					Type = (DnsType)ReadUInt16(Data, ref Pos)
				};

				ushort Class = ReadUInt16(Data, ref Pos);
				R.Class = (ushort)(Class & 0x7fff);
				R.CacheFlush = (Class & 0x8000) != 0;
				R.Ttl = ReadUInt32(Data, ref Pos);

				int Len = ReadUInt16(Data, ref Pos);
				int End = Pos + Len;

				if (End > Data.Length)
					throw new MatterDecodingException("DNS record exceeds message.");

				switch (R.Type)
				{
					case DnsType.A:
						if (Len == 4)
						{
							byte[] Bin = new byte[4];
							Array.Copy(Data, Pos, Bin, 0, 4);
							R.Address = new IPAddress(Bin);
						}
						break;

					case DnsType.AAAA:
						if (Len == 16)
						{
							byte[] Bin = new byte[16];
							Array.Copy(Data, Pos, Bin, 0, 16);
							R.Address = new IPAddress(Bin);
						}
						break;

					case DnsType.PTR:
						int p = Pos;
						R.Target = ReadName(Data, ref p);
						break;

					case DnsType.SRV:
						p = Pos;
						R.Priority = ReadUInt16(Data, ref p);
						R.Weight = ReadUInt16(Data, ref p);
						R.Port = ReadUInt16(Data, ref p);
						R.Target = ReadName(Data, ref p);
						break;

					case DnsType.TXT:
						List<string> Text = new List<string>();
						p = Pos;

						while (p < End)
						{
							int n = Data[p++];
							if (p + n > End)
								break;

							if (n > 0)
								Text.Add(Encoding.UTF8.GetString(Data, p, n));

							p += n;
						}

						R.Text = Text.ToArray();
						break;
				}

				Pos = End;
				Records.Add(R);
			}
		}

		private static string ReadName(byte[] Data, ref int Pos)
		{
			StringBuilder sb = new StringBuilder();
			int p = Pos;
			bool Jumped = false;
			int NrJumps = 0;

			while (true)
			{
				if (p >= Data.Length)
					throw new MatterDecodingException("Invalid DNS name.");

				int Len = Data[p++];

				if (Len == 0)
					break;

				if ((Len & 0xc0) == 0xc0)
				{
					if (p >= Data.Length)
						throw new MatterDecodingException("Invalid DNS name pointer.");

					int Offset = ((Len & 0x3f) << 8) | Data[p++];

					if (!Jumped)
						Pos = p;

					Jumped = true;
					p = Offset;

					if (++NrJumps > 64)
						throw new MatterDecodingException("DNS name pointer loop.");

					continue;
				}

				if (p + Len > Data.Length)
					throw new MatterDecodingException("Invalid DNS label.");

				if (sb.Length > 0)
					sb.Append('.');

				sb.Append(Encoding.UTF8.GetString(Data, p, Len));
				p += Len;
			}

			if (!Jumped)
				Pos = p;

			return sb.ToString();
		}

		private static ushort ReadUInt16(byte[] Data, ref int Pos)
		{
			if (Pos + 2 > Data.Length)
				throw new MatterDecodingException("Unexpected end of DNS message.");

			ushort Result = (ushort)((Data[Pos] << 8) | Data[Pos + 1]);
			Pos += 2;
			return Result;
		}

		private static uint ReadUInt32(byte[] Data, ref int Pos)
		{
			if (Pos + 4 > Data.Length)
				throw new MatterDecodingException("Unexpected end of DNS message.");

			uint Result = ((uint)Data[Pos] << 24) | ((uint)Data[Pos + 1] << 16) | ((uint)Data[Pos + 2] << 8) | Data[Pos + 3];
			Pos += 4;
			return Result;
		}
	}
}
