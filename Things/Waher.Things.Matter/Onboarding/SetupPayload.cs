using System;
using System.Collections.Generic;
using System.Text;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Onboarding
{
	/// <summary>
	/// Commissioning flow, as encoded in onboarding payloads.
	/// </summary>
	public enum CommissioningFlow
	{
		/// <summary>
		/// Standard commissioning flow: device is available for commissioning when powered on.
		/// </summary>
		Standard = 0,

		/// <summary>
		/// User-intent commissioning flow: user action required to put device in commissioning mode.
		/// </summary>
		UserIntent = 1,

		/// <summary>
		/// Custom commissioning flow: vendor-specific instructions required.
		/// </summary>
		Custom = 2
	}

	/// <summary>
	/// Discovery capabilities, as encoded in onboarding payloads.
	/// </summary>
	[Flags]
	public enum DiscoveryCapabilities
	{
		/// <summary>
		/// No capabilities specified.
		/// </summary>
		None = 0,

		/// <summary>
		/// Soft-AP.
		/// </summary>
		SoftAP = 1,

		/// <summary>
		/// Bluetooth Low Energy.
		/// </summary>
		BLE = 2,

		/// <summary>
		/// Device is already on the IP network.
		/// </summary>
		OnNetwork = 4,

		/// <summary>
		/// Wi-Fi Public Action Frame.
		/// </summary>
		WiFiPAF = 8
	}

	/// <summary>
	/// Matter onboarding payload (Matter Core Specification, section 5.1), parsed from or encoded as
	/// a QR code ("MT:...") or a manual pairing code (11 or 21 digits).
	/// </summary>
	public class SetupPayload
	{
		private const string Base38Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-.";

		/// <summary>
		/// Maximum value of passcodes.
		/// </summary>
		public const uint MaxPasscode = 99999998;

		/// <summary>
		/// Matter onboarding payload.
		/// </summary>
		public SetupPayload()
		{
		}

		/// <summary>
		/// Payload version.
		/// </summary>
		public byte Version { get; set; }

		/// <summary>
		/// Vendor ID (0 if not known).
		/// </summary>
		public ushort VendorId { get; set; }

		/// <summary>
		/// Product ID (0 if not known).
		/// </summary>
		public ushort ProductId { get; set; }

		/// <summary>
		/// Commissioning flow.
		/// </summary>
		public CommissioningFlow Flow { get; set; }

		/// <summary>
		/// Discovery capabilities.
		/// </summary>
		public DiscoveryCapabilities Capabilities { get; set; }

		/// <summary>
		/// Discriminator. If <see cref="IsShortDiscriminator"/> is true, only the upper 4 bits are known
		/// (available as <see cref="ShortDiscriminator"/>).
		/// </summary>
		public ushort Discriminator { get; set; }

		/// <summary>
		/// If only the short (4-bit) discriminator is known (manual pairing codes).
		/// </summary>
		public bool IsShortDiscriminator { get; set; }

		/// <summary>
		/// Short (4-bit) discriminator.
		/// </summary>
		public byte ShortDiscriminator => (byte)(this.Discriminator >> 8);

		/// <summary>
		/// Setup passcode.
		/// </summary>
		public uint Passcode { get; set; }

		/// <summary>
		/// Optional extension data encoded in a QR code (TLV), or null.
		/// </summary>
		public TlvElement ExtensionData { get; set; }

		/// <summary>
		/// Serial number, if provided in the QR code extension data.
		/// </summary>
		public string SerialNumber
		{
			get
			{
				TlvElement E = this.ExtensionData?.GetMember(TlvTag.Context(0));
				if (E is null)
					return null;
				else if (E.Type == TlvValueType.String)
					return E.AsString();
				else if (E.IsInteger)
					return E.AsUInt64().ToString();
				else
					return null;
			}
		}

		/// <summary>
		/// Checks if a passcode is valid (Matter Core Specification, 5.1.7.1).
		/// </summary>
		/// <param name="Passcode">Passcode.</param>
		/// <returns>If passcode is valid.</returns>
		public static bool IsValidPasscode(uint Passcode)
		{
			switch (Passcode)
			{
				case 0:
				case 11111111:
				case 22222222:
				case 33333333:
				case 44444444:
				case 55555555:
				case 66666666:
				case 77777777:
				case 88888888:
				case 99999999:
				case 12345678:
				case 87654321:
					return false;

				default:
					return Passcode <= MaxPasscode;
			}
		}

		/// <summary>
		/// Parses an onboarding payload, either a QR code string (starting with "MT:") or a manual pairing code.
		/// </summary>
		/// <param name="Code">Onboarding code.</param>
		/// <returns>Parsed payload.</returns>
		/// <exception cref="MatterDecodingException">If the code is not valid.</exception>
		public static SetupPayload Parse(string Code)
		{
			if (Code is null)
				throw new ArgumentNullException(nameof(Code));

			Code = Code.Trim();

			if (Code.IndexOf("MT:", StringComparison.OrdinalIgnoreCase) >= 0)
				return ParseQrCode(Code);
			else
				return ParseManualCode(Code);
		}

		/// <summary>
		/// Tries to parse an onboarding payload.
		/// </summary>
		/// <param name="Code">Onboarding code.</param>
		/// <param name="Payload">Parsed payload, if successful.</param>
		/// <returns>If parsing succeeded.</returns>
		public static bool TryParse(string Code, out SetupPayload Payload)
		{
			try
			{
				Payload = Parse(Code);
				return true;
			}
			catch (Exception)
			{
				Payload = null;
				return false;
			}
		}

		#region QR Code

		/// <summary>
		/// Parses a QR code onboarding payload. If the string contains multiple concatenated payloads
		/// (separated by '*'), the first one is returned.
		/// </summary>
		/// <param name="Code">QR code string.</param>
		/// <returns>Payload.</returns>
		public static SetupPayload ParseQrCode(string Code)
		{
			SetupPayload[] Result = ParseQrCodes(Code);
			if (Result.Length == 0)
				throw new MatterDecodingException("No payload found.");

			return Result[0];
		}

		/// <summary>
		/// Parses a QR code onboarding string, which may contain multiple concatenated payloads (separated by '*').
		/// </summary>
		/// <param name="Code">QR code string.</param>
		/// <returns>Payloads.</returns>
		public static SetupPayload[] ParseQrCodes(string Code)
		{
			int i = Code.IndexOf("MT:", StringComparison.OrdinalIgnoreCase);
			if (i < 0)
				throw new MatterDecodingException("QR code payload must start with \"MT:\".");

			string s = Code.Substring(i + 3);
			i = s.IndexOf('%');
			if (i >= 0)
				s = s.Substring(0, i);

			List<SetupPayload> Result = new List<SetupPayload>();

			foreach (string Part in s.Split('*'))
			{
				if (string.IsNullOrEmpty(Part))
					continue;

				byte[] Bin = Base38Decode(Part);
				Result.Add(DecodeQrBits(Bin));
			}

			return Result.ToArray();
		}

		private static SetupPayload DecodeQrBits(byte[] Bin)
		{
			if (Bin.Length < 11)
				throw new MatterDecodingException("QR code payload too short.");

			int BitPos = 0;
			SetupPayload Result = new SetupPayload()
			{
				Version = (byte)ReadBits(Bin, ref BitPos, 3),
				VendorId = (ushort)ReadBits(Bin, ref BitPos, 16),
				ProductId = (ushort)ReadBits(Bin, ref BitPos, 16),
				Flow = (CommissioningFlow)ReadBits(Bin, ref BitPos, 2),
				Capabilities = (DiscoveryCapabilities)ReadBits(Bin, ref BitPos, 8),
				Discriminator = (ushort)ReadBits(Bin, ref BitPos, 12),
				Passcode = (uint)ReadBits(Bin, ref BitPos, 27),
				IsShortDiscriminator = false
			};

			ulong Padding = ReadBits(Bin, ref BitPos, 4);
			if (Padding != 0)
				throw new MatterDecodingException("Invalid QR code padding.");

			if (Result.Version != 0)
				throw new MatterDecodingException("Unsupported QR code version: " + Result.Version.ToString());

			if (!IsValidPasscode(Result.Passcode))
				throw new MatterDecodingException("Invalid passcode in QR code.");

			if (Bin.Length > 11)
			{
				byte[] Tlv = new byte[Bin.Length - 11];
				Array.Copy(Bin, 11, Tlv, 0, Tlv.Length);

				try
				{
					Result.ExtensionData = TlvElement.Decode(Tlv);
				}
				catch (Exception)
				{
					// Ignore malformed optional data.
				}
			}

			return Result;
		}

		private static ulong ReadBits(byte[] Data, ref int BitPos, int NrBits)
		{
			ulong Result = 0;
			int i;

			for (i = 0; i < NrBits; i++)
			{
				int Pos = BitPos + i;
				if (((Data[Pos >> 3] >> (Pos & 7)) & 1) != 0)
					Result |= 1UL << i;
			}

			BitPos += NrBits;
			return Result;
		}

		private static void WriteBits(byte[] Data, ref int BitPos, ulong Value, int NrBits)
		{
			int i;

			for (i = 0; i < NrBits; i++)
			{
				int Pos = BitPos + i;
				if (((Value >> i) & 1) != 0)
					Data[Pos >> 3] |= (byte)(1 << (Pos & 7));
			}

			BitPos += NrBits;
		}

		/// <summary>
		/// Encodes the payload as a QR code string ("MT:...").
		/// </summary>
		/// <returns>QR code string.</returns>
		public string ToQrCode()
		{
			if (this.IsShortDiscriminator)
				throw new InvalidOperationException("A QR code requires a full (12-bit) discriminator.");

			if (!IsValidPasscode(this.Passcode))
				throw new InvalidOperationException("Invalid passcode.");

			byte[] Ext = this.ExtensionData?.Encode() ?? Array.Empty<byte>();
			byte[] Bin = new byte[11 + Ext.Length];
			int BitPos = 0;

			WriteBits(Bin, ref BitPos, this.Version, 3);
			WriteBits(Bin, ref BitPos, this.VendorId, 16);
			WriteBits(Bin, ref BitPos, this.ProductId, 16);
			WriteBits(Bin, ref BitPos, (ulong)this.Flow, 2);
			WriteBits(Bin, ref BitPos, (ulong)this.Capabilities, 8);
			WriteBits(Bin, ref BitPos, this.Discriminator, 12);
			WriteBits(Bin, ref BitPos, this.Passcode, 27);
			WriteBits(Bin, ref BitPos, 0, 4);

			Array.Copy(Ext, 0, Bin, 11, Ext.Length);

			return "MT:" + Base38Encode(Bin);
		}

		/// <summary>
		/// Encodes binary data using Base38 (Matter Core Specification, 5.1.3.1).
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Base38 encoded string.</returns>
		public static string Base38Encode(byte[] Data)
		{
			StringBuilder sb = new StringBuilder();
			int Pos = 0;
			int c = Data.Length;

			while (Pos < c)
			{
				int n = Math.Min(3, c - Pos);
				uint Value = 0;
				int i;

				for (i = n - 1; i >= 0; i--)
					Value = (Value << 8) | Data[Pos + i];

				int NrChars = n == 3 ? 5 : n == 2 ? 4 : 2;

				for (i = 0; i < NrChars; i++)
				{
					sb.Append(Base38Alphabet[(int)(Value % 38)]);
					Value /= 38;
				}

				Pos += n;
			}

			return sb.ToString();
		}

		/// <summary>
		/// Decodes a Base38 encoded string (Matter Core Specification, 5.1.3.1).
		/// </summary>
		/// <param name="Encoded">Encoded string.</param>
		/// <returns>Binary data.</returns>
		public static byte[] Base38Decode(string Encoded)
		{
			List<byte> Result = new List<byte>();
			int Pos = 0;
			int c = Encoded.Length;

			while (Pos < c)
			{
				int n = Math.Min(5, c - Pos);
				int NrBytes;

				switch (n)
				{
					case 5: NrBytes = 3; break;
					case 4: NrBytes = 2; break;
					case 2: NrBytes = 1; break;
					default: throw new MatterDecodingException("Invalid Base38 length.");
				}

				ulong Value = 0;
				int i;

				for (i = n - 1; i >= 0; i--)
				{
					int d = Base38Alphabet.IndexOf(char.ToUpperInvariant(Encoded[Pos + i]));
					if (d < 0)
						throw new MatterDecodingException("Invalid Base38 character: " + Encoded[Pos + i]);

					Value = Value * 38 + (ulong)d;
				}

				if (Value >= (1UL << (8 * NrBytes)))
					throw new MatterDecodingException("Invalid Base38 chunk.");

				for (i = 0; i < NrBytes; i++)
				{
					Result.Add((byte)Value);
					Value >>= 8;
				}

				Pos += n;
			}

			return Result.ToArray();
		}

		#endregion

		#region Manual pairing code

		/// <summary>
		/// Parses a manual pairing code (11 or 21 digits; spaces and dashes are ignored).
		/// </summary>
		/// <param name="Code">Manual pairing code.</param>
		/// <returns>Payload.</returns>
		public static SetupPayload ParseManualCode(string Code)
		{
			StringBuilder sb = new StringBuilder();

			foreach (char ch in Code)
			{
				if (ch >= '0' && ch <= '9')
					sb.Append(ch);
				else if (ch == '-' || ch == ' ' || ch == '.')
					continue;
				else
					throw new MatterDecodingException("Invalid character in manual pairing code: " + ch);
			}

			string s = sb.ToString();

			if (s.Length != 11 && s.Length != 21)
				throw new MatterDecodingException("Manual pairing code must have 11 or 21 digits.");

			if (Verhoeff.ComputeCheckDigit(s.Substring(0, s.Length - 1)) != s[s.Length - 1])
				throw new MatterDecodingException("Invalid check digit in manual pairing code.");

			int Chunk1 = s[0] - '0';
			bool VidPidPresent = (Chunk1 & 4) != 0;

			if (Chunk1 > 7)
				throw new MatterDecodingException("Unsupported manual pairing code version.");

			if (VidPidPresent != (s.Length == 21))
				throw new MatterDecodingException("Manual pairing code length does not match its VID/PID flag.");

			uint Chunk2 = uint.Parse(s.Substring(1, 5));
			uint Chunk3 = uint.Parse(s.Substring(6, 4));

			if (Chunk2 > 0xffff || Chunk3 > 0x1fff)
				throw new MatterDecodingException("Invalid manual pairing code.");

			uint ShortDiscriminator = (uint)(((Chunk1 & 3) << 2) | (int)(Chunk2 >> 14));
			uint Passcode = (Chunk2 & 0x3fff) | (Chunk3 << 14);

			if (!IsValidPasscode(Passcode))
				throw new MatterDecodingException("Invalid passcode in manual pairing code.");

			SetupPayload Result = new SetupPayload()
			{
				Discriminator = (ushort)(ShortDiscriminator << 8),
				IsShortDiscriminator = true,
				Passcode = Passcode,
				Flow = VidPidPresent ? CommissioningFlow.Custom : CommissioningFlow.Standard,
				Capabilities = DiscoveryCapabilities.None
			};

			if (VidPidPresent)
			{
				uint Vid = uint.Parse(s.Substring(10, 5));
				uint Pid = uint.Parse(s.Substring(15, 5));

				if (Vid > 0xffff || Pid > 0xffff)
					throw new MatterDecodingException("Invalid vendor or product ID in manual pairing code.");

				Result.VendorId = (ushort)Vid;
				Result.ProductId = (ushort)Pid;
			}

			return Result;
		}

		/// <summary>
		/// Encodes the payload as a manual pairing code.
		/// </summary>
		/// <param name="IncludeVendorAndProduct">If vendor and product IDs should be included (21-digit code).
		/// If null, they are included if the commissioning flow is <see cref="CommissioningFlow.Custom"/>.</param>
		/// <returns>Manual pairing code (digits only).</returns>
		public string ToManualCode(bool? IncludeVendorAndProduct = null)
		{
			if (!IsValidPasscode(this.Passcode))
				throw new InvalidOperationException("Invalid passcode.");

			bool VidPid = IncludeVendorAndProduct ?? (this.Flow == CommissioningFlow.Custom);
			uint ShortDiscriminator = (uint)(this.Discriminator >> 8) & 0x0f;
			uint Chunk1 = (VidPid ? 4u : 0u) | (ShortDiscriminator >> 2);
			uint Chunk2 = ((ShortDiscriminator & 3) << 14) | (this.Passcode & 0x3fff);
			uint Chunk3 = this.Passcode >> 14;

			StringBuilder sb = new StringBuilder();
			sb.Append(Chunk1.ToString());
			sb.Append(Chunk2.ToString("D5"));
			sb.Append(Chunk3.ToString("D4"));

			if (VidPid)
			{
				sb.Append(this.VendorId.ToString("D5"));
				sb.Append(this.ProductId.ToString("D5"));
			}

			string s = sb.ToString();
			return s + Verhoeff.ComputeCheckDigit(s);
		}

		/// <summary>
		/// Formats a manual pairing code for display (e.g. 3497-011-2332).
		/// </summary>
		/// <param name="Code">Manual code (digits only).</param>
		/// <returns>Formatted code.</returns>
		public static string FormatManualCode(string Code)
		{
			if (Code.Length == 11)
				return Code.Substring(0, 4) + "-" + Code.Substring(4, 3) + "-" + Code.Substring(7, 4);
			else if (Code.Length == 21)
				return Code.Substring(0, 4) + "-" + Code.Substring(4, 3) + "-" + Code.Substring(7, 4) + "-" +
					Code.Substring(11, 5) + "-" + Code.Substring(16, 5);
			else
				return Code;
		}

		#endregion

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Passcode: ");
			sb.Append(this.Passcode.ToString("D8"));
			sb.Append(", Discriminator: ");
			sb.Append(this.IsShortDiscriminator ? "short " + this.ShortDiscriminator.ToString() : this.Discriminator.ToString());

			if (this.VendorId != 0 || this.ProductId != 0)
			{
				sb.Append(", VID: 0x");
				sb.Append(this.VendorId.ToString("X4"));
				sb.Append(", PID: 0x");
				sb.Append(this.ProductId.ToString("X4"));
			}

			sb.Append(", Flow: ");
			sb.Append(this.Flow.ToString());

			if (this.Capabilities != DiscoveryCapabilities.None)
			{
				sb.Append(", Capabilities: ");
				sb.Append(this.Capabilities.ToString());
			}

			return sb.ToString();
		}
	}
}
