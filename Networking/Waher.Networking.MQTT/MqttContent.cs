using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.MQTT
{
	/// <summary>
	/// Information about content received from the MQTT server.
	/// </summary>
	public class MqttContent
	{
		private MqttHeader header;
		private string topic;
		private byte[] data;
		private BinaryInput dataInput = null;

		internal MqttContent(MqttHeader Header, string Topic, byte[] Data)
		{
			this.header = Header;
			this.topic = Topic;
			this.data = Data;
		}

		/// <summary>
		/// MQTT Header
		/// </summary>
		public MqttHeader Header
		{
			get { return this.header; }
		}

		/// <summary>
		/// Topic
		/// </summary>
		public string Topic
		{
			get { return this.topic; }
		}

		/// <summary>
		/// Binary Data
		/// </summary>
		public byte[] Data
		{
			get { return this.data; }
		}

		/// <summary>
		/// Data stream that can be used to parse incoming data.
		/// </summary>
		public BinaryInput DataInput
		{
			get
			{
				if (this.dataInput == null)
					this.dataInput = new BinaryInput(this.data);

				return this.dataInput;
			}
		}
	}
}
