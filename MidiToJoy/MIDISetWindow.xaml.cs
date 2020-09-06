using NAudio.Midi;
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

namespace MidiToJoy
{
	/// <summary>
	/// MIDISetWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MIDISetWindow : Window
	{
		/// <summary>
		/// チャンネル番号（0始まり）
		/// </summary>
		public int Channel { get; set; } = 0;

		/// <summary>
		/// コマンドコード名
		/// </summary>
		public MIDITriggerType TriggerType { get; set; } = MIDITriggerType.Note;

		/// <summary>
		/// CC番号
		/// </summary>
		public int DataByte1 { get; set; } = 0;

		public MIDISetWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// MIDIメッセージ受信したときのイベント
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
		{
			Channel = e.MidiEvent.Channel - 1;
			switch (e.MidiEvent.CommandCode)
			{
				case MidiCommandCode.NoteOff:
				case MidiCommandCode.NoteOn:
					TriggerType = MIDITriggerType.Note;
					break;
				case MidiCommandCode.ControlChange:
					TriggerType = MIDITriggerType.ControlChange;
					break;
				case MidiCommandCode.PitchWheelChange:
					TriggerType = MIDITriggerType.PitchWheelChange;
					break;
				default:
					break;
			}
			switch (e.MidiEvent.CommandCode)
			{
				case MidiCommandCode.NoteOff:
				case MidiCommandCode.NoteOn:
				case MidiCommandCode.ControlChange:
					DataByte1 = (e.RawMessage >> 8) & 0b0000000011111111;
					break;
				default:
					break;
			}

			SetDialogResult(true);
			Close();
		}

		/// <summary>
		/// ダイアログリザルトを設定します。
		/// </summary>
		/// <param name="result"></param>
		private void SetDialogResult(bool? result)
		{
			if (Dispatcher.CheckAccess())
			{
				DialogResult = result;
			}
			else
			{
				Dispatcher.Invoke(() =>
				{
					DialogResult = result;
				});
			}
		}

		/// <summary>
		/// 別スレッドからWindowを閉じます。
		/// </summary>
		new void Close()
		{
			if (Dispatcher.CheckAccess())
			{
				base.Close();
			}
			else
			{
				Dispatcher.Invoke(Close);
			}
		}
	}
}
