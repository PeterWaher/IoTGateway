using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ReferenceTypes
{
	/// <summary>
	/// Generic serializer of array types.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	public class ArraySerializer<T> : GeneratedObjectSerializerBase
	{
		private FilesProvider provider;

		public ArraySerializer(FilesProvider Provider)
		{
			this.provider = Provider;
		}

		public override Type ValueType
		{
			get
			{
				return typeof(T[]);
			}
		}

		public override bool IsNullable
		{
			get { return true; }
		}

		public override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			return ReadArray<T>(this.provider, Reader, DataType.Value);
		}

		public override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (Value == null)
			{
				if (!WriteTypeCode)
					throw new NullReferenceException("Value cannot be null.");

				Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
			}
			else
			{
				T[] Array = (T[])Value;
				Type LastType = typeof(T);
				IObjectSerializer S = this.provider.GetObjectSerializer(LastType);
				Type ItemType;
				bool Nullable;

				Writer.WriteBits(ObjectSerializer.TYPE_ARRAY, 6);
				Writer.WriteVariableLengthUInt64((ulong)Array.Length);

				if (Nullable = S.IsNullable)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
				else
					Writer.WriteBits(FilesProvider.GetFieldDataTypeCode(LastType), 6);

				foreach (T Item in Array)
				{
					if (Item == null)
						Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					else
					{
						ItemType = Item.GetType();
						if (ItemType != LastType)
						{
							S = this.provider.GetObjectSerializer(ItemType);
							LastType = ItemType;
						}

						S.Serialize(Writer, Nullable, true, Item);
					}
				}
			}
		}

	}
}
