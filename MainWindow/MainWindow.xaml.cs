using CNCTool.GCode;
using CNCTool.Util;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CNCTool.MainWindow
{
	public partial class MainWindow : Window
	{
		#region UpdateUi
		private event Action UpdateUiEvent;

		private void UpdateUi()
		{
			if (System.Threading.Thread.CurrentThread != Dispatcher.Thread)
			{
				Dispatcher.Invoke(UpdateUi);
				return;
			}

			if (UpdateUiEvent != null)
				UpdateUiEvent();
		}
		#endregion

		public MainWindow()
		{
			InitializeComponent();

			InitEditor();
			InitMachine();
			InitRun();

			UpdateUi();
		}

		#region Closing
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (MachineInterface != null)
			{
				MessageBox.Show("Can't exit while connected");
				e.Cancel = true;
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Properties.Settings.Default.Save();
		}
		#endregion
	}
}
