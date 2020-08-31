using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiToJoy
{
	[Serializable]
	public class DataClass
	{
		public Dictionary<Axis, ChannelCommandCCnum> AxisData { get; set; }
	}
}
