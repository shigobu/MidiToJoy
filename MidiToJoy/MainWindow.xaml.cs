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
using System.Windows.Navigation;
using System.Windows.Shapes;
using vJoyInterfaceWrap;
using NAudio.Midi;

namespace MidiToJoy
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		static public vJoy joystick;
		static public uint vjoyId = 1;
		const string appName = "MidiToJoy";
		MidiIn MidiIn = null;

		public MainWindow()
		{
			InitializeComponent();
			joystick = new vJoy();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (!joystick.vJoyEnabled())
			{
				MessageBox.Show("vJoy driver not enabled: Failed Getting vJoy attributes.");
				Close();
			}

			VjdStat status = joystick.GetVJDStatus(vjoyId);

			if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(vjoyId))))
			{
				MessageBox.Show(string.Format("Failed to acquire vJoy device number {0}.\n", vjoyId));
				Close();
			}

			joystick.RegisterRemovalCB(ChangedCB, this);

			// Reset this device to default values
			bool ret = joystick.ResetVJD(vjoyId);

			Title = Title + " Device[1]:OK";

			for (int device = 0; device < MidiIn.NumberOfDevices; device++)
			{
				comboBoxMidiInDevices.Items.Add(MidiIn.DeviceInfo(device).ProductName);
			}
			if (comboBoxMidiInDevices.Items.Count > 0)
			{
				comboBoxMidiInDevices.SelectedIndex = 0;
			}
		}

		private void ChangedCB(bool Removed, bool First, object userData)
		{
			MainWindow f = (MainWindow)userData;
			string title = appName;

			if (!Removed && !First)
			{
				title += " Device[1]:OK";
				bool ret = joystick.AcquireVJD(vjoyId);
			}
			else
			{
				title += " Device[1]:Wait...";
			}
			f.setTitle(title);
		}

		/// <summary>
		/// タイトルをスレッドセーフな状態で設定します。
		/// </summary>
		/// <param name="title">タイトル</param>
		private void setTitle(string title)
		{
			if (Dispatcher.CheckAccess())
			{
				Title = title;
			}
			else
			{
				Dispatcher.Invoke(() =>
				{
					Title = title;
				});
			}
		}

		private void ComboBoxMidiInDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (MidiIn != null)
			{
				MidiIn.Stop();
				MidiIn.Dispose();
			}

			MidiIn = new MidiIn(comboBoxMidiInDevices.SelectedIndex);
			MidiIn.MessageReceived += midiIn_MessageReceived;
			MidiIn.ErrorReceived += midiIn_ErrorReceived;
			MidiIn.Start();
		}

		private void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
		{
		}

		private void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
		{
			if (e.MidiEvent.CommandCode == MidiCommandCode.PitchWheelChange && e.MidiEvent.Channel == 1)
			{
				int valu = 0;
				byte LSB = (byte)(e.RawMessage >> 8);
				byte MSB = (byte)(e.RawMessage >> 16);
				valu = (MSB << 7) + LSB;

				long Xmax = 0;
				joystick.GetVJDAxisMax(vjoyId, HID_USAGES.HID_USAGE_X, ref Xmax);
				long Xmin = 0;
				joystick.GetVJDAxisMin(vjoyId, HID_USAGES.HID_USAGE_X, ref Xmin);

				valu = (int)Map(valu, 0, 16129, (int)Xmin, (int)Xmax);
				joystick.SetAxis(valu, vjoyId, HID_USAGES.HID_USAGE_X);

			}
		}

		/// <summary>
		/// 渡された数値をある範囲から別の範囲に変換
		/// </summary>
		/// <param name="value">変換する入力値</param>
		/// <param name="start1">現在の範囲の下限</param>
		/// <param name="stop1">現在の範囲の上限</param>
		/// <param name="start2">変換する範囲の下限</param>
		/// <param name="stop2">変換する範囲の上限</param>
		/// <returns>変換後の値</returns>
		double Map(double value, double start1, double stop1, double start2, double stop2)
		{
			return start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			if (MidiIn != null)
			{
				MidiIn.Stop();
				MidiIn.Dispose();
			}
		}
	}
}
