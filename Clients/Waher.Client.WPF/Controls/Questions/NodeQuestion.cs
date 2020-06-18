using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence.Attributes;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Controls.Questions
{
	public abstract class NodeQuestion : Question
	{
		private readonly Dictionary<string, X509Certificate2> certificates = new Dictionary<string, X509Certificate2>();
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

		[IgnoreMember]
		public bool IsNode
		{
			get
			{
				return !string.IsNullOrEmpty(this.nodeId) || !string.IsNullOrEmpty(this.sourceId) || !string.IsNullOrEmpty(this.partition);
			}
		}

		public ThingReference GetNodeReference()
		{
			return new ThingReference(this.nodeId, this.sourceId, this.partition);
		}

		protected void AddNodeInfo(StackPanel Details)
		{
			if (this.IsNode)
			{
				if (!string.IsNullOrEmpty(this.NodeId))
					this.AddKeyValue(Details, "Node ID", this.NodeId);

				if (!string.IsNullOrEmpty(this.SourceId))
					this.AddKeyValue(Details, "Source ID", this.SourceId);

				if (!string.IsNullOrEmpty(this.Partition))
					this.AddKeyValue(Details, "Partition", this.Partition);
			}
		}

		protected void AddTokens(StackPanel Details, ProvisioningClient Client, RoutedEventHandler OnYes, RoutedEventHandler OnNo)
		{
			this.AddTokens(Details, Client, this.ServiceTokens, OnYes, OnNo, OperationRange.ServiceToken);
			this.AddTokens(Details, Client, this.UserTokens, OnYes, OnNo, OperationRange.UserToken);
			this.AddTokens(Details, Client, this.DeviceTokens, OnYes, OnNo, OperationRange.DeviceToken);
		}

		private void AddTokens(StackPanel Details, ProvisioningClient Client, string[] Tokens, RoutedEventHandler OnYes, RoutedEventHandler OnNo, OperationRange Range)
		{
			if (Tokens != null)
			{
				X509Certificate2 Certificate;

				foreach (string Token in Tokens)
				{
					lock (this.certificates)
					{
						if (!this.certificates.TryGetValue(Token, out Certificate))
							Certificate = null;
					}

					if (Certificate != null)
						this.AddToken(Details, Token, Certificate, OnYes, OnNo, Range);
					else
					{
						Client.GetCertificate(Token, (sender, e) =>
						{
							if (e.Ok)
							{
								string Token2 = (string)e.State;

								lock (this.certificates)
								{
									this.certificates[Token2] = e.Certificate;
								}

								MainWindow.UpdateGui(() =>
								{
									this.AddToken(Details, Token2, e.Certificate, OnYes, OnNo, Range);
								});
							}

							return Task.CompletedTask;

						}, Token);
					}
				}
			}
		}

		private void AddToken(StackPanel Details, string Token, X509Certificate2 Certificate, RoutedEventHandler OnYes, RoutedEventHandler OnNo, OperationRange Range)
		{
			Button Button;

			if (!Certificate.Verify())
				return;

			Details.Children.Add(Button = new Button()
			{
				Margin = new Thickness(0, 6, 0, 6),
				Content = "Yes, for " + Certificate.FriendlyName,
				Tag = new object[] { Token, Range }
			});

			Button.Click += OnYes;

			Details.Children.Add(Button = new Button()
			{
				Margin = new Thickness(0, 6, 0, 6),
				Content = "No, for " + Certificate.FriendlyName,
				Tag = new object[] { Token, Range }
			});

			Button.Click += OnNo;
		}
	}
}
