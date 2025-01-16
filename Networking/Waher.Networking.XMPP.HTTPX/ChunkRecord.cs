using System;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Networking.XMPP.HTTPX
{
	internal abstract class ChunkRecord : IDisposableAsync
	{
		private int id = 0;

		internal ChunkRecord()
		{
		}

		[Obsolete("Use the DisposeAsync() method.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		public abstract Task DisposeAsync();

		internal abstract Task<bool> ChunkReceived(int Nr, bool Last, bool ConstantBuffer, byte[] Data);
		internal abstract Task Fail(string Message);

		internal int NextId()
		{
			return this.id++;
		}
	}
}
