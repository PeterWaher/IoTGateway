using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Cache;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;

namespace Waher.Things.Ieee1451.Ieee1451_1_6
{
	/// <summary>
	/// Helps connect IEEE 1451.1.6 requests and responses across MQTT topics.
	/// </summary>
	public static class MessageSwitch
	{
		private static readonly Cache<string, MessageRec> messages = new Cache<string, MessageRec>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromDays(1));

		private class MessageRec
		{
			public Message Message;
			public DateTime Timestamp;
			public TaskCompletionSource<Message> Pending;
		}

		/// <summary>
		/// Called when new data has been received.
		/// </summary>
		/// <param name="Message">New parsed message.</param>
		/// <param name="NcapId">NCAP ID</param>
		/// <param name="TimId">TIM ID (or null/zeroes if none)</param>
		/// <param name="ChannelId">Channel ID (or 0 if none)</param>
		/// <returns>If response to a pending request was received (true)</returns>
		public static bool DataReported(Message Message, byte[] NcapId, byte[] TimId, ushort ChannelId)
		{
			string Key = GetKey(Message.GetType(), NcapId, TimId, ChannelId);

			lock (messages)
			{
				if (messages.TryGetValue(Key, out MessageRec Rec))
				{
					Rec.Message = Message;
					Rec.Timestamp = DateTime.UtcNow;

					if (!(Rec.Pending is null))
					{
						Rec.Pending.TrySetResult(Message);
						Rec.Pending = null;
						return true;
					}
				}
				else
				{
					messages[Key] = new MessageRec()
					{
						Message = Message,
						Timestamp = DateTime.UtcNow,
						Pending = null
					};
				}
			}

			return false;
		}

		private static string GetKey(Type T, byte[] NcapId, byte[] TimId, ushort ChannelId)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(T.Name);
			sb.Append('/');
			sb.Append(Convert.ToBase64String(NcapId));

			if (!IsZero(TimId))
			{
				sb.Append('/');
				sb.Append(Convert.ToBase64String(TimId));

				if (ChannelId > 0)
				{
					sb.Append('/');
					sb.Append(ChannelId.ToString());
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Checks if an ID is "zero", i.e. contains only zero bytes.
		/// </summary>
		/// <param name="A"></param>
		/// <returns></returns>
		public static bool IsZero(byte[] A)
		{
			if (A is null)
				return true;

			foreach (byte b in A)
			{
				if (b != 0)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Waits for a message to be received.
		/// </summary>
		/// <typeparam name="T">Type of message expected.</typeparam>
		/// <param name="StaleLimitSeconds">A received message is considered stale, if
		/// older than this number of seconds.</param>
		/// <param name="TimeoutMilliseconds">Maximum amount of time to wait for the message.</param>
		/// <returns>Message</returns>
		/// <param name="NcapId">NCAP ID</param>
		/// <param name="TimId">TIM ID (or null/zeroes if none)</param>
		/// <param name="ChannelId">Channel ID (or 0 if none)</param>
		/// <exception cref="TimeoutException">If no message is received within the
		/// prescribed time.</exception>
		public static async Task<T> WaitForMessage<T>(int TimeoutMilliseconds, int StaleLimitSeconds, 
			byte[] NcapId, byte[] TimId, ushort ChannelId)
			where T : Message
		{
			TaskCompletionSource<Message> Pending = new TaskCompletionSource<Message>();
			TaskCompletionSource<Message> Obsolete = null;
			string Key = GetKey(typeof(T), NcapId, TimId, ChannelId);

			lock (messages)
			{
				if (messages.TryGetValue(Key, out MessageRec Rec))
				{
					if (!(Rec.Message is null) && DateTime.UtcNow.Subtract(Rec.Timestamp).TotalSeconds < StaleLimitSeconds)
						return (T)Rec.Message;

					Obsolete = Rec.Pending;
					Rec.Pending = Pending;
				}
				else
				{
					messages[Key] = new MessageRec()
					{
						Message = null,
						Timestamp = DateTime.MinValue,
						Pending = Pending
					};
				}
			}

			_ = Task.Delay(TimeoutMilliseconds).ContinueWith(_ =>
			{
				Pending.TrySetResult(null);
			});

			Message Result = await Pending.Task;

			if (Result is null)
			{
				lock (messages)
				{
					if (messages.TryGetValue(Key, out MessageRec Rec) && Rec.Pending == Pending)
						Rec.Pending = null;
				}

				throw new TimeoutException();
			}

			return (T)Result;
		}

	}
}
