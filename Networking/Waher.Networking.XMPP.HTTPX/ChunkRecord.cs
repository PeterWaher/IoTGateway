using System;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Networking.XMPP.HTTPX
{
	internal abstract class ChunkRecord : IDisposable
	{
		private int id = 0;

		internal ChunkRecord()
		{
		}

		[Obsolete("Use the DisposeAsync() method.")]
		public async void Dispose()
		{
			try
			{
				await this.DisposeAsync();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		public abstract Task DisposeAsync();

		internal abstract Task<bool> ChunkReceived(int Nr, bool Last, byte[] Data);
		internal abstract Task Fail(string Message);

		internal int NextId()
		{
			return this.id++;
		}
	}
}
