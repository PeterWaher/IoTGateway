using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;

namespace Waher.Networking.MQTT.Test
{
	[TestClass]
	public abstract class MqttTests
	{
		private MqttClient client;

		[TestInitialize]
		public void TestInitialize()
		{
			this.client = new MqttClient("mqtt.eclipse.org", this.Port, this.Encypted, "UnitTest", string.Empty,
				new TextWriterSniffer(Console.Out, BinaryPresentationMethod.Hexadecimal));
		}

		public abstract bool Encypted
		{
			get;
		}

		public abstract int Port
		{
			get;
		}

		[TestCleanup]
		public void TestCleanup()
		{
			this.client?.Dispose();
			this.client = null;
		}

		[TestMethod]
		public void Test_01_Connect()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.client.OnStateChanged += (sender, State) =>
			{
				switch (State)
				{
					case MqttState.Connected:
						Done.Set();
						break;

					case MqttState.Error:
					case MqttState.Offline:
						Error.Set();
						break;
				}

				return Task.CompletedTask;
			};

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Unable to connect to MQTT broker.");
		}

		[TestMethod]
		public async Task Test_02_Subscribe()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			int Count = 0;

			this.Test_01_Connect();

			this.client.OnContentReceived += (sender, e) =>
			{
				Console.Out.WriteLine(e.Topic + ": " + System.Text.Encoding.UTF8.GetString(e.Data));

				if (++Count == 10)
					Done.Set();

				return Task.CompletedTask;
			};

			await this.client.SUBSCRIBE("#");
			try
			{
				Assert.IsTrue(Done.WaitOne(60000), "Content not received correctly.");
			}
			finally
			{
				await this.client.UNSUBSCRIBE("#");
			}
		}

		[TestMethod]
		public async Task Test_03_Publish_AtMostOne()
		{
			await this.Publish(MqttQualityOfService.AtMostOnce);
		}

		[TestMethod]
		public async void Test_04_Publish_AtLeastOnce()
		{
			await this.Publish(MqttQualityOfService.AtLeastOnce);
		}

		[TestMethod]
		public async void Test_05_Publish_ExactlyOne()
		{
			await this.Publish(MqttQualityOfService.ExactlyOnce);
		}

		private async Task Publish(MqttQualityOfService QoS)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.Test_01_Connect();

			this.client.OnContentReceived += (sender, e) =>
			{
				string s = System.Text.Encoding.UTF8.GetString(e.Data);

				if (s == "Hello world.")
					Done.Set();
				else
					Error.Set();
			
				return Task.CompletedTask;
			};

			await this.client.SUBSCRIBE("/Waher/IoT/Test", QoS);
			try
			{
				await this.client.PUBLISH("/Waher/IoT/Test", QoS, false,
					System.Text.Encoding.UTF8.GetBytes("Hello world."));

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Content not published correctly.");
			}
			finally
			{
				await this.client.UNSUBSCRIBE("/Waher/IoT/Test");
			}
		}

		[TestMethod]
		public void Test_06_HeartBeats()
		{
			ManualResetEvent Ping = new ManualResetEvent(false);
			ManualResetEvent PingResp = new ManualResetEvent(false);

			this.Test_01_Connect();

			this.client.OnPing += (sender, e) => { Ping.Set(); return Task.CompletedTask; };
			this.client.OnPingResponse += (sender, e) => { PingResp.Set(); return Task.CompletedTask; };

			Assert.IsTrue(Ping.WaitOne(60000), "Ping not sent properly.");
			Assert.IsTrue(PingResp.WaitOne(60000), "Ping response not received properly.");
		}

	}
}
