using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Console;
using Waher.Security;

namespace Waher.Networking.HTTP.JsonRpc.Transports
{
	/// <summary>
	/// Delegate for reading lines of text.
	/// </summary>
	/// <returns>Line read, or null if no more lines.</returns>
	public delegate Task<string> ReadLineAsyncDelegate();

	/// <summary>
	/// Delegate for writing lines of text.
	/// </summary>
	/// <param name="Line">Line to write.</param>
	public delegate Task WriteLineAsyncDelegate(string Line);

	/// <summary>
	/// JSON-RPC via Standard Input/Output (stdio).
	/// </summary>
	public class StdioJsonRpc
	{
		private readonly ReadLineAsyncDelegate readLine;
		private readonly WriteLineAsyncDelegate writeLine;
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
			: this(Service,User,Server,BaseUrl,
				  ConsoleIn.ReadLineAsync,
				  ConsoleOut.WriteLineAsync)
		{
		}

		/// <summary>
		/// JSON-RPC via Standard Input/Output (stdio).
		/// </summary>
		/// <param name="Service">JSON-RPC web service.</param>
		/// <param name="User">Authenticated user making the call.</param>
		/// <param name="Server">Server managing calls.</param>
		/// <param name="BaseUrl">Base URL of the web service.</param>
		/// <param name="ReadLine">Method to call to read a line of text.</param>
		/// <param name="WriteLine">Method to call to write a line of text.</param>
		public StdioJsonRpc(JsonRpcWebService Service, IUser User,
			ICommunicationLayer Server, string BaseUrl,
			ReadLineAsyncDelegate ReadLine, WriteLineAsyncDelegate WriteLine)
		{
			this.Service = Service;
			this.user = User;
			this.server = Server;
			this.baseUrl = BaseUrl;
			this.readLine = ReadLine;
			this.writeLine = WriteLine;

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
					string? Input = await this.readLine();
					if (Input is null)
						break;

					string Output = await this.Service.ExecuteJsonRpc(Input,
						this.user, this.server, this.baseUrl, this.EventReceived);

					await this.writeLine(Output);
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
