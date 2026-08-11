using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Console;
using Waher.Security;

namespace Waher.Networking.HTTP.JsonRpc.Transports
{
	/// <summary>
	/// JSON-RPC via Standard Input/Output (stdio).
	/// </summary>
	public class StdioJsonRpc
	{
		private readonly TaskCompletionSource<bool> completed = new TaskCompletionSource<bool>();
		private readonly JsonRpcWebService Service;
		private readonly ICommunicationLayer server;
		private readonly IUser user;
		private readonly string baseUrl;
		private bool running = false;

		/// <summary>
		/// JSON-RPC via Standard Input/Output (stdio).
		/// </summary>
		/// <param name="Service">JSON-RPC web service.</param>
		/// <param name="User">Authenticated user making the call.</param>
		/// <param name="Server">Server managing calls.</param>
		/// <param name="BaseUrl">Base URL of the web service.</param>
		public StdioJsonRpc(JsonRpcWebService Service, IUser User,
			ICommunicationLayer Server, string BaseUrl)
		{
			this.Service = Service;
			this.user = User;
			this.server = Server;
			this.baseUrl = BaseUrl;



			Task.Run(this.PerformWork);
		}

		/// <summary>
		/// If the process is running.
		/// </summary>
		public bool Running => this.running;

		private async void PerformWork()
		{
			try
			{
				this.running = true;

				while (true)
				{
					string? Input = await ConsoleIn.ReadLineAsync();
					if (Input is null)
						break;

					string Output = await this.Service.ExecuteJsonRpc(Input,
						this.user, this.server, this.baseUrl, this.EventReceived);

					await ConsoleOut.WriteLineAsync(Output);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				this.running = false;
			}
			finally
			{
				this.completed.SetResult(true);
			}
		}

		private async Task EventReceived(object _, NotificationEventArgs Event)
		{
			string? Data = Event["data"] as string;

			if (!string.IsNullOrEmpty(Data))
				await ConsoleOut.WriteLineAsync(Data);
		}

		/// <summary>
		/// Waits for the process to complete.
		/// </summary>
		public Task WaitAsync()
		{
			return this.completed.Task;
		}
	}
}
