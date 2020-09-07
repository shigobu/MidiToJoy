using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiToJoy
{
	class MIDITriggerInfo
	{
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
