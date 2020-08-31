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

		public ChannelCommandCCnum(int channel, string commandCode, int cCnum)
		{
			Channel = channel;
			CommandCode = commandCode ?? throw new ArgumentNullException(nameof(commandCode));
			CCnum = cCnum;
		}

		/// <summary>
		/// チャンネル
		/// </summary>
		public int Channel { get; set; } = 0;

		/// <summary>
		/// コマンドコード名
		/// </summary>
		public string CommandCode { get; set; } = "CC";

		/// <summary>
		/// CC番号
		/// </summary>
		public int CCnum { get; set; } = 0;
	}
}
