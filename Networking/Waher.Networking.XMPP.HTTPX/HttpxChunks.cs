using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.Events;
using Waher.Runtime.Cache;
using Waher.Script.Constants;

namespace Waher.Networking.XMPP.HTTPX
{
	internal static class HttpxChunks
	{
		private static readonly Dictionary<XmppClient, int> registrationsPerClient = new Dictionary<XmppClient, int>();
		private static readonly SemaphoreSlim chunkSynchObj = new SemaphoreSlim(1);
		private static readonly Cache<string, ChunkRecord> chunkedStreams;

		static HttpxChunks()
		{
			chunkedStreams = new Cache<string, ChunkRecord>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromMinutes(1), true);
			chunkedStreams.Removed += CacheItem_Removed;

			Log.Terminating += (sender, e) =>
			{
				chunkedStreams.Clear();
				chunkedStreams.Dispose();

				return Task.CompletedTask;
			};
		}

		internal static void RegisterChunkReceiver(XmppClient Client)
		{
			lock (registrationsPerClient)
			{
				if (registrationsPerClient.TryGetValue(Client, out int i))
					registrationsPerClient[Client] = i + 1;
				else
				{
					registrationsPerClient[Client] = 1;
					Client.RegisterMessageHandler("chunk", HttpxClient.Namespace, ChunkReceived, true);
				}
			}
		}

		internal static void UnregisterChunkReceiver(XmppClient Client)
		{
			lock (registrationsPerClient)
			{
				if (registrationsPerClient.TryGetValue(Client, out int i))
				{
					if (i > 1)
						registrationsPerClient[Client] = i - 1;
					else
					{
						registrationsPerClient.Remove(Client);
						Client.UnregisterMessageHandler("chunk", HttpxClient.Namespace, ChunkReceived, true);
					}
				}
			}
		}

		private static async Task CacheItem_Removed(object Sender, CacheItemEventArgs<string, ChunkRecord> e)
		{
			try
			{
				await e.Value.DisposeAsync();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		internal static async Task ChunkReceived(object Sender, MessageEventArgs e)
		{
			string StreamId = XML.Attribute(e.Content, "streamId");
			string Key = e.From + " " + StreamId;

			int Nr = XML.Attribute(e.Content, "nr", 0);
			if (Nr < 0)
				return;

			bool Last = XML.Attribute(e.Content, "last", false);
			byte[] Data = Convert.FromBase64String(e.Content.InnerText);

			await chunkSynchObj.WaitAsync();
			try
			{
				if (!chunkedStreams.TryGetValue(Key, out ChunkRecord Rec))
				{
					Rec = new PendingChunkRecord();
					chunkedStreams[Key] = Rec;
				}

				if (!await Rec.ChunkReceived(Nr, Last, true, Data))
				{
					await Rec.DisposeAsync();
					chunkedStreams.Remove(Key);
				}
			}
			finally
			{
				chunkSynchObj.Release();
			}
		}

		internal static async Task<PendingChunkRecord> Add(string Key, ChunkRecord Rec)
		{
			await chunkSynchObj.WaitAsync();
			try
			{
				if (!chunkedStreams.TryGetValue(Key, out ChunkRecord OldRec) ||
					!(OldRec is PendingChunkRecord PendingRec))
				{
					PendingRec = null;
				}

				chunkedStreams[Key] = Rec;

				return PendingRec;
			}
			finally
			{
				chunkSynchObj.Release();
			}
		}

		internal static async Task<bool> Cancel(string Key)
		{
			await chunkSynchObj.WaitAsync();
			try
			{
				return chunkedStreams.Remove(Key);
			}
			finally
			{
				chunkSynchObj.Release();
			}
		}

		internal static async Task<bool> Contains(string Key)
		{
			await chunkSynchObj.WaitAsync();
			try
			{
				return chunkedStreams.ContainsKey(Key);
			}
			finally
			{
				chunkSynchObj.Release();
			}
		}

		internal static async Task<ChunkRecord> TryGetRecord(string Key, bool Remove)
		{
			await chunkSynchObj.WaitAsync();
			try
			{
				if (chunkedStreams.TryGetValue(Key, out ChunkRecord Rec))
				{
					if (Remove)
						chunkedStreams.Remove(Key);

					return Rec;
				}
				else
					return null;
			}
			finally
			{
				chunkSynchObj.Release();
			}
		}

		internal static async Task<bool> Received(string Key, int Nr, bool Last, bool ConstantBuffer, byte[] Data)
		{
			await chunkSynchObj.WaitAsync();
			try
			{
				if (chunkedStreams.TryGetValue(Key, out ChunkRecord Rec))
				{
					await Rec.ChunkReceived(Nr, Last, ConstantBuffer, Data ?? Array.Empty<byte>());
					return true;
				}
				else
					return false;
			}
			finally
			{
				chunkSynchObj.Release();
			}
		}

	}
}
