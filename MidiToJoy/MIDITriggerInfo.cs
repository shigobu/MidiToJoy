using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiToJoy
{
	public class MIDITriggerInfo
	{
		public MIDITriggerInfo()
		{
			Channel = 1;
			Type = MIDITriggerType.Note;
			DataByte1 = 0;
		}

		public MIDITriggerInfo(int channel, MIDITriggerType type, int dataByte1)
		{
			Channel = channel;
			Type = type;
			DataByte1 = dataByte1;
		}

		/// <summary>
		/// チャンネル(1-16)
		/// </summary>
		public int Channel { get; set; }

		/// <summary>
		/// タイプ
		/// </summary>
		public MIDITriggerType Type { get; set; }

		/// <summary>
		/// データバイト１
		/// </summary>
		public int DataByte1 { get; set; }
	}
}
