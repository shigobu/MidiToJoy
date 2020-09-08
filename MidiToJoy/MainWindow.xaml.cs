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
		/// アナログ軸の最大値
		/// </summary>
		private long MaxAxisValue = 0;

		const int CCMax = 127;
		const int pitchBendMax = 16383;

		static public vJoy joystick;
		static public uint vjoyId = 1;
		const string appName = "MidiToJoy";
		MidiIn MidiIn = null;
		int buttonDelay = 100;
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

		/// <summary>
		/// ボタン設定の内容
		/// </summary>
		Dictionary<int, MIDITriggerInfo> ButtonTriggerInfos { get; set; }

		/// <summary>
		/// 種類コンボボックスに割り当てるもの。
		/// </summary>
		public List<MIDITriggerTypeAndName> TriggerTypeAndNames { get; } = new List<MIDITriggerTypeAndName>()
		{
			new MIDITriggerTypeAndName(MIDITriggerType.ControlChange),
			new MIDITriggerTypeAndName(MIDITriggerType.PitchWheelChange)
		};

		public MainWindow()
		{
			InitializeComponent();
			joystick = new vJoy();
			DataContext = this;

			ButtonTriggerInfos = new Dictionary<int, MIDITriggerInfo>()
			{
				{1, new MIDITriggerInfo() },
				{2, new MIDITriggerInfo() },
				{3, new MIDITriggerInfo() },
				{4, new MIDITriggerInfo() },
				{5, new MIDITriggerInfo() },
				{6, new MIDITriggerInfo() },
				{7, new MIDITriggerInfo() },
				{8, new MIDITriggerInfo() },
				{9, new MIDITriggerInfo() },
				{10, new MIDITriggerInfo() },
				{11, new MIDITriggerInfo() },
				{12, new MIDITriggerInfo() },
				{13, new MIDITriggerInfo() },
				{14, new MIDITriggerInfo() },
				{15, new MIDITriggerInfo() },
				{16, new MIDITriggerInfo() },
				{17, new MIDITriggerInfo() },
				{18, new MIDITriggerInfo() },
				{19, new MIDITriggerInfo() },
				{20, new MIDITriggerInfo() },
				{21, new MIDITriggerInfo() },
				{22, new MIDITriggerInfo() },
				{23, new MIDITriggerInfo() },
				{24, new MIDITriggerInfo() },
				{25, new MIDITriggerInfo() },
				{26, new MIDITriggerInfo() },
				{27, new MIDITriggerInfo() },
				{28, new MIDITriggerInfo() },
				{29, new MIDITriggerInfo() },
				{30, new MIDITriggerInfo() },
				{31, new MIDITriggerInfo() },
				{32, new MIDITriggerInfo() }
			};
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

		delegate MIDITriggerType GetTypeComboValueDelegate(ComboBox comboBox);
		/// <summary>
		/// 種類コンボボックスの選択中タイプを取得します。
		/// </summary>
		/// <param name="comboBox"></param>
		/// <returns></returns>
		private MIDITriggerType GetTypeComboValue(ComboBox comboBox)
		{
			if (comboBox.Dispatcher.CheckAccess())
			{
				return (MIDITriggerType)comboBox.SelectedValue;
			}
			else
			{
				return (MIDITriggerType)comboBox.Dispatcher.Invoke(new GetTypeComboValueDelegate(GetTypeComboValue), comboBox);
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
			MidiIn.MessageReceived += midiIn_MessageReceivedAsync;
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
		public async void midiIn_MessageReceivedAsync(object sender, MidiInMessageEventArgs e)
		{
			//アナログ軸
			foreach (Axis item in Enum.GetValues(typeof(Axis)))
			{
				if (IsValidAxisInput(item, e))
				{
					SetVjoyAxis(item, e);
				}
			}

			//ボタン
			foreach (var info in ButtonTriggerInfos)
			{
				if (IsValidButtonInput(info.Key, e))
				{
					await SetVjoyButtonAsync(info.Key, e);
				}
			}
		}

		/// <summary>
		/// 指定のmidi入力であるかどうかをかえします。
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool IsValidAxisInput(Axis axis, MidiInMessageEventArgs e)
		{
			ComboBox ChannelCombo = AxisChannelCombos[axis];
			ComboBox CommandCodeCombo = AxisCommandCodeCombos[axis];
			TextBox CCNumText = AxisCCNumTextBoxs[axis];
			int channel = GetComboIndex(ChannelCombo) + 1;
			if (e.MidiEvent.Channel != channel)
			{
				return false;
			}
			MIDITriggerType type = GetTypeComboValue(CommandCodeCombo);
			if (type == MIDITriggerType.ControlChange)
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
			else if (type == MIDITriggerType.PitchWheelChange)
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
		/// 指定のmidiボタン入力であるかどうかを返します。
		/// </summary>
		/// <param name="buttonNum"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		bool IsValidButtonInput(int buttonNum, MidiInMessageEventArgs e)
		{
			MIDITriggerInfo triggerInfo = ButtonTriggerInfos[buttonNum];

			int channel = triggerInfo.Channel;
			if (e.MidiEvent.Channel != channel)
			{
				return false;
			}
			switch (triggerInfo.Type)
			{
				case MIDITriggerType.Note:
				case MIDITriggerType.NoteOn:
				case MIDITriggerType.NoteOff:
					if (e.MidiEvent.CommandCode != MidiCommandCode.NoteOff || e.MidiEvent.CommandCode != MidiCommandCode.NoteOn)
					{
						return false;
					}
					byte midiNoteNum = (byte)((e.RawMessage >> 8) & 0b11111111);
					if (midiNoteNum != triggerInfo.DataByte1)
					{
						return false;
					}
					break;
				case MIDITriggerType.ControlChange:
				case MIDITriggerType.ControlChangeOn:
				case MIDITriggerType.ControlChangeOff:
					if (e.MidiEvent.CommandCode != MidiCommandCode.ControlChange)
					{
						return false;
					}
					byte midiCCNum = (byte)((e.RawMessage >> 8) & 0b11111111);
					if (midiCCNum != triggerInfo.DataByte1)
					{
						return false;
					}
					break;
				default:
					return false;
			}
			return true;
		}

		/// <summary>
		/// vjoyデバイスにボタン入力を指定します。
		/// </summary>
		/// <param name="buttonNum"></param>
		/// <param name="e"></param>
		async Task SetVjoyButtonAsync(int buttonNum, MidiInMessageEventArgs e)
		{
			MIDITriggerInfo triggerInfo = ButtonTriggerInfos[buttonNum];
			switch (e.MidiEvent.CommandCode)
			{
				case MidiCommandCode.NoteOff:
					if (triggerInfo.Type == MIDITriggerType.Note)
					{
						joystick.SetBtn(false, vjoyId, (uint)buttonNum);
					}
					else if(triggerInfo.Type == MIDITriggerType.NoteOff)
					{
						joystick.SetBtn(true, vjoyId, (uint)buttonNum);
						await Task.Delay(buttonDelay);
						joystick.SetBtn(false, vjoyId, (uint)buttonNum);
					}
					break;
				case MidiCommandCode.NoteOn:
					if (triggerInfo.Type == MIDITriggerType.Note)
					{
						joystick.SetBtn(true, vjoyId, (uint)buttonNum);
					}
					else if (triggerInfo.Type == MIDITriggerType.NoteOn)
					{
						joystick.SetBtn(true, vjoyId, (uint)buttonNum);
						await Task.Delay(buttonDelay);
						joystick.SetBtn(false, vjoyId, (uint)buttonNum);
					}
					else { /*何もしない*/}
					break;
				case MidiCommandCode.ControlChange:
					byte midiCCVal = (byte)((e.RawMessage >> 16) & 0b11111111);

					if (triggerInfo.Type == MIDITriggerType.ControlChange)
					{
						if (midiCCVal >= 64)
						{
							joystick.SetBtn(true, vjoyId, (uint)buttonNum);
						}
						else
						{
							joystick.SetBtn(false, vjoyId, (uint)buttonNum);
						}
					}
					else if(triggerInfo.Type == MIDITriggerType.ControlChangeOn)
					{
						if (midiCCVal >= 64)
						{
							joystick.SetBtn(true, vjoyId, (uint)buttonNum);
							await Task.Delay(buttonDelay);
							joystick.SetBtn(false, vjoyId, (uint)buttonNum);
						}
					}
					else if (triggerInfo.Type == MIDITriggerType.ControlChangeOff)
					{
						if (midiCCVal < 64)
						{
							joystick.SetBtn(true, vjoyId, (uint)buttonNum);
							await Task.Delay(buttonDelay);
							joystick.SetBtn(false, vjoyId, (uint)buttonNum);
						}
					}
					else { /*何もしない*/}
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
			//ボタン
			ButtonTriggerInfos = dataClass.ButtonData;
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

			dataClass.ButtonData = ButtonTriggerInfos;

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
				MidiIn.MessageReceived -= midiIn_MessageReceivedAsync;

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

				//midi読み込みを一時停止
				MidiIn.Stop();

				//CCかピッチベンドで無い場合設定しない。
				if (setWindow.TriggerType != MIDITriggerType.ControlChange && setWindow.TriggerType != MIDITriggerType.PitchWheelChange)
				{
					MessageBox.Show("アナログ軸には、コントロールチェンジかピッチベンドを指定してください。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
					return;
				}

				AxisChannelCombos[axis].SelectedIndex = setWindow.Channel;
				AxisCommandCodeCombos[axis].SelectedValue = setWindow.TriggerType;
				AxisCCNumTextBoxs[axis].Text = setWindow.DataByte1.ToString();
				
			}
			finally
			{
				if (setWindow != null)
				{
					MidiIn.MessageReceived -= setWindow.MidiIn_MessageReceived;
				}

				//midiインベント
				MidiIn.MessageReceived += midiIn_MessageReceivedAsync;
				MidiIn.Start();
			}
		}

		/// <summary>
		/// ボタンタブのボタン設定ボタンが押されたとき
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonSettingButton_Click(object sender, RoutedEventArgs e)
		{
			string buttonName = ((Button)sender).Content.ToString();
			int buttonNum = 0;
			int.TryParse(buttonName, out buttonNum);
			MIDIButtonSetWindow buttonSetWindow = new MIDIButtonSetWindow(buttonName, MidiIn, ButtonTriggerInfos[buttonNum]);
			buttonSetWindow.Owner = this;

			bool result = buttonSetWindow.ShowDialog() ?? false;
			if (!result)
			{
				return;
			}

			ButtonTriggerInfos[buttonNum] = buttonSetWindow.TriggerInfo;
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
