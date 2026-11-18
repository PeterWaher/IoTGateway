using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Things.Matter.Controller;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.Onboarding;

namespace Waher.Things.Matter.Test.Devices
{
	/// <summary>
	/// Runs a Matter test device, based on the open-source matter.js reference implementation, in a separate
	/// Node.js process. The device uses the publicly known test credentials of the Matter SDK (test vendor ID 0xFFF1,
	/// test device attestation certificates and test certification declaration).
	/// </summary>
	public sealed class MatterTestDevice : IDisposable
	{
		private readonly List<string> output = new List<string>();
		private readonly Dictionary<string, TaskCompletionSource<string>> pendingCommands = new Dictionary<string, TaskCompletionSource<string>>();
		private readonly TaskCompletionSource<string> ready = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
		private readonly string storageFolder;
		private Process process;

		private MatterTestDevice(string Id, int Port, uint Passcode, ushort Discriminator, string StorageFolder)
		{
			this.Id = Id;
			this.Port = Port;
			this.Passcode = Passcode;
			this.Discriminator = Discriminator;
			this.storageFolder = StorageFolder;
		}

		/// <summary>
		/// ID of the device.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// UDP port of the device.
		/// </summary>
		public int Port { get; }

		/// <summary>
		/// Setup passcode.
		/// </summary>
		public uint Passcode { get; }

		/// <summary>
		/// Discriminator.
		/// </summary>
		public ushort Discriminator { get; }

		/// <summary>
		/// Manual pairing code, as reported by the device.
		/// </summary>
		public string ManualCode { get; private set; }

		/// <summary>
		/// QR code payload, as reported by the device.
		/// </summary>
		public string QrCode { get; private set; }

		/// <summary>
		/// Local IPv4 address and port of the device, for commissioning without DNS-SD.
		/// </summary>
		public IPEndPoint LocalEndPoint => new IPEndPoint(IPAddress.Loopback, this.Port);

		/// <summary>
		/// Starts a test device.
		/// </summary>
		/// <param name="Id">ID of device. Must be unique among concurrently running devices.</param>
		/// <param name="Bridge">If the device should act as a bridge.</param>
		/// <param name="Simulate">If sensor values should be simulated (updated periodically).</param>
		/// <returns>Test device.</returns>
		public static async Task<MatterTestDevice> StartAsync(string Id, bool Bridge = false, bool Simulate = false)
		{
			string Folder = await MatterJs.GetScriptFolderAsync();
			string StorageFolder = Path.Combine(AppContext.BaseDirectory, "TestDevices", "storage", Id);

			if (Directory.Exists(StorageFolder))
				Directory.Delete(StorageFolder, true);

			Directory.CreateDirectory(StorageFolder);

			int Port = MatterJs.GetFreeUdpPort();
			uint Passcode = MatterNode.GenerateRandomPasscode();
			ushort Discriminator = (ushort)(MatterCrypto.RandomUInt16() & 0x0fff);

			MatterTestDevice Device = new MatterTestDevice(Id, Port, Passcode, Discriminator, StorageFolder);
			await Device.StartProcessAsync(Folder, Bridge, Simulate);

			return Device;
		}

		private async Task StartProcessAsync(string Folder, bool Bridge, bool Simulate)
		{
			ProcessStartInfo StartInfo = MatterJs.CreateStartInfo("node", MatterJs.ScriptFileName, Folder);

			StartInfo.Environment["TEST_DEVICE_ID"] = this.Id;
			StartInfo.Environment["TEST_DEVICE_PORT"] = this.Port.ToString();
			StartInfo.Environment["TEST_DEVICE_PASSCODE"] = this.Passcode.ToString();
			StartInfo.Environment["TEST_DEVICE_DISCRIMINATOR"] = this.Discriminator.ToString();
			StartInfo.Environment["TEST_DEVICE_BRIDGE"] = Bridge ? "1" : "0";
			StartInfo.Environment["TEST_DEVICE_SIMULATE"] = Simulate ? "1" : "0";
			StartInfo.Environment["MATTER_STORAGE_PATH"] = this.storageFolder;
			StartInfo.Environment["MATTER_LOG_LEVEL"] = "warn";

			this.process = new Process()
			{
				StartInfo = StartInfo,
				EnableRaisingEvents = true
			};

			this.process.OutputDataReceived += this.Process_OutputDataReceived;
			this.process.ErrorDataReceived += this.Process_ErrorDataReceived;
			this.process.Exited += (Sender, e) => this.ready.TrySetException(new Exception("Test device process exited."));

			if (!this.process.Start())
				Assert.Inconclusive("Unable to start test device process.");

			this.process.BeginOutputReadLine();
			this.process.BeginErrorReadLine();

			Task Timeout = Task.Delay(60000);
			Task Result = await Task.WhenAny(this.ready.Task, Timeout);

			if (Result != this.ready.Task || this.ready.Task.IsFaulted)
			{
				string Log = this.GetOutput();
				this.Dispose();

				throw new Exception("Test device did not start properly.\r\n" + Log);
			}

			string[] Parts = this.ready.Task.Result.Split(' ');

			this.ManualCode = Parts.Length > 1 ? Parts[1] : SetupPayload.FormatManualCode(new SetupPayload()
			{
				Discriminator = this.Discriminator,
				Passcode = this.Passcode
			}.ToManualCode());

			this.QrCode = Parts.Length > 2 ? Parts[2] : null;

			Console.Out.WriteLine("Test device " + this.Id + " started on port " + this.Port.ToString() + ". Manual code: " +
				this.ManualCode + ", QR code: " + this.QrCode);
		}

		private void Process_OutputDataReceived(object Sender, DataReceivedEventArgs e)
		{
			string s = e.Data;
			if (s is null)
				return;

			lock (this.output)
			{
				this.output.Add(s);

				if (this.output.Count > 1000)
					this.output.RemoveAt(0);
			}

			if (s.StartsWith("READY "))
				this.ready.TrySetResult(s);
			else if (s.StartsWith("OK ") || s.StartsWith("ERROR "))
			{
				int i = s.IndexOf(' ');
				string Rest = s.Substring(i + 1);
				int j = Rest.IndexOf(':');
				string Command = j < 0 ? Rest.Trim() : Rest.Substring(0, j).Trim();
				TaskCompletionSource<string> Pending;

				lock (this.pendingCommands)
				{
					if (this.pendingCommands.TryGetValue(Command, out Pending))
						this.pendingCommands.Remove(Command);
				}

				if (s.StartsWith("OK "))
					Pending?.TrySetResult(s);
				else
					Pending?.TrySetException(new Exception(s));
			}
		}

		private void Process_ErrorDataReceived(object Sender, DataReceivedEventArgs e)
		{
			if (e.Data is null)
				return;

			lock (this.output)
			{
				this.output.Add(e.Data);

				if (this.output.Count > 1000)
					this.output.RemoveAt(0);
			}
		}

		/// <summary>
		/// Gets the output of the test device process.
		/// </summary>
		/// <returns>Output.</returns>
		public string GetOutput()
		{
			StringBuilder sb = new StringBuilder();

			lock (this.output)
			{
				foreach (string Line in this.output)
					sb.AppendLine(Line);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Sends a command to the test device (for instance, to change a sensor value).
		/// </summary>
		/// <param name="Command">Command, e.g. "temperature 23.5".</param>
		public async Task SendCommandAsync(string Command)
		{
			string Name = Command.Split(' ')[0];
			TaskCompletionSource<string> Pending = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

			lock (this.pendingCommands)
			{
				this.pendingCommands[Name] = Pending;
			}

			await this.process.StandardInput.WriteLineAsync(Command);
			await this.process.StandardInput.FlushAsync();

			Task Timeout = Task.Delay(10000);
			if (await Task.WhenAny(Pending.Task, Timeout) != Pending.Task)
				throw new TimeoutException("Test device did not respond to command: " + Command);

			await Pending.Task;
		}

		/// <summary>
		/// Stops the test device.
		/// </summary>
		public void Dispose()
		{
			if (!(this.process is null))
			{
				try
				{
					if (!this.process.HasExited)
					{
						this.process.StandardInput.WriteLine("quit");
						this.process.StandardInput.Flush();

						if (!this.process.WaitForExit(5000))
							this.process.Kill(true);
					}
				}
				catch (Exception)
				{
					try
					{
						this.process.Kill(true);
					}
					catch (Exception)
					{
						// Ignore
					}
				}

				this.process.Dispose();
				this.process = null;
			}

			try
			{
				if (Directory.Exists(this.storageFolder))
					Directory.Delete(this.storageFolder, true);
			}
			catch (Exception)
			{
				// Ignore
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Id + " (port " + this.Port.ToString() + ")";
		}
	}
}
