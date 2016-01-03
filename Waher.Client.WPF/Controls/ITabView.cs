using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
