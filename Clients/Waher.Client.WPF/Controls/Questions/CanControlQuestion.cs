using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence.Attributes;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Controls.Questions
{
	public class CanControlQuestion : NodeQuestion
	{
		private string[] parameterNames = null;
		private string[] availableParameterNames = null;

		public CanControlQuestion()
			: base()
		{
		}

		[DefaultValueNull]
		public string[] ParameterNames
		{
			get { return this.parameterNames; }
			set { this.parameterNames = value; }
		}

		[DefaultValueNull]
		public string[] AvailableParameterNames
		{
			get { return this.availableParameterNames; }
			set { this.availableParameterNames = value; }
		}

		public override string QuestionString => "Allowed to control?";

		public override void PopulateDetailsDialog(QuestionView QuestionView, ProvisioningClient ProvisioningClient)
		{
			// TODO
		}

		public override bool IsResolvedBy(Question Question)
		{
			// TODO
			return false;
		}
	}
}
