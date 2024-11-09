using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
using Waher.Runtime.Console;

namespace Waher.Networking.MQTT.Test
{
	[TestClass]
	public abstract class MqttTests
	{
		private static XmlFileSniffer xmlSniffer = null;
		private MqttClient client;
		private Exception ex;

		public static void SetupSniffer()
		{
			if (xmlSniffer is null)
			{
				File.Delete("MQTT.xml");
				xmlSniffer = xmlSniffer = new XmlFileSniffer("MQTT.xml",
					@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
					int.MaxValue, BinaryPresentationMethod.Hexadecimal);
			}
		}

		public static void CloseSniffer()
		{
			xmlSniffer?.Dispose();
			xmlSniffer = null;
		}

		[TestInitialize]
		public void TestInitialize()
		{
			this.ex = null;
			this.client = new MqttClient("test.mosquitto.org", this.Port, this.Encypted, string.Empty, string.Empty,
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal, LineEnding.NewLine),
				xmlSniffer)
			{
				TrustServer = true
			};

			this.client.OnError += (Sender, e) =>
			{
				this.ex = e;
				return Task.CompletedTask;
			};

			this.client.OnConnectionError += (Sender, e) =>
			{
				this.ex = e;
				return Task.CompletedTask;
			};
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
		public async Task TestCleanup()
		{
			if (this.ex is not null)
				Assert.Fail(this.ex.Message);

			if (this.client is not null)
			{
				await this.client.DisposeAsync();
				this.client = null;
			}
		}

		[TestMethod]
		public void Test_01_Connect()
		{
			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);

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
			ManualResetEvent Done = new(false);
			int Count = 0;

			this.Test_01_Connect();

			this.client.OnContentReceived += (Sender, e) =>
			{
				ConsoleOut.WriteLine(e.Topic + ": " + System.Text.Encoding.UTF8.GetString(e.Data));

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
		public async Task Test_04_Publish_AtLeastOnce()
		{
			await this.Publish(MqttQualityOfService.AtLeastOnce);
		}

		[TestMethod]
		public async Task Test_05_Publish_ExactlyOne()
		{
			await this.Publish(MqttQualityOfService.ExactlyOnce);
		}

		private async Task Publish(MqttQualityOfService QoS)
		{
			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);

			this.Test_01_Connect();

			this.client.OnContentReceived += (Sender, e) =>
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
			ManualResetEvent Ping = new(false);
			ManualResetEvent PingResp = new(false);

			this.Test_01_Connect();

			this.client.OnPing += (Sender, e) => { Ping.Set(); return Task.CompletedTask; };
			this.client.OnPingResponse += (Sender, e) => { PingResp.Set(); return Task.CompletedTask; };

			Assert.IsTrue(Ping.WaitOne(60000), "Ping not sent properly.");
			Assert.IsTrue(PingResp.WaitOne(60000), "Ping response not received properly.");
		}

	}
}
