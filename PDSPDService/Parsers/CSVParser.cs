using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CsvHelper;
using System.IO;
using System.Globalization;

namespace ProductionWebService.Parsers
{
    public class CSVFileParser
    {
        public List<string[]> ReadCSV(string FileName)
        {
            var Records = new List<string[]>();

            using (StreamReader reader = new StreamReader(FileName))
            {
                
                var csv = new CsvParser(reader);
                
                //csv.Configuration.Delimiter = Delimiter;

                while (true)
                {
                    var record = csv.Read();
                    

                    // Need to put these checks before
                    if (record == null)// reached end of file
                        break;

                    Records.Add(record);
                }
            }

            return Records;
        }

        public List<string[]> ReadCSV(byte[] Contents)
        {
            var Records = new List<string[]>();
            Stream stream = new MemoryStream(Contents);

            using (StreamReader reader = new StreamReader(stream))
            {

                var csv = new CsvParser(reader);

                //csv.Configuration.Delimiter = Delimiter;

                while (true)
                {
                    var record = csv.Read();


                    // Need to put these checks before
                    if (record == null)// reached end of file
                        break;

                    Records.Add(record);
                }
            }

            return Records;
        }
    }
}