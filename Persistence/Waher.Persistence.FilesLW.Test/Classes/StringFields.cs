using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	[TypeName(TypeNameSerialization.FullName)]
	[CollectionName("StringFields")]
	[Index("A", "B")]
	public class StringFields
	{
		[ObjectId]
		public Guid ObjectId;
		[DefaultValueStringEmpty]
		public string A = string.Empty;
		[DefaultValueStringEmpty]
		public string B = string.Empty;
		[DefaultValueStringEmpty]
		public string C = string.Empty;

		public StringFields()
		{
		}

		public override string ToString()
		{
			return string.Format("A={0}, B={1}, C={2}", this.A, this.B, this.C);
		}
	}
}
