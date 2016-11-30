using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Storage
{
	/// <summary>
	/// Handles primary storage of objects, as pairs of primary keys (GUIDs) and serialized objects.
	/// </summary>
	internal class PrimaryRecords : IRecordHandler
	{
		private int recordStart;
		private int inlineObjectSizeLimit;

		/// <summary>
		/// Handles primary storage of objects, as pairs of primary keys (GUIDs) and serialized objects.
		/// </summary>
		/// <param name="InlineObjectSizeLimit">Maximum size of objects that are stored in-line. Larger objects will be stored as BLOBs.</param>
		public PrimaryRecords(int InlineObjectSizeLimit)
		{
			this.inlineObjectSizeLimit = InlineObjectSizeLimit;
			this.recordStart = 0;
		}

		/// <summary>
		/// Gets the next record key.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Next key value, or null if no more keys available.</returns>
		public IComparable GetKey(BinaryDeserializer Reader)
		{
			if (Reader.BytesLeft < 17)
				return null;

			this.recordStart = Reader.Position;

			Guid Result = Reader.ReadGuid();

			if (Result.Equals(Guid.Empty))
				return null;
			else
				return Result;
		}

		/// <summary>
		/// Skips the next record key.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>If a key was skipped (true), or if no more keys are available (false).</returns>
		public bool SkipKey(BinaryDeserializer Reader)
		{
			if (Reader.BytesLeft < 17)
				return false;

			this.recordStart = Reader.Position;
			Guid Result = Reader.ReadGuid();

			if (Result.Equals(Guid.Empty))
				return false;
			else
				return true;
		}

		/// <summary>
		/// Gets the full payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Full size of the payload.</returns>
		public uint GetFullPayloadSize(BinaryDeserializer Reader)
		{
			return (uint)Reader.ReadVariableLengthUInt64();
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Size of the payload.</returns>
		public int GetPayloadSize(BinaryDeserializer Reader)
		{
			int Len = (int)Reader.ReadVariableLengthUInt64();
			if (Reader.Position - this.recordStart + Len > this.inlineObjectSizeLimit)
				return 4;
			else
				return Len;
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Size of the payload.</returns>
		public int GetPayloadSize(BinaryDeserializer Reader, out bool IsBlob)
		{
			int Len = (int)Reader.ReadVariableLengthUInt64();
			if (IsBlob = (Reader.Position - this.recordStart + Len > this.inlineObjectSizeLimit))
				return 4;
			else
				return Len;
		}

		/// <summary>
		/// Gets the type of the payload, if any.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Payload type, if any, or null, if not.</returns>
		public string GetPayloadType(BinaryDeserializer Reader)
		{
			return Reader.ReadString();
		}
	}
}
