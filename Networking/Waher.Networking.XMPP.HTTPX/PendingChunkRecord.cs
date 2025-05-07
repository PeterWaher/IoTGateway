using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Networking.XMPP.HTTPX
{
	internal class PendingChunkRecord : ChunkRecord
	{
		private readonly ChunkedList<ChunkEvent> pending = new ChunkedList<ChunkEvent>();

		internal PendingChunkRecord()
			: base()
		{
		}

		public override Task DisposeAsync()
		{
			return Task.CompletedTask;
		}

		private abstract class ChunkEvent
		{
			public abstract Task Replay(ChunkRecord To);
		}

		private class ChunkReceivedEvent : ChunkEvent
		{
			public int Nr;
			public bool Last;
			public bool ConstantBuffer;
			public byte[] Data;

			public override Task Replay(ChunkRecord To)
			{
				return To.ChunkReceived(this.Nr, this.Last, this.ConstantBuffer, this.Data);
			}
		}

		private class ChunkFailEvent : ChunkEvent
		{
			public string Message;

			public override Task Replay(ChunkRecord To)
			{
				return To.Fail(this.Message);
			}
		}

		internal override Task<bool> ChunkReceived(int Nr, bool Last, bool ConstantBuffer, byte[] Data)
		{
			this.pending.Add(new ChunkReceivedEvent()
			{
				Nr = Nr,
				Last = Last,
				ConstantBuffer = ConstantBuffer,
				Data = Data
			});

			return Task.FromResult(true);
		}

		internal override Task Fail(string Message)
		{
			this.pending.Add(new ChunkFailEvent()
			{
				Message = Message
			});

			return Task.CompletedTask;
		}

		internal async Task Replay(ChunkRecord To)
		{
			foreach (ChunkEvent E in this.pending)
				await E.Replay(To);

			this.pending.Clear();
		}
	}
}
