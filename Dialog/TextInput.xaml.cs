using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CNCTool.Dialog
{
	/// <summary>
	/// Interaction logic for TextInput.xaml
	/// </summary>
	public partial class TextInputDialog : Window
	{
		public delegate bool ValidationHandler(string input);
		public event ValidationHandler Validate;
		public string ErrorMessage = null;

		public TextInputDialog(string header)
		{
			InitializeComponent();
			labelDescription.Content = header;
		}

		bool ValidateInput()
		{
			if (Validate.GetInvocationList().Length == 0)
				return true;

			if (Validate(textBoxInput.Text))
				return true;

			if(ErrorMessage != null)
				MessageBox.Show(ErrorMessage);

			return false;
		}

		private void Cancel(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void Ok(object sender, RoutedEventArgs e)
		{
			if(ValidateInput())
			{
				DialogResult = true;
				Close();
			}
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}
	}
}
