using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TradingSystems_FileWriter
{
    public class CSVWriter : IWriter
    {
        public bool Write(Dictionary<int, double> tradePositions, string fileFullPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileFullPath));
                using (var writer = new StreamWriter(fileFullPath))
                using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteRecord(new { Time = "Local Time", Volume = "Volume" });
                    csvWriter.NextRecord();
                    csvWriter.WriteRecords(tradePositions);
                }
                return true;
            }
            catch (Exception)
            {
                // Log the exception
                throw;
            }
        }
    }
}
