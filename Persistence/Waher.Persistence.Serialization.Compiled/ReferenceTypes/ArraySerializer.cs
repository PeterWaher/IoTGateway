using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ReferenceTypes
{
	/// <summary>
	/// Generic serializer of array types.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	public class ArraySerializer<T> : GeneratedObjectSerializerBase
	{
		private readonly ISerializerContext context;

		/// <summary>
		/// Generic serializer of array types.
		/// </summary>
		/// <param name="Context">Serialization context.</param>
		public ArraySerializer(ISerializerContext Context)
		{
			this.context = Context;
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(T[]);
			}
		}

		/// <summary>
		/// If the value being serialized, can be null.
		/// </summary>
		public override bool IsNullable
		{
			get { return true; }
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public override async Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			return await ReadArray<T>(this.context, Reader, DataType.Value);
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		public override async Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value, object State)
		{
			if (Value is null)
			{
				if (!WriteTypeCode)
					throw new NullReferenceException("Value cannot be null.");

				Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
			}
			else
			{
				T[] Array = (T[])Value;
				Type LastType = typeof(T);
				IObjectSerializer S = await this.context.GetObjectSerializer(LastType);
				Type ItemType;
				bool Nullable;

				Writer.WriteBits(ObjectSerializer.TYPE_ARRAY, 6);
				Writer.WriteVariableLengthUInt64((ulong)Array.Length);

				if (Nullable = S.IsNullable)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
				else
					Writer.WriteBits(ObjectSerializer.GetFieldDataTypeCode(LastType), 6);

				foreach (T Item in Array)
				{
					if (Item == null)
					{
						if (Nullable)
							Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
						else
							throw new Exception("Elements cannot be null.");
					}
					else
					{
						ItemType = Item.GetType();
						if (ItemType != LastType)
						{
							S = await this.context.GetObjectSerializer(ItemType);
							LastType = ItemType;
						}

						await S.Serialize(Writer, Nullable, true, Item, State);
					}
				}
			}
		}

		/// <summary>
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <returns>Corresponding field or property value, if found, or null otherwise.</returns>
		public override Task<object> TryGetFieldValue(string FieldName, object Object)
		{
			return Task.FromResult<object>(null);
		}

	}
}
