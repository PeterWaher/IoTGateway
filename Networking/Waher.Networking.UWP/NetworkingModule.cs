using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Networking
{
	/// <summary>
	/// Module that controls the life cycle of communication.
	/// </summary>
	public class NetworkingModule : IGracefulModule
	{
		private static readonly Dictionary<Guid, CancellationTokenSource> sourcesById = new Dictionary<Guid, CancellationTokenSource>();
		private static bool running = false;
		private static bool stopping = false;

		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			running = true;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Prepares the module for being stopped.
		/// </summary>
		public Task PrepareStop()
		{
			stopping = true;
			running = false;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			lock (sourcesById)
			{
				foreach (CancellationTokenSource Source in sourcesById.Values)
				{
					try
					{
						Source.Cancel();
					}
					catch (Exception)
					{
						// Ignore exceptions.
					}
				}

				sourcesById.Clear();
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// If the system is runnning.
		/// </summary>
		public static bool Running => running;

		/// <summary>
		/// If the system is stopping.
		/// </summary>
		public static bool Stopping => stopping;

		/// <summary>
		/// Registers a cancellation token source.
		/// </summary>
		/// <param name="Id">ID of cancellation token.</param>
		/// <param name="Token">Token source</param>
		public static void RegisterToken(Guid Id, CancellationTokenSource Token)
		{
			lock (sourcesById)
			{
				sourcesById[Id] = Token;
			}
		}

		/// <summary>
		/// Unregisters a cancellation token source.
		/// </summary>
		/// <param name="Id">ID of cancellation token.</param>
		/// <returns>If token source was found and removed.</returns>
		public static bool UnregisterToken(Guid Id)
		{
			lock (sourcesById)
			{
				return sourcesById.Remove(Id);
			}
		}

	}
}
