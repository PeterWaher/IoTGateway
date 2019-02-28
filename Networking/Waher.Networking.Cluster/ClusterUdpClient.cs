using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Waher.Events;

namespace Waher.Networking.Cluster
{
	internal class ClusterUdpClient : IDisposable
	{
		private readonly ClusterEndpoint endpoint;
		private readonly LinkedList<byte[]> outputQueue = new LinkedList<byte[]>();
		private readonly IPAddress localAddress;
		private readonly byte[] ivTx = new byte[16];
		private readonly byte[] ivRx = new byte[16];
		private HMACSHA1 hmac;
		private UdpClient client;
		private bool isWriting = false;
		private bool disposed = false;

		internal ClusterUdpClient(ClusterEndpoint Endpoint, UdpClient Client, IPAddress LocalAddress)
		{
			this.endpoint = Endpoint;
			this.client = Client;
			this.localAddress = LocalAddress;

			byte[] A = LocalAddress.GetAddressBytes();
			Array.Copy(A, 0, this.ivTx, 8, 4);

			this.hmac = new HMACSHA1(A);
		}

		internal IPAddress Address => this.localAddress;
		internal IPEndPoint EndPoint => this.client.Client.LocalEndPoint as IPEndPoint;

		public void Dispose()
		{
			this.disposed = true;

			this.client?.Dispose();
			this.client = null;

			this.hmac?.Dispose();
			this.hmac = null;
		}

		internal async void BeginReceive()
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await client.ReceiveAsync();
					if (this.disposed)
						return;

					this.endpoint.Information(Data.Buffer.Length.ToString() + " bytes received. (" + DateTime.Now.TimeOfDay.ToString() + ")");

					try
					{
						byte[] Datagram = Data.Buffer;
						int i, c = Datagram.Length - 12;

						if (c <= 0 || (c & 15) != 0)
							continue;

						long Ticks = BitConverter.ToInt64(Datagram, 0);
						DateTime TP = new DateTime(Ticks, DateTimeKind.Utc);
						if ((DateTime.UtcNow - TP).TotalSeconds >= 10)  // Margin for unsynchronized clocks.
							continue;

						Array.Copy(Datagram, 0, this.ivRx, 0, 8);
						Array.Copy(Datagram, 8, this.ivRx, 12, 4);

						byte[] A = Data.RemoteEndPoint.Address.GetAddressBytes();
						Array.Copy(A, 0, this.ivRx, 8, 4);

						int FragmentNr = this.ivRx[13];
						bool LastFragment = (FragmentNr & 0x80) != 0;
						FragmentNr &= 0x7f;
						FragmentNr <<= 8;
						FragmentNr |= this.ivRx[12];

						int Padding = this.ivRx[14] >> 4;

						using (ICryptoTransform Decryptor = this.endpoint.aes.CreateDecryptor(this.endpoint.key, this.ivRx))
						{
							byte[] Decrypted = Decryptor.TransformFinalBlock(Datagram, 12, Datagram.Length - 12);

							using (HMACSHA1 HMAC = new HMACSHA1(A))
							{
								c = Decrypted.Length - 20 - Padding;

								byte[] MAC = HMAC.ComputeHash(Decrypted, 20, c);

								for (i = 0; i < 20; i++)
								{
									if (MAC[i] != Decrypted[i])
										break;
								}

								if (i < 20)
									continue;

								byte[] Received = new byte[c];
								Array.Copy(Decrypted, 20, Received, 0, c);

								if (LastFragment && FragmentNr == 0)
									this.endpoint.DataReceived(Received, Data.RemoteEndPoint);
								else
								{
									string Key = Data.RemoteEndPoint.ToString() + " " + Ticks.ToString();

									if (!this.endpoint.currentStatus.TryGetValue(Key, out object Obj) ||
										!(Obj is Fragments Fragments))
									{
										Fragments = new Fragments()
										{
											Source = Data.RemoteEndPoint,
											Timestamp = Ticks
										};

										this.endpoint.currentStatus[Key] = Fragments;
									}

									Fragments.Parts[FragmentNr] = Received;

									if (LastFragment)
									{
										Fragments.Done = true;
										Fragments.NrParts = FragmentNr + 1;
									}

									if (Fragments.Done &&
										Fragments.NrParts == Fragments.Parts.Count)
									{
										this.endpoint.currentStatus.Remove(Key);
										this.endpoint.DataReceived(Fragments.ToByteArray(), Fragments.Source);
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						this.endpoint.Error(ex.Message);
						Log.Critical(ex);
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// Closed.
			}
			catch (Exception ex)
			{
				this.endpoint.Error(ex.Message);
			}
		}

		internal async void BeginTransmit(byte[] Message)
		{
			try
			{
				if (this.disposed)
					return;

				lock (this.outputQueue)
				{
					if (this.isWriting)
					{
						this.outputQueue.AddLast(Message);
						return;
					}
					else
						this.isWriting = true;
				}

				while (Message != null)
				{
					int Len = Message.Length;
					int NrFragments = (Len + 32767) >> 15;
					int FragmentNr;
					int Pos = 0;

					if (NrFragments == 0)
						return;

					if (NrFragments >= 32768)
						throw new ArgumentOutOfRangeException("Message too big.", nameof(Message));

					byte[] TP = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
					Array.Copy(TP, 0, this.ivTx, 0, 8);

					for (FragmentNr = 0; FragmentNr < NrFragments; FragmentNr++, Pos += 32768)
					{
						int FragmentSize = Math.Min(32768, Len - (FragmentNr << 15));
						int Padding = (-(20 + FragmentSize)) & 15;
						byte[] Datagram = new byte[32 + FragmentSize + Padding];

						this.ivTx[12] = (byte)FragmentNr;
						this.ivTx[13] = (byte)(FragmentNr >> 8);

						if (FragmentNr == NrFragments - 1)
							this.ivTx[13] |= 0x80;

						this.ivTx[14] = (byte)((this.ivTx[14] & 0x0f) | (Padding << 4));

						Array.Copy(this.ivTx, 0, Datagram, 0, 8);
						Array.Copy(this.ivTx, 12, Datagram, 8, 4);

						byte[] MAC = this.hmac.ComputeHash(Message, Pos, FragmentSize);

						Array.Copy(MAC, 0, Datagram, 12, 20);
						Array.Copy(Message, Pos, Datagram, 32, FragmentSize);

						using (ICryptoTransform Encryptor = this.endpoint.aes.CreateEncryptor(this.endpoint.key, this.ivTx))
						{
							byte[] Encrypted = Encryptor.TransformFinalBlock(Datagram, 12, Datagram.Length - 12);
							Array.Copy(Encrypted, 0, Datagram, 12, Encrypted.Length);
						}

						if (++this.ivTx[15] == 0)
							++this.ivTx[14];

						await this.client.SendAsync(Datagram, Datagram.Length, this.endpoint.destination);

						if (this.disposed)
							return;
					}

					lock (this.outputQueue)
					{
						if (this.outputQueue.First is null)
						{
							this.isWriting = false;
							Message = null;
						}
						else
						{
							Message = this.outputQueue.First.Value;
							this.outputQueue.RemoveFirst();
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.endpoint.Error(ex.Message);

				lock (this.outputQueue)
				{
					this.outputQueue.Clear();
					this.isWriting = false;
				}
			}
			finally
			{
				if (this.endpoint.shuttingDown)
					this.endpoint.Dispose2();
			}
		}
	}
}
