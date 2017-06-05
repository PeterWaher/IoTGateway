using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Networking.MQTT.Test
{
	[TestClass]
	public class UnencryptedMqttTests : MqttTests
	{
		public override bool Encypted
		{
			get { return false; }
		}

		public override int Port
		{
			get { return 1883; }
		}
	}
}
