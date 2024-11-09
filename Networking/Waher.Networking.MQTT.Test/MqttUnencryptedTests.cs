using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Networking.MQTT.Test
{
	[TestClass]
	public class MqttUnencryptedTests : MqttTests
	{
		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			SetupSniffer();
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			CloseSniffer();
		}

		public override bool Encypted => false;
		public override int Port => 1883;
	}
}
