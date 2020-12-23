using System;
using System.Collections.Generic;
using System.Windows;
using Waher.Networking.XMPP.MUC;

namespace Waher.Client.WPF.Dialogs.Muc
{
	/// <summary>
	/// Interaction logic for OccupantListForm.xaml
	/// </summary>
	public partial class OccupantListForm : Window
	{
		public OccupantListForm(string Title, params MucOccupant[] Occupants)
			: base()
		{
			InitializeComponent();

			this.Title = Title;

			foreach (MucOccupant Occupant in Occupants)
				this.OccupantsView.Items.Add(new Item(Occupant));
		}

		private class Item
		{
			private readonly MucOccupant occupant;
			private Affiliation affiliation;
			private Role role;

			public Item(MucOccupant Occupant)
			{
				this.occupant = Occupant;
				this.affiliation = Occupant.Affiliation;
				this.role = Occupant.Role;
			}

			public MucOccupant Occupant => this.occupant;
			public string NickName => this.occupant.NickName;

			public string Affiliation
			{
				get => this.affiliation.ToString();
				set => this.affiliation = (Affiliation)Enum.Parse(typeof(Affiliation), value);
			}

			public string Role
			{
				get => this.role.ToString();
				set => this.role = (Role)Enum.Parse(typeof(Role), value);
			}

			public bool Changed => this.affiliation != this.occupant.Affiliation || this.role != this.occupant.Role;

			public MucOccupantConfiguration GetChange()
			{
				Affiliation? Affiliation = this.affiliation == this.occupant.Affiliation ? (Affiliation?)null : this.affiliation;
				Role? Role = this.role == this.occupant.Role ? (Role?)null : this.role;

				return new MucOccupantConfiguration(this.occupant.Jid, this.occupant.NickName, Affiliation, Role);
			}
		}

		public MucOccupantConfiguration[] GetChanges()
		{
			List<MucOccupantConfiguration> Result = new List<MucOccupantConfiguration>();

			foreach (Item Item in this.OccupantsView.Items)
			{
				if (!Item.Changed)
					continue;

			}

			return Result.ToArray();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void ApplyButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
	}
}
