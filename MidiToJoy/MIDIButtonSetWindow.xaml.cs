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
		/// 「CC」を返します。
		/// </summary>
		public static string CCString { get; } = "CC";

		/// <summary>
		/// 「CCOn」を返します。
		/// </summary>
		public static string CCOnString { get; } = "CCOn";

		/// <summary>
		/// 「CCOff」を返します。
		/// </summary>
		public static string CCOffString { get; } = "CCOff";

		/// <summary>
		/// 「ノート」を返します。
		/// </summary>
		public static string NoteString { get; } = "ノート";

		/// <summary>
		/// 「ノートOn」を返します。
		/// </summary>
		public static string NoteOnString { get; } = "ノートOn";

		/// <summary>
		/// 「ノートOff」を返します。
		/// </summary>
		public static string NoteOffString { get; } = "ノートOff";

		/// <summary>
		/// ボタン名を取得、設定します。
		/// </summary>
		public string ButtonName { get; set; } = "";

		/// <summary>
		/// midi入力デバイスを取得、設定します。
		/// </summary>
		private MidiIn MidiIn { get; set; } = null;

		public MIDIButtonSetWindow(string buttonName, MidiIn midiIn)
		{
			InitializeComponent();
			ButtonName = buttonName;
			MidiIn = midiIn;

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
				MidiIn.MessageReceived -= ((MainWindow)Owner).midiIn_MessageReceived;

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
				if (setWindow.CommandCodeName != CCString && setWindow.CommandCodeName != NoteString)
				{
					MessageBox.Show("アナログ軸には、ノートかコントロールチェンジを指定してください。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
					return;
				}

				ChannelCombo.SelectedIndex = setWindow.Channel;
				CommandCodeCombo.SelectedValue = setWindow.CommandCodeName;
				CCNoteNumTextBox.Text = setWindow.DataByte1.ToString();

			}
			finally
			{
				if (setWindow != null)
				{
					MidiIn.MessageReceived -= setWindow.MidiIn_MessageReceived;
				}

				//midiインベント
				MidiIn.MessageReceived += ((MainWindow)Owner).midiIn_MessageReceived;
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

		}
	}
}
