using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;

namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	public class EmbeddedEncrypted
	{
		[ObjectId]
		public string ObjectId;

		[Encrypted]
		public byte Byte;

		[Encrypted(100)]
		public short Short;

		[Encrypted]
		public int Int;

		public EmbeddedEncrypted()
		{
		}
	}
}
