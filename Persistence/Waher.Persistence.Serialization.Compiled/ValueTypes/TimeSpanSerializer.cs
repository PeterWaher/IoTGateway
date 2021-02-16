using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ValueTypes
{
	/// <summary>
	/// Serializes a <see cref="TimeSpan"/> value.
	/// </summary>
	public class TimeSpanSerializer : ValueTypeSerializer
	{
		/// <summary>
		/// Serializes a <see cref="TimeSpan"/> value.
		/// </summary>
		public TimeSpanSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(TimeSpan);
			}
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public override Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_TIMESPAN: return Task.FromResult<object>(Reader.ReadTimeSpan());
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return Task.FromResult<object>(TimeSpan.Parse(Reader.ReadString()));
				case ObjectSerializer.TYPE_MIN: return Task.FromResult<object>(TimeSpan.MinValue);
				case ObjectSerializer.TYPE_MAX: return Task.FromResult<object>(TimeSpan.MaxValue);
				case ObjectSerializer.TYPE_NULL: return Task.FromResult<object>(null);
				default: throw new Exception("Expected a TimeSpan value.");
			}
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public override Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_TIMESPAN, 6);

			Writer.Write((TimeSpan)Value);

			return Task.CompletedTask;
		}

	}
}
