using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Adapter
{
    public static class EventAggregator
    {
        // 定义更新 LoadPort 颜色的事件
        public static event Action<string, string> LoadPortColorUpdateRequested;

        // 触发事件的方法
        public static void RequestLoadPortColorUpdate(string loadPortName, string color)
        {
            LoadPortColorUpdateRequested?.Invoke(loadPortName, color);
        }
    }
}
