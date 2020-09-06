using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiToJoy
{
	/// <summary>
	/// midi入力をjoyスティック入力に変換するときのきっかけの種類。
	/// MidiCommandCodeに似ているが、CCオン・CCオフなど、独自なものが存在するため、自作。
	/// </summary>
	public enum MIDITriggerType
	{
		Note,
		NoteOn,
		NoteOff,
		ControlChange,
		ControlChangeOn,
		ControlChangeOff,
		PitchWheelChange,
	}

	public class MIDITriggerTypeAndName
	{
		public MIDITriggerTypeAndName(MIDITriggerType type)
		{
			Type = type;
		}

		/// <summary>
		/// タイプ
		/// </summary>
		public MIDITriggerType Type { get; set; } = MIDITriggerType.Note;

		/// <summary>
		/// 表示名
		/// </summary>
		public string MIDITriggerTypeName
		{
			get
			{
				return GetMIDITriggerTypeName(Type);
			}
		}

		/// <summary>
		/// タイプに対応した文字列を返します。
		/// </summary>
		/// <param name="type">タイプ</param>
		/// <returns>タイプに対応した文字列</returns>
		string GetMIDITriggerTypeName(MIDITriggerType type)
		{
			switch (type)
			{
				case MIDITriggerType.Note:
					return "ノート";
				case MIDITriggerType.NoteOn:
					return "ノートOn";
				case MIDITriggerType.NoteOff:
					return "ノートOff";
				case MIDITriggerType.ControlChange:
					return "CC";
				case MIDITriggerType.ControlChangeOn:
					return "CCOn";
				case MIDITriggerType.ControlChangeOff:
					return "CCOff";
				case MIDITriggerType.PitchWheelChange:
					return "ピッチベンド";
				default:
					return "";
			}
		}
	}
}
