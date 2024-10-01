using Shared;
using System.Threading.Tasks;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.SOS2
{
    public interface IisShip 
    {
        Task<bool> ReceiveDataAsync(Map data);
    }
    public interface IStartSOS2 
    {
        void ReceiveData();
    }
}
