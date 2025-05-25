using System.Collections.Generic;

namespace Shared
{

    public class PlayerRecountData
    {
        public int _currentPlayerCount { get; set; } = -1;

        public List<string> _currentPlayerNames { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"PlayerRecountData:|{_currentPlayerCount}|{_currentPlayerNames?.Count ?? 0}";
        }
    }
}
