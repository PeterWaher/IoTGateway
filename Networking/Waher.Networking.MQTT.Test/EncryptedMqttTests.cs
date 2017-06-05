using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Networking.MQTT.Test
{
	[TestClass]
	public class EncryptedMqttTests : MqttTests
	{
		public override bool Encypted
		{
			get { return true; }
		}

		public override int Port
		{
			get { return 8883; }
		}
	}
}
