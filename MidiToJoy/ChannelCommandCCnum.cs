using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiToJoy
{
	[Serializable]
	public class ChannelCommandCCnum
	{
		public ChannelCommandCCnum()
		{
		}

		public ChannelCommandCCnum(int channel, int commandCode, int cCnum)
		{
			Channel = channel;
			CommandCode = commandCode;
			CCnum = cCnum;
		}

		/// <summary>
		/// チャンネル
		/// </summary>
		public int Channel { get; set; } = 0;

		/// <summary>
		/// コマンドコード名
		/// </summary>
		public int CommandCode { get; set; } = 0;

		/// <summary>
		/// CC番号
		/// </summary>
		public int CCnum { get; set; } = 0;
	}
}
