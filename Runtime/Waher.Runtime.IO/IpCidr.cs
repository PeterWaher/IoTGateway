using System;
using System.Net;

namespace Waher.Runtime.IO
{
	/// <summary>
	/// IP Address Rangee, expressed using CIDR format.
	/// </summary>
	public class IpCidr
	{
		private readonly IPAddress address;
		private readonly byte[] binary;
		private readonly int range;

		/// <summary>
		/// IP Address Rangee, expressed using CIDR format.
		/// </summary>
		/// <param name="Address">Address</param>
		/// <param name="Range">Range (0-8*bytelen)</param>
		public IpCidr(IPAddress Address, int Range)
			: this(Address, Address.GetAddressBytes(), Range)
		{
		}

		/// <summary>
		/// IP Address Rangee, expressed using CIDR format.
		/// </summary>
		/// <param name="Address">Address</param>
		/// <param name="Binary">Binary representation of address.</param>
		/// <param name="Range">Range (0-8*bytelen)</param>
		private IpCidr(IPAddress Address, byte[] Binary, int Range)
		{
			this.address = Address;
			this.binary = Binary;
			this.range = Range;
		}

		/// <summary>
		/// Parsed IP Address
		/// </summary>
		public IPAddress Address => this.address;

		/// <summary>
		/// Range
		/// </summary>
		public int Range => this.range;

		/// <summary>
		/// Tries to parse an IP Range in CIDR format. (An IP Address alone is considered implicitly having a mask of 8*byte length of address);
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="Parsed">Parsed object, if function returns true.</param>
		/// <returns>If the string could be parsed.</returns>
		public static bool TryParse(string s, out IpCidr Parsed)
		{
			s = s.Trim();

			int i = s.IndexOf('/');
			int Range = -1;

			Parsed = null;

			if (i > 0)
			{
				if (!int.TryParse(s.Substring(i + 1), out Range) || Range < 0)
					return false;

				s = s.Substring(0, i);
			}

			if (!IPAddress.TryParse(s, out IPAddress Address))
				return false;

			byte[] Bin = Address.GetAddressBytes();
			int MaxRange = Bin.Length << 3;

			if (Range < 0)
				Range = MaxRange;
			else if (Range > MaxRange)
				return false;

			Parsed = new IpCidr(Address, Bin, Range);

			return true;
		}

		/// <summary>
		/// Checks if an IP Address matches the defined range.
		/// </summary>
		/// <param name="Address">IP Address.</param>
		/// <returns>If it matches.</returns>
		public bool Matches(IPAddress Address)
		{
			if (Address.AddressFamily != this.address.AddressFamily)
				return false;

			byte[] Bin = Address.GetAddressBytes();
			int i, c = Math.Min(this.binary.Length, Bin.Length);
			int Bits = this.range;
			byte Mask;

			for (i = 0; i < c && Bits > 0; i++, Bits -= 8)
			{
				if (Bits >= 8)
					Mask = 0xff;
				else
					Mask = (byte)(0xff << (8 - Bits));

				if (((this.binary[i] ^ Bin[i]) & Mask) != 0)
					return false;
			}

			return Bits <= 0;
		}

	}
}
