using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.HTTPX
{
	internal abstract class ChunkRecord : IDisposable
	{
		internal ChunkRecord()
		{
		}

		public abstract void Dispose();

		internal abstract void ChunkReceived(int Nr, bool Last, byte[] Data);
	}
}
