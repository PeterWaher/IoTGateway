using System;
using System.ComponentModel;
using System.Windows;
using Waher.Events;

namespace Waher.IoTGateway.Setup.Windows
{
	/// <summary>
	/// Interaction logic for ProgressWindow.xaml
	/// </summary>
	public partial class ProgressWindow : Window, INotifyPropertyChanged
	{
		private string installationStatus = string.Empty;
		private int nrFilesExpected= 0;
		private int nrFilesCopied= 0;
		private int nrFilesSkipped= 0;
		private int nrFilesDeleted= 0;
		private int progressPercent= 0;
		private bool hasInstallationStatus = false;
		private bool installingFiles = true;

		/// <summary>
		/// Interaction logic for ProgressWindow.xaml
		/// </summary>
		/// <param name="NrFilesExpected">Number of files expected during installation.</param>
		public ProgressWindow(int NrFilesExpected)
		{
			this.nrFilesExpected = NrFilesExpected;
			this.InitializeComponent();
			this.DataContext = this;
		}

		public int NrFilesExpected
		{
			get => this.nrFilesExpected;
			set
			{
				if (this.nrFilesExpected != value)
				{
					this.nrFilesExpected = value;
					this.RaisePropertyChanged(nameof(this.NrFilesExpected));
				}
			}
		}

		public int ProgressPercent
		{
			get => this.progressPercent;
			set
			{
				if (this.progressPercent != value)
				{
					this.progressPercent = value;
					this.RaisePropertyChanged(nameof(this.ProgressPercent));
				}
			}
		}

		public int NrFilesCopied
		{
			get => this.nrFilesCopied;
			set
			{
				if (this.nrFilesCopied != value)
				{
					this.nrFilesCopied = value;
					this.RaisePropertyChanged(nameof(this.NrFilesCopied));

					this.ProgressPercent = Math.Min(100, (int)(100.0 * this.nrFilesCopied / this.NrFilesExpected + 0.5));
				}
			}
		}

		public int NrFilesSkipped
		{
			get => this.nrFilesSkipped;
			set
			{
				if (this.nrFilesSkipped != value)
				{
					this.nrFilesSkipped = value;
					this.RaisePropertyChanged(nameof(this.NrFilesSkipped));
				}
			}
		}

		public int NrFilesDeleted
		{
			get => this.nrFilesDeleted;
			set
			{
				if (this.nrFilesDeleted != value)
				{
					this.nrFilesDeleted = value;
					this.RaisePropertyChanged(nameof(this.NrFilesDeleted));
				}
			}
		}

		public string InstallationStatus
		{
			get => this.installationStatus;
			set
			{
				if (this.installationStatus != value)
				{
					this.installationStatus = value;
					this.RaisePropertyChanged(nameof(this.InstallationStatus));

					this.HasInstallationStatus = !string.IsNullOrEmpty(value);
					this.InstallingFiles = string.IsNullOrEmpty(value);
				}
			}
		}

		public bool HasInstallationStatus
		{
			get => this.hasInstallationStatus;
			set
			{
				if (this.hasInstallationStatus != value)
				{
					this.hasInstallationStatus = value;
					this.RaisePropertyChanged(nameof(this.HasInstallationStatus));
				}
			}
		}

		public bool InstallingFiles
		{
			get => this.installingFiles;
			set
			{
				if (this.installingFiles != value)
				{
					this.installingFiles = value;
					this.RaisePropertyChanged(nameof(this.InstallingFiles));
				}
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		private void RaisePropertyChanged(string PropertyName)
		{
			try
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		public void ReportProgress(int NrFilesCopied, int NrFilesSkipped, int NrFilesDeleted, string InstallationStatus)
		{
			this.NrFilesCopied = NrFilesCopied;
			this.NrFilesSkipped = NrFilesSkipped;
			this.NrFilesDeleted = NrFilesDeleted;
			this.InstallationStatus = InstallationStatus;
		}

	}
}
