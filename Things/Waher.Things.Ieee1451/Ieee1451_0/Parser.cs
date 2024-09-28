using Waher.Things.Ieee1451.Ieee1451_0.Raw;

namespace Waher.Things.Ieee1451.Ieee1451_0
{
	/// <summary>
	/// Parses IEEE 1451.0 messages.
	/// </summary>
	public static class Parser
	{
		/// <summary>
		/// Tries to parse IEEE 1451.0-encoded data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="Message">Raw Message, if parsed</param>
		/// <returns>If able to parse the raw message.</returns>
		public static bool TryParseRaw(byte[] Data, out RawMessage Message)
		{
			NetworkServiceType NetworkServiceType = 0;
			MessageType MessageType = 0;
			byte NetworkServiceId = 0;
			int Length = 0;
			int TailLength = 0;
			byte[] Body = null;
			byte[] Tail = null;
			int i = 0;
			int c = Data.Length;
			int Pos = 0;
			int State = 0;
			byte b;

			Message = null;

			while (i < c)
			{
				b = Data[i++];

				switch (State)
				{
					case 0: // Network Service Type
						if (b < 1 || b > 4)
							return false;

						NetworkServiceType = (NetworkServiceType)b;
						State += b;
						break;

					case 1: // Discovery services
						if (b < 1 || b > 10)
							return false;

						NetworkServiceId = b;
						State = 5;
						break;

					case 2: // Transducer Access Service
						if (b < 1 || b > 20)
							return false;

						NetworkServiceId = b;
						State = 5;
						break;

					case 3: // TEDS Access Service
						if (b < 1 || b > 4)
							return false;

						NetworkServiceId = b;
						State = 5;
						break;

					case 4: // Event Notification Service
						if (b < 1 || b > 12)
							return false;

						NetworkServiceId = b;
						State = 5;
						break;

					case 5: // Message Type
						MessageType = (MessageType)b;
						State++;
						break;

					case 6: // Length MSB
						Length = b;
						State++;
						break;

					case 7: // Length LSB
						Length <<= 8;
						Length |= b;

						TailLength = c - Length;
						Length -= i;
						if (Length < 0 || TailLength < 0)
							return false;

						Body = new byte[Length];
						Tail = new byte[TailLength];

						if (Length > 0)
							State++;
						else if (TailLength > 0)
							State += 2;
						else
							State += 3;
						break;

					case 8:
						Body[Pos++] = b;
						Length--;
						if (Length == 0)
						{
							if (TailLength > 0)
							{
								Pos = 0;
								State++;
							}
							else
								State += 2;
						}
						break;

					case 9:
						Tail[Pos++] = b;
						TailLength--;
						if (TailLength == 0)
							State++;
						break;

					case 10:
					default:
						return false;
				}
			}

			if (State != 10)
				return false;

			switch (NetworkServiceType)
			{
				case NetworkServiceType.DiscoveryServices:
					Message = new DiscoveryMessage(NetworkServiceType, (DiscoveryService)NetworkServiceId, MessageType, Body, Tail);
					return true;

				case NetworkServiceType.TransducerAccessServices:
					Message = new TransducerAccessMessage(NetworkServiceType, (TransducerAccessService)NetworkServiceId, MessageType, Body, Tail);
					return true;

				case NetworkServiceType.TedsAccessServices:
					Message = new TedsAccessMessage(NetworkServiceType, (TedsAccessService)NetworkServiceId, MessageType, Body, Tail);
					return true;

				case NetworkServiceType.EventNotificationServices:
					Message = new EventNotificationMessage(NetworkServiceType, (EventNotificationService)NetworkServiceId, MessageType, Body, Tail);
					return true;

				default:
					return false;
			}
		}
	}
}
