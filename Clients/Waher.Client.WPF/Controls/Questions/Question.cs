using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Client.WPF.Controls.Questions
{
	[CollectionName("ProvisioningQuestions")]
	[TypeName(TypeNameSerialization.LocalName)]
	[Index("Key")]
	[Index("OwnerJID", "ProvisioningJID", "Created")]
	public abstract class Question
	{
		private Guid objectId = Guid.Empty;
		private DateTime created = DateTime.MinValue;
		private string key = string.Empty;
		private string jid = string.Empty;
		private string remoteJid = string.Empty;
		private string ownerJid = string.Empty;
		private string provisioningJid = string.Empty;

		public Question()
		{
		}

		[ObjectId]
		public Guid ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		[DefaultValueDateTimeMinValue]
		public DateTime Created
		{
			get { return this.created; }
			set { this.created = value; }
		}

		[DefaultValueStringEmpty]
		public string Key
		{
			get { return this.key; }
			set { this.key = value; }
		}

		[DefaultValueStringEmpty]
		public string JID
		{
			get { return this.jid; }
			set { this.jid = value; }
		}

		[DefaultValueStringEmpty]
		public string RemoteJID
		{
			get { return this.remoteJid; }
			set { this.remoteJid = value; }
		}

		[DefaultValueStringEmpty]
		public string OwnerJID
		{
			get { return this.ownerJid; }
			set { this.ownerJid = value; }
		}

		[DefaultValueStringEmpty]
		public string ProvisioningJID
		{
			get { return this.provisioningJid; }
			set { this.provisioningJid = value; }
		}

		[IgnoreMember]
		public string Date
		{
			get { return this.created.ToShortDateString(); }
		}

		[IgnoreMember]
		public string Time
		{
			get { return this.created.ToLongTimeString(); }
		}

		[IgnoreMember]
		public abstract string QuestionString
		{
			get;
		}

		public abstract void PopulateDetailsDialog(QuestionView QuestionView, ProvisioningClient ProvisioningClient);
		public abstract bool IsResolvedBy(Question Question);

		protected void AddJidName(string JID, ProvisioningClient ProvisioningClient, TextBlock TextBlock)
		{
			XmppClient Client = ProvisioningClient.Client;
			RosterItem Item = Client[JID];

			if (Item != null && !string.IsNullOrEmpty(Item.Name))
			{
				TextBlock.Inlines.Add(new Run()
				{
					FontWeight = FontWeights.Bold,
					Text = Item.Name
				});
				TextBlock.Inlines.Add(" (");
				TextBlock.Inlines.Add(JID);
				TextBlock.Inlines.Add(")");
			}
			else
			{
				TextBlock.Inlines.Add(new Run()
				{
					FontWeight = FontWeights.Bold,
					Text = JID
				});
			}
		}

		public async Task Processed(QuestionView QuestionView)
		{
			MainWindow.currentInstance.Dispatcher.Invoke(() =>
			{
				QuestionView.Details.Children.Clear();
				QuestionView.QuestionListView.Items.Remove(this);
			});

			await Database.Delete(this);

			LinkedList<Question> ToRemove = null;

			foreach (Question Question in QuestionView.QuestionListView.Items)
			{
				if (Question.IsResolvedBy(this))
				{
					if (ToRemove == null)
						ToRemove = new LinkedList<Question>();

					ToRemove.AddLast(Question);
				}
			}

			if (ToRemove != null)
			{
				MainWindow.currentInstance.Dispatcher.Invoke(() =>
				{
					foreach (Question Question in ToRemove)
						QuestionView.QuestionListView.Items.Remove(Question);
				});

				foreach (Question Question in ToRemove)
					await Database.Delete(Question);
			}
		}

	}
}
