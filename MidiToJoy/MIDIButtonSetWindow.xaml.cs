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

		public MIDIButtonSetWindow()
		{
			InitializeComponent();
			DataContext = this;
		}

		/// <summary>
		/// 設定ボタン押下時
		/// </summary>
		private void SettingButton_Click(object sender, RoutedEventArgs e)
		{
#if false
			MIDISetWindow setWindow = null;
			try
			{
				//midiイベントを一旦削除
				MidiIn.MessageReceived -= midiIn_MessageReceived;

				Axis axis = Axis.X;
				//どの軸のボタンが押されたか検索
				foreach (var item in AxisSettingButtons)
				{
					if (ReferenceEquals(item.Value, sender))
					{
						axis = item.Key;
					}
				}

				//設定画面表示
				setWindow = new MIDISetWindow();
				setWindow.Owner = this;
				MidiIn.MessageReceived += setWindow.MidiIn_MessageReceived;

				bool dialogResult = setWindow.ShowDialog() ?? false;
				if (dialogResult == false)
				{
					return;
				}

				AxisChannelCombos[axis].SelectedIndex = setWindow.Channel;
				AxisCommandCodeCombos[axis].SelectedValue = setWindow.CommandCodeName;
				AxisCCNumTextBoxs[axis].Text = setWindow.CCNum.ToString();

			}
			finally
			{
				if (setWindow != null)
				{
					MidiIn.MessageReceived -= setWindow.MidiIn_MessageReceived;
				}

				//midiインベント
				MidiIn.MessageReceived += midiIn_MessageReceived;
			}
#endif
		}
	}
}
