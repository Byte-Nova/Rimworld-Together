using Shared;

namespace TCPNetwork.Packets
{
    public class PollutionData 
    {
        public PollutionDetails _pollutionData { get; set; } = new PollutionDetails();

        public override string ToString()
        {
            return $"PollutionData:|{_pollutionData}";
        }
    }
}