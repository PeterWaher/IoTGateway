using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Client.WPF.Model;
using Waher.Client.WPF.Controls.Questions;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for QuestionView.xaml
	/// </summary>
	public partial class QuestionView : UserControl, ITabView
	{
		public QuestionView()
		{
			InitializeComponent();
		}

		public void Dispose()
		{
		}

		public void NewQuestion(Question Question)
		{
			this.QuestionListView.Items.Add(Question);
		}

		public void NewButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO
		}

		public void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO
		}

		public void SaveAsButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO
		}

		public void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO
		}
	}
}
