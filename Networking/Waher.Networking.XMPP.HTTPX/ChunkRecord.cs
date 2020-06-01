using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.HTTPX
{
	internal abstract class ChunkRecord : IDisposable
	{
		private int id = 0;

		internal ChunkRecord()
		{
		}

		public abstract void Dispose();

		internal abstract bool ChunkReceived(int Nr, bool Last, byte[] Data);
		internal abstract void Fail(string Message);

		internal int NextId()
		{
			return this.id++;
		}
	}
}
