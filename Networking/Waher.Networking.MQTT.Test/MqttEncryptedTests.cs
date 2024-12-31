using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Waher.Networking.MQTT.Test
{
	[TestClass]
	public class MqttEncryptedTests : MqttTests
	{
		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			SetupSniffer();
		}

		[ClassCleanup]
		public static async Task ClassCleanup()
		{
			await CloseSniffer();
		}

		public override bool Encypted => true;
		public override int Port => 8883;
	}
}
