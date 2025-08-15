using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.IServices
{
    public interface IRobotManager
    {
        bool IsConnected { get; }
        IRobotSocketAPI RobotClient { get; }
        Task ConnectAsync();
        void Disconnect();
    }
}
