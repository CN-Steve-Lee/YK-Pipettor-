using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipettor
{
    public class GlobalVars
    {
        static GlobalVars instance;
        string sComPort = ConfigurationManager.AppSettings["ComPort"];
        public const int ratio = 2;
        public const int xMargin = 100;
        public const int carrierWidth = 25;
        public const int yMargin = 30;
        public const int tubeCntPerCarrier = 12;
        public const int tubeDistance = 18;
        public const int carrierLength = tubeDistance * tubeCntPerCarrier + yMargin;
        public const int jointRadius = 35;
        public const int carrierCnt = 6;
        public const int jointSafeDelta = jointRadius;
        static public double GetXPosition(double carrierIndex)
        {
            return (carrierIndex * GlobalVars.carrierWidth + GlobalVars.xMargin) * GlobalVars.ratio;
        }
        static public GlobalVars Instance
        {
            get
            {
                if (instance == null)
                    instance = new GlobalVars();

                return instance;
            }
        }

        public string ComPort
        {
            get
            {
                return sComPort;
            }
        }
    }
}
