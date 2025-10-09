﻿using System;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	public class ContainerEncrypted
	{
		[ObjectId]
		public Guid ObjectId;
		public EmbeddedEncrypted Embedded;
		public EmbeddedEncrypted EmbeddedNull;
		public EmbeddedEncrypted[] MultipleEmbedded;
		public EmbeddedEncrypted[] MultipleEmbeddedNullable;
		public EmbeddedEncrypted[] MultipleEmbeddedNull;
	}
}
