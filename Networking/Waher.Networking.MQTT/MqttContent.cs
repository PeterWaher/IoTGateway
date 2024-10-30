using System.Text;
using Waher.Networking.Sniffers;

namespace Waher.Networking.MQTT
{
	/// <summary>
	/// Information about content received from the MQTT server.
	/// </summary>
	public class MqttContent
	{
		private readonly ISniffable sniffable;
		private readonly string topic;
		private readonly byte[] data;
		private string dataString = null;
		private MqttHeader header;
		private BinaryInput dataInput = null;

		/// <summary>
		/// Information about content received from the MQTT server.
		/// </summary>
		/// <param name="Header">MQTT Header</param>
		/// <param name="Topic">Topic</param>
		/// <param name="Data">Binary Data</param>
		/// <param name="Sniffable">Sniffable interface on which the message was received.</param>
		public MqttContent(MqttHeader Header, string Topic, byte[] Data, ISniffable Sniffable)
		{
			this.header = Header;
			this.topic = Topic;
			this.data = Data;
			this.sniffable = Sniffable;
		}

		/// <summary>
		/// MQTT Header
		/// </summary>
		public MqttHeader Header => this.header;

		/// <summary>
		/// Topic
		/// </summary>
		public string Topic => this.topic;

		/// <summary>
		/// Binary Data
		/// </summary>
		public byte[] Data => this.data;

		/// <summary>
		/// Sniffable interface on which the message was received.
		/// </summary>
		public ISniffable Sniffable => this.sniffable;

		/// <summary>
		/// String representation of UTF-8 encoded binary data.
		/// </summary>
		public string DataString
		{
			get
			{
				if (this.dataString is null)
					this.dataString = Encoding.UTF8.GetString(this.data);

				return this.dataString;
			}
		}

		/// <summary>
		/// Data stream that can be used to parse incoming data.
		/// </summary>
		public BinaryInput DataInput
		{
			get
			{
				if (this.dataInput is null)
					this.dataInput = new BinaryInput(this.data);

				return this.dataInput;
			}
		}
	}
}
