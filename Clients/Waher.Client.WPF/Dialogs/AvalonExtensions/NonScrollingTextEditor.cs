using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Waher.Client.WPF.Dialogs.AvalonExtensions
{
	/// <summary>
	/// Avalon editor that does not capture mouse wheel (scrolling) events.
	/// </summary>
	public class NonScrollingTextEditor : ICSharpCode.AvalonEdit.TextEditor
	{
		/// <summary>
		/// Avalon editor that does not capture mouse wheel (scrolling) events.
		/// </summary>
		public NonScrollingTextEditor()
			: base()
		{
		}

		/// <summary>
		/// Avalon editor that does not capture mouse wheel (scrolling) events.
		/// </summary>
		/// <param name="TextArea">Text area.</param>
		public NonScrollingTextEditor(TextArea TextArea)
			: base(TextArea)
		{
		}

		/// <inheritdoc/>
		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			if (!e.Handled)
			{
				e.Handled = true;
				MouseWheelEventArgs e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
				{
					RoutedEvent = UIElement.MouseWheelEvent,
					Source = this
				};
				UIElement Parent = VisualTreeHelper.GetParent(this) as UIElement;
				
				while (!(Parent is null) && !(Parent is ScrollViewer))
					Parent = VisualTreeHelper.GetParent(Parent) as UIElement;

				Parent?.RaiseEvent(e2);
			}
		}
	}
}
