using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeadTracker
{
    public class DesktopChanger
    {
        public DesktopChanger(UdpListener listener)
        {
            listener.HeadPositionChanged += Change;
        }

        public void Change(object sender, HeadPosition pos)
        {

        }
    }
}
