using System;
using System.Windows;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Dialogs.Muc
{
	/// <summary>
	/// Interaction logic for InviteOccupantForm.xaml
	/// </summary>
	public partial class RoomInvitationReceivedForm : Window
	{
		public string InviteFrom = string.Empty;
		public string Password = string.Empty;
		public string InvitationReason = string.Empty;
		public string RoomId = string.Empty;
		public string Domain = string.Empty;
		public string RoomName = string.Empty;
		public string RoomJid = string.Empty;
		public bool MembersOnly = false;
		public bool Moderated = false;
		public bool NonAnonymous = false;
		public bool Open = false;
		public bool PasswordProtected = false;
		public bool Persistent = false;
		public bool Public = false;
		public bool SemiAnonymous = false;
		public bool Temporary = false;
		public bool Unmoderated = false;
		public bool Unsecured = false;

		public string MembersOnlyStr => ToStr(MembersOnly);
		public string ModeratedStr => ToStr(Moderated);
		public string NonAnonymousStr => ToStr(NonAnonymous);
		public string OpenStr => ToStr(Open);
		public string PasswordProtectedStr => ToStr(PasswordProtected);
		public string PersistentStr => ToStr(Persistent);
		public string PublicStr => ToStr(Public);
		public string HiddenStr => ToStr(!Public);
		public string SemiAnonymousStr => ToStr(SemiAnonymous);
		public string TemporaryStr => ToStr(Temporary);
		public string UnmoderatedStr => ToStr(Unmoderated);
		public string UnsecuredStr => ToStr(Unsecured);

		public RoomInvitationReceivedForm()
		{
			InitializeComponent();
		}

		private static string ToStr(bool b)
		{
			return b ? "✓" : string.Empty;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void NickName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			this.AcceptButton.IsEnabled = !string.IsNullOrEmpty(this.NickName.Text) && AllLetters(this.NickName.Text);
		}

		private static bool AllLetters(string s)
		{
			foreach (char ch in s)
			{
				if (!char.IsLetter(ch))
					return false;
			}

			return true;
		}

		private void AcceptButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void DeclineButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}
