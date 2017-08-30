using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.LWM2M.ContentFormats
{
	/// <summary>
	/// Type of Identifier.
	/// </summary>
	public enum IdentifierType
	{
		/// <summary>
		/// Object Instance in which case the Value contains one or more Resource TLVs.
		/// </summary>
		ObjectInstance = 0,

		/// <summary>
		/// Resource Instance with Value for use within a multiple Resource TLV.
		/// </summary>
		ResourceInstance = 64,

		/// <summary>
		/// Multiple Resource, in which case the Value contains one or more Resource Instance TLVs.
		/// </summary>
		MultipleResource = 128,

		/// <summary>
		/// Resource with Value.
		/// </summary>
		Resource = 192
	}

	/// <summary>
	/// Interface for LWM2M writers.
	/// </summary>
	public interface ILwm2mWriter
    {
		/// <summary>
		/// Content format of generated payload.
		/// </summary>
		ushort ContentFormat
		{
			get;
		}

		/// <summary>
		/// Writes a TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, byte[] Value);

		/// <summary>
		/// Begins a new nested TLV
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		void Begin(IdentifierType IdentifierType, ushort Identifier);

		/// <summary>
		/// Ends a nested TLV
		/// </summary>
		void End();

		/// <summary>
		/// Writes a string-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, string Value);

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, sbyte Value);

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, short Value);

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, int Value);

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, long Value);

		/// <summary>
		/// Writes a floating point-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, float Value);

		/// <summary>
		/// Writes a floating point-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, double Value);

		/// <summary>
		/// Writes a Boolean valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, bool Value);

		/// <summary>
		/// Writes a DateTime-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, DateTime Value);

		/// <summary>
		/// Writes an object link TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="ObjectInstanceId">Object Instance ID</param>
		void Write(IdentifierType IdentifierType, ushort Identifier, ushort ObjectId,
			ushort ObjectInstanceId);

		/// <summary>
		/// Writes a none-valued (or void-valued) TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		void Write(IdentifierType IdentifierType, ushort Identifier);

		/// <summary>
		/// Binary serialization of what has been written.
		/// </summary>
		/// <returns></returns>
		byte[] ToArray();

	}
}
