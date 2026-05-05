using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyriaNTMP.Options
{
    public class NazeelOptions
    {
        public const string OptionsKey = "Ntmp";
        public const string NTMPAuthHeaderKey = "AuthKey";
        public string NTMPAuthKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; }
    }
}
