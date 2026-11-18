using System;
using System.Text;

namespace Waher.Things.Matter.Tlv
{
	/// <summary>
	/// TLV Tag control field (upper three bits of the control octet).
	/// </summary>
	public enum TlvTagControl : byte
	{
		/// <summary>
		/// Anonymous tag (0 octets).
		/// </summary>
		Anonymous = 0,

		/// <summary>
		/// Context-specific tag (1 octet).
		/// </summary>
		ContextSpecific = 1,

		/// <summary>
		/// Common profile tag (2 octets).
		/// </summary>
		CommonProfile2 = 2,

		/// <summary>
		/// Common profile tag (4 octets).
		/// </summary>
		CommonProfile4 = 3,

		/// <summary>
		/// Implicit profile tag (2 octets).
		/// </summary>
		ImplicitProfile2 = 4,

		/// <summary>
		/// Implicit profile tag (4 octets).
		/// </summary>
		ImplicitProfile4 = 5,

		/// <summary>
		/// Fully qualified tag (6 octets).
		/// </summary>
		FullyQualified6 = 6,

		/// <summary>
		/// Fully qualified tag (8 octets).
		/// </summary>
		FullyQualified8 = 7
	}

	/// <summary>
	/// Represents a Matter TLV tag (Matter Core Specification, Appendix A).
	/// </summary>
	public readonly struct TlvTag : IEquatable<TlvTag>
	{
		/// <summary>
		/// Anonymous tag.
		/// </summary>
		public static readonly TlvTag Anonymous = new TlvTag(TlvTagControl.Anonymous, 0, 0, 0);

		private readonly TlvTagControl control;
		private readonly uint number;
		private readonly ushort vendorId;
		private readonly ushort profileNumber;

		/// <summary>
		/// Represents a Matter TLV tag.
		/// </summary>
		/// <param name="Control">Tag control.</param>
		/// <param name="Number">Tag number.</param>
		/// <param name="VendorId">Vendor ID (fully qualified tags).</param>
		/// <param name="ProfileNumber">Profile number (fully qualified tags).</param>
		public TlvTag(TlvTagControl Control, uint Number, ushort VendorId, ushort ProfileNumber)
		{
			this.control = Control;
			this.number = Number;
			this.vendorId = VendorId;
			this.profileNumber = ProfileNumber;
		}

		/// <summary>
		/// Creates a context-specific tag.
		/// </summary>
		/// <param name="Tag">Tag number (0-255).</param>
		/// <returns>Context-specific tag.</returns>
		public static TlvTag Context(int Tag)
		{
			if (Tag < 0 || Tag > 255)
				throw new ArgumentOutOfRangeException(nameof(Tag), "Context tags must be in the range 0-255.");

			return new TlvTag(TlvTagControl.ContextSpecific, (uint)Tag, 0, 0);
		}

		/// <summary>
		/// Creates a common profile tag.
		/// </summary>
		/// <param name="Tag">Tag number.</param>
		/// <returns>Common profile tag.</returns>
		public static TlvTag CommonProfile(uint Tag)
		{
			return new TlvTag(Tag <= 0xffff ? TlvTagControl.CommonProfile2 : TlvTagControl.CommonProfile4, Tag, 0, 0);
		}

		/// <summary>
		/// Creates an implicit profile tag.
		/// </summary>
		/// <param name="Tag">Tag number.</param>
		/// <returns>Implicit profile tag.</returns>
		public static TlvTag ImplicitProfile(uint Tag)
		{
			return new TlvTag(Tag <= 0xffff ? TlvTagControl.ImplicitProfile2 : TlvTagControl.ImplicitProfile4, Tag, 0, 0);
		}

		/// <summary>
		/// Creates a fully qualified tag.
		/// </summary>
		/// <param name="VendorId">Vendor ID.</param>
		/// <param name="ProfileNumber">Profile number.</param>
		/// <param name="Tag">Tag number.</param>
		/// <returns>Fully qualified tag.</returns>
		public static TlvTag FullyQualified(ushort VendorId, ushort ProfileNumber, uint Tag)
		{
			return new TlvTag(Tag <= 0xffff ? TlvTagControl.FullyQualified6 : TlvTagControl.FullyQualified8, Tag, VendorId, ProfileNumber);
		}

		/// <summary>
		/// Implicit conversion of an integer to a context-specific tag.
		/// </summary>
		/// <param name="Tag">Tag number.</param>
		public static implicit operator TlvTag(int Tag) => Context(Tag);

		/// <summary>
		/// Tag control.
		/// </summary>
		public TlvTagControl Control => this.control;

		/// <summary>
		/// Tag number.
		/// </summary>
		public uint Number => this.number;

		/// <summary>
		/// Vendor ID (fully qualified tags).
		/// </summary>
		public ushort VendorId => this.vendorId;

		/// <summary>
		/// Profile number (fully qualified tags).
		/// </summary>
		public ushort ProfileNumber => this.profileNumber;

		/// <summary>
		/// If the tag is anonymous.
		/// </summary>
		public bool IsAnonymous => this.control == TlvTagControl.Anonymous;

		/// <summary>
		/// If the tag is a context-specific tag.
		/// </summary>
		public bool IsContextSpecific => this.control == TlvTagControl.ContextSpecific;

		/// <summary>
		/// Checks if the tag is a given context-specific tag.
		/// </summary>
		/// <param name="Tag">Context-specific tag number.</param>
		/// <returns>If the tag matches.</returns>
		public bool IsContext(int Tag)
		{
			return this.control == TlvTagControl.ContextSpecific && this.number == (uint)Tag;
		}

		/// <summary>
		/// Number of octets used to encode the tag.
		/// </summary>
		public int EncodedLength
		{
			get
			{
				switch (this.control)
				{
					case TlvTagControl.Anonymous: return 0;
					case TlvTagControl.ContextSpecific: return 1;
					case TlvTagControl.CommonProfile2: return 2;
					case TlvTagControl.CommonProfile4: return 4;
					case TlvTagControl.ImplicitProfile2: return 2;
					case TlvTagControl.ImplicitProfile4: return 4;
					case TlvTagControl.FullyQualified6: return 6;
					case TlvTagControl.FullyQualified8: return 8;
					default: return 0;
				}
			}
		}

		/// <inheritdoc/>
		public bool Equals(TlvTag Other)
		{
			return this.control == Other.control &&
				this.number == Other.number &&
				this.vendorId == Other.vendorId &&
				this.profileNumber == Other.profileNumber;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is TlvTag Tag && this.Equals(Tag);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.control.GetHashCode();
			Result ^= Result << 5 ^ this.number.GetHashCode();
			Result ^= Result << 5 ^ this.vendorId.GetHashCode();
			Result ^= Result << 5 ^ this.profileNumber.GetHashCode();
			return Result;
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		public static bool operator ==(TlvTag A, TlvTag B) => A.Equals(B);

		/// <summary>
		/// Non-equality operator.
		/// </summary>
		public static bool operator !=(TlvTag A, TlvTag B) => !A.Equals(B);

		/// <inheritdoc/>
		public override string ToString()
		{
			switch (this.control)
			{
				case TlvTagControl.Anonymous:
					return string.Empty;

				case TlvTagControl.ContextSpecific:
					return this.number.ToString();

				case TlvTagControl.CommonProfile2:
				case TlvTagControl.CommonProfile4:
					return "Matter::" + this.number.ToString();

				case TlvTagControl.ImplicitProfile2:
				case TlvTagControl.ImplicitProfile4:
					return "Implicit::" + this.number.ToString();

				default:
					StringBuilder sb = new StringBuilder();
					sb.Append(this.vendorId.ToString());
					sb.Append("::");
					sb.Append(this.profileNumber.ToString());
					sb.Append(':');
					sb.Append(this.number.ToString());
					return sb.ToString();
			}
		}
	}
}
