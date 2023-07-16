using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class FactionManifestJSON
    {
        public string manifestMode;

        public string manifestDetails;

        public List<string> manifestComplexDetails = new List<string>();

        public List<string> manifestSecondaryComplexDetails = new List<string>();
    }
}
