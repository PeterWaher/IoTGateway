using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS
{
    internal class ResendableRecord
    {
		public ContentType Type;
		public byte[] Fragment;
		public bool More;
	}
}
