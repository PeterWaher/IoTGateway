using System;
using System.Windows;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interface for tab view user controls in the client.
	/// </summary>
	public interface ITabView : IDisposable
	{
		void NewButton_Click(object Sender, RoutedEventArgs e);
		void SaveButton_Click(object Sender, RoutedEventArgs e);
		void SaveAsButton_Click(object Sender, RoutedEventArgs e);
		void OpenButton_Click(object Sender, RoutedEventArgs e);
	}
}
