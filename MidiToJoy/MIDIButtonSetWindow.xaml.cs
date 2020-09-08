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
	/// MIDIButtonSetWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MIDIButtonSetWindow : Window
	{
		/// <summary>
		/// 種類コンボボックスに割り当てるもの。
		/// </summary>
		public List<MIDITriggerTypeAndName> TriggerTypeAndNames { get; } = new List<MIDITriggerTypeAndName>()
		{
			new MIDITriggerTypeAndName(MIDITriggerType.Note),
			new MIDITriggerTypeAndName(MIDITriggerType.NoteOn),
			new MIDITriggerTypeAndName(MIDITriggerType.NoteOff),
			new MIDITriggerTypeAndName(MIDITriggerType.ControlChange),
			new MIDITriggerTypeAndName(MIDITriggerType.ControlChangeOn),
			new MIDITriggerTypeAndName(MIDITriggerType.ControlChangeOff),
			new MIDITriggerTypeAndName(MIDITriggerType.Unallocated)
		};

		/// <summary>
		/// ボタン名を取得、設定します。
		/// </summary>
		public string ButtonName { get; set; } = "";

		/// <summary>
		/// キッカケ情報
		/// </summary>
		public MIDITriggerInfo TriggerInfo
		{
			get
			{
				int channel = ChannelCombo.SelectedIndex + 1;
				MIDITriggerType type = (MIDITriggerType)CommandCodeCombo.SelectedValue;
				int dataByte1 = 0;
				int.TryParse(CCNoteNumTextBox.Text, out dataByte1);
				return new MIDITriggerInfo(channel, type, dataByte1);
			}
			set
			{
				ChannelCombo.SelectedIndex = value.Channel - 1;
				CommandCodeCombo.SelectedValue = value.Type;
				CCNoteNumTextBox.Text = value.DataByte1.ToString();
			}
		}

		/// <summary>
		/// midi入力デバイスを取得、設定します。
		/// </summary>
		private MidiIn MidiIn { get; set; } = null;

		public MIDIButtonSetWindow(string buttonName, MidiIn midiIn, MIDITriggerInfo triggerInfo)
		{
			InitializeComponent();
			ButtonName = buttonName;
			MidiIn = midiIn;
			TriggerInfo = triggerInfo;

			DataContext = this;
		}

		/// <summary>
		/// 設定ボタン押下時
		/// </summary>
		private void SettingButton_Click(object sender, RoutedEventArgs e)
		{
			MIDISetWindow setWindow = null;
			try
			{
				//midiイベントを一旦削除
				MidiIn.MessageReceived -= ((MainWindow)Owner).midiIn_MessageReceivedAsync;

				//設定画面表示
				setWindow = new MIDISetWindow();
				setWindow.Owner = this;
				MidiIn.MessageReceived += setWindow.MidiIn_MessageReceived;

				bool dialogResult = setWindow.ShowDialog() ?? false;
				if (dialogResult == false)
				{
					return;
				}

				//midi読み込みを一時停止
				MidiIn.Stop();

				//CCかピッチベンドで無い場合設定しない。
				if (setWindow.TriggerType != MIDITriggerType.ControlChange && setWindow.TriggerType != MIDITriggerType.Note)
				{
					MessageBox.Show("アナログ軸には、ノートかコントロールチェンジを指定してください。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
					return;
				}

				ChannelCombo.SelectedIndex = setWindow.Channel;
				CommandCodeCombo.SelectedValue = setWindow.TriggerType;
				CCNoteNumTextBox.Text = setWindow.DataByte1.ToString();

			}
			finally
			{
				if (setWindow != null)
				{
					MidiIn.MessageReceived -= setWindow.MidiIn_MessageReceived;
				}

				//midiインベント
				MidiIn.MessageReceived += ((MainWindow)Owner).midiIn_MessageReceivedAsync;
				MidiIn.Start();
			}
		}

		/// <summary>
		/// OKボタン押下時
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}
