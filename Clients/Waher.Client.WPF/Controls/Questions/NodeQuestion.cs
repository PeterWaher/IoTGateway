using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Controls.Questions
{
	public abstract class NodeQuestion : Question
	{
		private string[] serviceTokens = null;
		private string[] deviceTokens = null;
		private string[] userTokens = null;
		private string nodeId = string.Empty;
		private string sourceId = string.Empty;
		private string partition = string.Empty;

		public NodeQuestion()
			: base()
		{
		}

		[DefaultValueNull]
		public string[] ServiceTokens
		{
			get { return this.serviceTokens; }
			set { this.serviceTokens = value; }
		}

		[DefaultValueNull]
		public string[] DeviceTokens
		{
			get { return this.deviceTokens; }
			set { this.deviceTokens = value; }
		}

		[DefaultValueNull]
		public string[] UserTokens
		{
			get { return this.userTokens; }
			set { this.userTokens = value; }
		}

		[DefaultValueStringEmpty]
		public string NodeId
		{
			get { return this.nodeId; }
			set { this.nodeId = value; }
		}

		[DefaultValueStringEmpty]
		public string SourceId
		{
			get { return this.sourceId; }
			set { this.sourceId = value; }
		}

		[DefaultValueStringEmpty]
		public string Partition
		{
			get { return this.partition; }
			set { this.partition = value; }
		}
	}
}
