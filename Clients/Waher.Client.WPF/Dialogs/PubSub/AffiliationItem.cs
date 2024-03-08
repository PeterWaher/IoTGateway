using System;
using Waher.Networking.XMPP.PubSub;

namespace Waher.Client.WPF.Dialogs.PubSub
{
	public class AffiliationItem
	{
		private Affiliation affiliation;

		public AffiliationItem(Affiliation Affiliation)
		{
			this.affiliation = Affiliation;
		}

		public Affiliation Affiliation => this.affiliation;
		public string Jid => this.affiliation.Jid;

		public static int ToIndex(AffiliationStatus Status)
		{
			switch (Status)
			{
				case AffiliationStatus.owner: return 0;
				case AffiliationStatus.publisher: return 1;
				case AffiliationStatus.publishOnly: return 2;
				case AffiliationStatus.member: return 3;
				case AffiliationStatus.none: return 4;
				case AffiliationStatus.outcast: return 5;
				default: return -1;
			}
		}

		public static AffiliationStatus FromIndex(int Index)
		{
			switch (Index)
			{
				case 0: return AffiliationStatus.owner;
				case 1: return AffiliationStatus.publisher;
				case 2: return AffiliationStatus.publishOnly;
				case 3: return AffiliationStatus.member;
				case 4: return AffiliationStatus.none;
				case 5: return AffiliationStatus.outcast;
				default: throw new ArgumentException("Invalid affiliation.", nameof(Index));
			}
		}

		public int AffiliationIndex
		{
			get => ToIndex(this.affiliation.Status);
			set
			{
				AffiliationStatus Status = FromIndex(value);
				this.affiliation = new Affiliation(this.affiliation.Node, this.affiliation.Jid, Status);
			}
		}
	}
}
