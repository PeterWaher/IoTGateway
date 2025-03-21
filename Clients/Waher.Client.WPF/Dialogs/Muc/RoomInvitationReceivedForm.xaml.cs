﻿using System;
using System.Windows;

namespace Waher.Client.WPF.Dialogs.Muc
{
	/// <summary>
	/// Interaction logic for RoomInvitationReceivedForm.xaml
	/// </summary>
	public partial class RoomInvitationReceivedForm : Window
	{
		public string RoomName 
		{
			get => this.lblRoomName.Text;
			set => this.lblRoomName.Text = value; 
		}

		public string InviteTo { set => this.lblInviteTo.Text = value; }
		public string InviteFrom { set => this.lblInviteFrom.Text = value; }
		public string InvitationReason { set => this.lblInvitationReason.Text = value; }
		public string RoomJid { set => this.lblRoomJid.Text = value; }
		public bool MembersOnly { set => this.lblMembersOnly.Text = ToStr(value); }
		public bool Moderated { set => this.lblModerated.Text = ToStr(value); }
		public bool NonAnonymous { set => this.lblNonAnonymous.Text = ToStr(value); }
		public bool Open { set => this.lblOpen.Text = ToStr(value); }
		public bool PasswordProtected { set => this.lblPasswordProtected.Text = ToStr(value); }
		public bool Persistent { set => this.lblPersistent.Text = ToStr(value); }
		public bool Public { set => this.lblPublic.Text = ToStr(value); }
		public bool SemiAnonymous { set => this.lblSemiAnonymous.Text = ToStr(value); }
		public bool Temporary { set => this.lblTemporary.Text = ToStr(value); }
		public bool Unmoderated { set => this.lblUnmoderated.Text = ToStr(value); }
		public bool Unsecured { set => this.lblUnsecured.Text = ToStr(value); }

		public RoomInvitationReceivedForm()
		{
			InitializeComponent();
		}

		private static string ToStr(bool b)
		{
			return b ? "✓" : string.Empty;
		}

		private void CancelButton_Click(object Sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void NickName_TextChanged(object Sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			this.AcceptButton.IsEnabled = !string.IsNullOrEmpty(this.NickName.Text) && AllLetters(this.NickName.Text);
		}

		private static bool AllLetters(string s)
		{
			foreach (char ch in s)
			{
				if (!char.IsLetterOrDigit(ch))
					return false;
			}

			return true;
		}

		private void AcceptButton_Click(object Sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void DeclineButton_Click(object Sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}
