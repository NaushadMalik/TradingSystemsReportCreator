using System.Collections.Generic;

namespace TradingSystems_FileWriter
{
    public interface IWriter
    {
        bool Write(Dictionary<int, double> tradePositions, string FilePath);
    }
}
