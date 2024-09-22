using System.Text;

namespace Waher.Things.Mqtt
{
	/// <summary>
	/// Contains information about an MQTT topic
	/// </summary>
	public class MqttTopicRepresentation
	{
		/// <summary>
		/// Contains information about an MQTT topic
		/// </summary>
		/// <param name="TopicString">Full Topic string</param>
		/// <param name="Segments">Segments in topic string.</param>
		/// <param name="SegmentIndex">Current segment index.</param>
		public MqttTopicRepresentation(string TopicString, string[] Segments, int SegmentIndex)
		{
			this.TopicString = TopicString;
			this.Segments = Segments;
			this.SegmentIndex = SegmentIndex;
		}

		/// <summary>
		/// Full Topic string
		/// </summary>
		public string TopicString { get; }

		/// <summary>
		/// Segments in topic string.
		/// </summary>
		public string[] Segments { get; }

		/// <summary>
		/// Current segment index.
		/// </summary>
		public int SegmentIndex { get; set; }

		/// <summary>
		/// Current segment being processed.
		/// </summary>
		public string CurrentSegment => this.Segments[this.SegmentIndex];

		/// <summary>
		/// Processed segments
		/// </summary>
		public string ProcessedSegments
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				int i;

				for (i = 0; i < this.SegmentIndex; i++)
				{
					sb.Append(this.Segments[i]);
					sb.Append('/');
				}

				sb.Append(this.CurrentSegment);

				return sb.ToString();
			}
		}

		/// <summary>
		/// Moves to the next segment.
		/// </summary>
		/// <returns>If a next segment is available or not.</returns>
		public bool MoveNext()
		{
			this.SegmentIndex++;
			return this.SegmentIndex < this.Segments.Length;
		}
	}
}
