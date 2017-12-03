using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
	}
}
