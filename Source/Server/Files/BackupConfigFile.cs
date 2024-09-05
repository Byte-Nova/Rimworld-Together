using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class BackupConfigFile 
    {
        public bool AutomaticBackups = true;

        public float IntervalHours = 24f;

        public bool AutomaticDeletion = true;

        public int Amount = 14;
    }
}
