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
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace MidiToJoy
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// 「CC」を返します。
		/// </summary>
		public static string CCstring { get; } = "CC";
		/// <summary>
		/// 「ピッチベンド」を返します。
		/// </summary>
		public static string PitchBendString { get; } = "ピッチベンド";

		/// <summary>
		/// アナログ軸の最大値
		/// </summary>
		private long MaxAxisValue = 0;

		const int CCMax = 127;
		const int pitchBendMax = 16383;

		static public vJoy joystick;
		static public uint vjoyId = 1;
		const string appName = "MidiToJoy";
		MidiIn MidiIn = null;
		/// <summary>
		/// チェンネルコンボの一覧アナログ軸
		/// </summary>
		Dictionary<Axis, ComboBox> AxisChannelCombos { get; set; }
		/// <summary>
		/// 種類コンボの一覧アナログ軸
		/// </summary>
		Dictionary<Axis, ComboBox> AxisCommandCodeCombos { get; set; }
		/// <summary>
		/// CCナンバーテキストボックス
		/// </summary>
		Dictionary<Axis, TextBox> AxisCCNumTextBoxs { get; set; }
		/// <summary>
		/// 設定ボタン
		/// </summary>
		Dictionary<Axis, Button> AxisSettingButtons { get; set; }

		public MainWindow()
		{
			InitializeComponent();
			joystick = new vJoy();
			DataContext = this;
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

			//アナログ軸の値の最大値取得
			joystick.GetVJDAxisMax(vjoyId, HID_USAGES.HID_USAGE_X, ref MaxAxisValue);

			for (int device = 0; device < MidiIn.NumberOfDevices; device++)
			{
				comboBoxMidiInDevices.Items.Add(MidiIn.DeviceInfo(device).ProductName);
			}
			if (comboBoxMidiInDevices.Items.Count > 0)
			{
				comboBoxMidiInDevices.SelectedIndex = 0;
			}

			//コントロールをディクショナリに格納
			AxisChannelCombos = new Dictionary<Axis, ComboBox>()
			{
				{Axis.X, XChannelCombo },
				{Axis.Y, YChannelCombo },
				{Axis.Z, ZChannelCombo },
				{Axis.XR, XRChannelCombo },
				{Axis.YR, YRChannelCombo },
				{Axis.ZR, ZRChannelCombo },
				{Axis.Slider, SliderChannelCombo },
				{Axis.Dial, DialChannelCombo },
			};
			AxisCommandCodeCombos = new Dictionary<Axis, ComboBox>()
			{
				{Axis.X, XCommandCodeCombo },
				{Axis.Y, YCommandCodeCombo },
				{Axis.Z, ZCommandCodeCombo },
				{Axis.XR, XRCommandCodeCombo },
				{Axis.YR, YRCommandCodeCombo },
				{Axis.ZR, ZRCommandCodeCombo },
				{Axis.Slider, SliderCommandCodeCombo },
				{Axis.Dial, DialCommandCodeCombo },
			};
			AxisCCNumTextBoxs = new Dictionary<Axis, TextBox>()
			{
				{Axis.X, XCCNumTextBox },
				{Axis.Y, YCCNumTextBox },
				{Axis.Z, ZCCNumTextBox },
				{Axis.XR, XRCCNumTextBox },
				{Axis.YR, YRCCNumTextBox },
				{Axis.ZR, ZRCCNumTextBox },
				{Axis.Slider, SliderCCNumTextBox },
				{Axis.Dial, DialCCNumTextBox },
			};
			AxisSettingButtons = new Dictionary<Axis, Button>()
			{
				{Axis.X, XSettingButton },
				{Axis.Y, YSettingButton },
				{Axis.Z, ZSettingButton },
				{Axis.XR, XRSettingButton },
				{Axis.YR, YRSettingButton },
				{Axis.ZR, ZRSettingButton },
				{Axis.Slider, SliderSettingButton },
				{Axis.Dial, DialSettingButton },
			};

			LoadData();
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


		delegate int GetComboIndexDelegate(ComboBox comboBox);
		/// <summary>
		/// コンボボックスの選択中インデックスを取得します。
		/// </summary>
		/// <param name="comboBox"></param>
		/// <returns></returns>
		private int GetComboIndex(ComboBox comboBox)
		{
			if (comboBox.Dispatcher.CheckAccess())
			{
				return comboBox.SelectedIndex;
			}
			else
			{
				 return (int)comboBox.Dispatcher.Invoke(new GetComboIndexDelegate(GetComboIndex), comboBox);
			}
		}

		delegate string GetComboItemDelegate(ComboBox comboBox);
		/// <summary>
		/// コンボボックスの選択中文字列を取得します。
		/// </summary>
		/// <param name="comboBox"></param>
		/// <returns></returns>
		private string GetComboItem(ComboBox comboBox)
		{
			if (comboBox.Dispatcher.CheckAccess())
			{
				return ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
			}
			else
			{
				return (string)comboBox.Dispatcher.Invoke(new GetComboItemDelegate(GetComboItem), comboBox);
			}
		}

		delegate string GetTextBoxTextDelegate(TextBox textBox);
		/// <summary>
		/// テキストボックスのテキストを取得します。
		/// </summary>
		/// <param name="comboBox"></param>
		/// <returns></returns>
		private string GetTextBoxText(TextBox textBox)
		{
			if (textBox.Dispatcher.CheckAccess())
			{
				return textBox.Text;
			}
			else
			{
				return (string)textBox.Dispatcher.Invoke(new GetTextBoxTextDelegate(GetTextBoxText), textBox);
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

		/// <summary>
		/// MIDIメッセージ受信したときのイベント
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
		{
			foreach (Axis item in Enum.GetValues(typeof(Axis)))
			{
				if (IsValidInput(item, e))
				{
					SetVjoyAxis(item, e);
				}
			}
		}

		/// <summary>
		/// 指定のmidi入力であるかどうかをかえします。
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool IsValidInput(Axis axis, MidiInMessageEventArgs e)
		{
			ComboBox ChannelCombo = AxisChannelCombos[axis];
			ComboBox CommandCodeCombo = AxisCommandCodeCombos[axis];
			TextBox CCNumText = AxisCCNumTextBoxs[axis];
			int channel = GetComboIndex(ChannelCombo) + 1;
			if (e.MidiEvent.Channel != channel)
			{
				return false;
			}
			string CommandCodeName = GetComboItem(CommandCodeCombo);
			if (CommandCodeName == CCstring)
			{
				if (e.MidiEvent.CommandCode != MidiCommandCode.ControlChange)
				{
					return false;
				}
				byte midiCCNum = (byte)((e.RawMessage >> 8) & 0b11111111);
				int temp = 0;
				int.TryParse(GetTextBoxText(CCNumText), out temp);
				if (midiCCNum != temp)
				{
					return false;
				}
			}
			else if (CommandCodeName == PitchBendString)
			{
				if (e.MidiEvent.CommandCode != MidiCommandCode.PitchWheelChange)
				{
					return false;
				}
			}
			else
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// 指定のアナログ軸にMIDI情報を設定します。
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="e"></param>
		private void SetVjoyAxis(Axis axis, MidiInMessageEventArgs e)
		{
			int value = 0;
			switch (e.MidiEvent.CommandCode)
			{
				case MidiCommandCode.ControlChange:
					byte ccValue = (byte)(e.RawMessage >> 16);
					value = (int)Map(ccValue, 0, CCMax, 0, MaxAxisValue);
					break;
				case MidiCommandCode.PitchWheelChange:
					byte LSB = (byte)(e.RawMessage >> 8);
					byte MSB = (byte)(e.RawMessage >> 16);
					int pitchVal = (MSB << 7) + LSB;
					value = (int)Map(pitchVal, 0, pitchBendMax, 0, MaxAxisValue);
					break;
				default:
					break;
			}

			switch (axis)
			{
				case Axis.X:
					joystick.SetAxis(value, vjoyId, HID_USAGES.HID_USAGE_X);
					break;
				case Axis.Y:
					joystick.SetAxis(value, vjoyId, HID_USAGES.HID_USAGE_Y);
					break;
				case Axis.Z:
					joystick.SetAxis(value, vjoyId, HID_USAGES.HID_USAGE_Z);
					break;
				case Axis.XR:
					joystick.SetAxis(value, vjoyId, HID_USAGES.HID_USAGE_RX);
					break;
				case Axis.YR:
					joystick.SetAxis(value, vjoyId, HID_USAGES.HID_USAGE_RY);
					break;
				case Axis.ZR:
					joystick.SetAxis(value, vjoyId, HID_USAGES.HID_USAGE_RZ);
					break;
				case Axis.Slider:
					joystick.SetAxis(value, vjoyId, HID_USAGES.HID_USAGE_SL0);
					break;
				case Axis.Dial:
					joystick.SetAxis(value, vjoyId, HID_USAGES.HID_USAGE_SL1);
					break;
				default:
					break;
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

		/// <summary>
		/// データ読み込み
		/// </summary>
		private void LoadData()
		{
			// シリアライズ先のファイル
			Assembly myAssembly = Assembly.GetEntryAssembly();
			string path = myAssembly.Location;
			string xmlFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "setting.xml");

			//ファイル存在確認
			if (!File.Exists(xmlFileName))
			{
				return;
			}

			//DataContractSerializerオブジェクトを作成
			DataContractSerializer serializer =	new DataContractSerializer(typeof(DataClass));
			//読み込むファイルを開く
			DataClass dataClass;
			using (XmlReader xr = XmlReader.Create(xmlFileName))
			{
				//XMLファイルから読み込み、逆シリアル化する
				try
				{
					dataClass = (DataClass)serializer.ReadObject(xr);
				}
				catch (Exception)
				{
					return;
				}
			}

			//MIDIデバイス設定
			comboBoxMidiInDevices.SelectedItem = dataClass.SelectedMIDIDeviceName;

			//アナログ軸
			foreach (Axis item in Enum.GetValues(typeof(Axis)))
			{
				ChannelCommandCCnum channelCommandCCnum = dataClass.AxisData[item];
				AxisChannelCombos[item].SelectedIndex = channelCommandCCnum.Channel;
				AxisCommandCodeCombos[item].SelectedIndex = channelCommandCCnum.CommandCode;
				AxisCCNumTextBoxs[item].Text = channelCommandCCnum.CCnum.ToString();
			}
		}

		/// <summary>
		/// データの保存
		/// </summary>
		private void SaveData()
		{
			DataClass dataClass = new DataClass();

			//midiデバイス名の保存
			dataClass.SelectedMIDIDeviceName = comboBoxMidiInDevices.SelectedItem.ToString();

			//アナログ軸設定の保存
			Dictionary<Axis, ChannelCommandCCnum> axisData = new Dictionary<Axis, ChannelCommandCCnum>();
			foreach (Axis item in Enum.GetValues(typeof(Axis)))
			{
				ComboBox ChannelCombo = AxisChannelCombos[item];
				ComboBox CommandCodeCombo = AxisCommandCodeCombos[item];
				TextBox CCNumText = AxisCCNumTextBoxs[item];

				int channel = ChannelCombo.SelectedIndex;
				int commandCode = CommandCodeCombo.SelectedIndex;
				int CCnum = 0;
				int.TryParse(CCNumText.Text, out CCnum);

				ChannelCommandCCnum channelCommandCCnum = new ChannelCommandCCnum(channel, commandCode, CCnum);

				axisData.Add(item, channelCommandCCnum);
			}
			dataClass.AxisData = axisData;

			// シリアライズ先のファイル
			Assembly myAssembly = Assembly.GetEntryAssembly();
			string path = myAssembly.Location;
			string xmlFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "setting.xml");

			//DataContractSerializerオブジェクトを作成
			//オブジェクトの型を指定する
			DataContractSerializer serializer =	new DataContractSerializer(typeof(DataClass));
			//BOMが付かないUTF-8で、書き込むファイルを開く
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = new UTF8Encoding(false);
			using (XmlWriter xw = XmlWriter.Create(xmlFileName, settings))
			{
				//シリアル化し、XMLファイルに保存する
				serializer.WriteObject(xw, dataClass);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			if (MidiIn != null)
			{
				MidiIn.Stop();
				MidiIn.Dispose();
				MidiIn = null;
			}

			SaveData();
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
		}
	}

	/// <summary>
	/// アナログ軸
	/// </summary>
	[Serializable]
	public enum Axis
	{
		X,
		Y,
		Z,
		XR,
		YR,
		ZR,
		Slider,
		Dial,
	}
}
