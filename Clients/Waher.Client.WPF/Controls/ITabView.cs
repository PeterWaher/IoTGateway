using System;
using System.Windows;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interface for tab view user controls in the client.
	/// </summary>
	public interface ITabView : IDisposable
	{
		void NewButton_Click(object sender, RoutedEventArgs e);
		void SaveButton_Click(object sender, RoutedEventArgs e);
		void SaveAsButton_Click(object sender, RoutedEventArgs e);
		void OpenButton_Click(object sender, RoutedEventArgs e);
	}
}
