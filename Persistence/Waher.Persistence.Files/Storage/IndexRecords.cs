using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Storage
{
	internal class IndexRecords : IRecordHandler
	{
		private string[] fieldNames;

		public IndexRecords(params string[] FieldNames)
		{
			this.fieldNames = FieldNames;
		}

		internal byte[] Serialize(Guid Guid, object Object, ObjectSerializer Serializer)
		{
			throw new NotImplementedException();
		}

		public uint GetFullPayloadSize(BinaryDeserializer Reader)
		{
			throw new NotImplementedException();
		}

		public IComparable GetKey(BinaryDeserializer Reader)
		{
			throw new NotImplementedException();
		}

		public int GetPayloadSize(BinaryDeserializer Reader)
		{
			throw new NotImplementedException();
		}

		public int GetPayloadSize(BinaryDeserializer Reader, out bool IsBlob)
		{
			throw new NotImplementedException();
		}

		public string GetPayloadType(BinaryDeserializer Reader)
		{
			throw new NotImplementedException();
		}

		public bool SkipKey(BinaryDeserializer Reader)
		{
			throw new NotImplementedException();
		}
	}
}
