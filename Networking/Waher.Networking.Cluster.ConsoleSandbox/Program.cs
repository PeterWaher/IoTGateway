using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;

namespace Waher.Networking.Cluster.ConsoleSandbox
{
	class Program
	{
		static void Main(string[] _)
		{
			try
			{
				Types.Initialize(typeof(ClusterEndpoint).Assembly, typeof(Program).Assembly);

				ConsoleOut.WriteLine("Welcome to the cluster sandbox.");
				ConsoleOut.WriteLine();
				ConsoleOut.Write("Multicast address (default=239.255.0.0): ");
				string s = ConsoleIn.ReadLine();
				if (string.IsNullOrEmpty(s))
					s = "239.255.0.0";

				IPAddress Address = IPAddress.Parse(s);

				ConsoleOut.Write("Port (default=12345): ");
				s = ConsoleIn.ReadLine();
				if (string.IsNullOrEmpty(s))
					s = "12345";

				int Port = int.Parse(s);

				ConsoleOut.Write("Secret: ");
				s = ConsoleIn.ReadLine();

				Dictionary<string, bool> Locked = new();
				Dictionary<IPEndPoint, string> Names = new();
				string Name = Dns.GetHostName();

				using ClusterEndpoint Endpoint = new(Address, Port, s);

				ConsoleOut.WriteLine();
				ConsoleOut.WriteLine("You are now connected to the cluster.");
				ConsoleOut.WriteLine("Pressing a letter or a digit will lock/release a corresponding resource.");
				ConsoleOut.WriteLine("Press ESC to quit the application.");

				Endpoint.GetStatus += (sender, e) =>
				{
					e.Status = Name;
					return Task.CompletedTask;
				};

				Endpoint.EndpointOnline += (sender, e) =>
				{
					lock (Names)
					{
						Names[e.Endpoint] = string.Empty;
					}

					ConsoleOut.WriteLine("New endpoint online: " + e.Endpoint.ToString());

					return Task.CompletedTask;
				};

				Endpoint.EndpointOffline += (sender, e) =>
				{
					string RemoteName;

					lock (Names)
					{
						if (!Names.TryGetValue(e.Endpoint, out RemoteName))
							RemoteName = string.Empty;
					}

					ConsoleOut.WriteLine("Endpoint offline: " + e.Endpoint.ToString() + " (" + RemoteName + ")");

					return Task.CompletedTask;
				};

				Endpoint.EndpointStatus += (sender, e) =>
				{
					if (e.Status is string RemoteName)
					{
						bool New;

						lock (Names)
						{
							New = !Names.TryGetValue(e.Endpoint, out string s2) || RemoteName != s2;

							if (New)
								Names[e.Endpoint] = RemoteName;
						}

						if (New)
							ConsoleOut.WriteLine(e.Endpoint.ToString() + " is " + RemoteName);
					}

					return Task.CompletedTask;
				};

				ConsoleKeyInfo KeyInfo = ConsoleIn.ReadKey(true);
				while (KeyInfo.Key != ConsoleKey.Escape)
				{
					char ch = KeyInfo.KeyChar;
					if (char.IsLetterOrDigit(ch))
					{
						s = new string(ch, 1);

						if (Locked.ContainsKey(s))
						{
							ConsoleOut.WriteLine("Releasing " + s + ".");
							Endpoint.Release(s).Wait();
							ConsoleOut.WriteLine(s + " released.");
							Locked.Remove(s);
						}
						else
						{
							ConsoleOut.WriteLine("Locking " + s + ".");

							Endpoint.Lock(s, 10000, (sender, e) =>
							{
								if (e.LockSuccessful)
								{
									ConsoleOut.WriteLine(e.State.ToString() + " locked.");
									Locked[e.State.ToString()] = true;
								}
								else if (e.LockedBy is null)
									ConsoleOut.WriteLine(e.State.ToString() + " is already locked.");
								else
									ConsoleOut.WriteLine(e.State.ToString() + " is already locked by " + e.LockedBy.ToString());

								return Task.CompletedTask;
							}, s);
						}
					}

					KeyInfo = ConsoleIn.ReadKey(true);
				}
			}
			catch (Exception ex)
			{
				ConsoleOut.WriteLine(ex.Message);
			}
			finally
			{
				ConsoleOut.Flush(true);
			}
		}
	}
}