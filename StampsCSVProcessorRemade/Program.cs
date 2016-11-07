using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;

namespace StampsCSVProcessorRemade
{
    class StampsProcessor
    {
        
        public StampsProcessor(string filepath)
        {
            DateList = new List<DateTime>();
            SumsList = new Dictionary<DateTime, decimal>();
            using (StreamReader file = new StreamReader(filepath))
            {
                createCSVRecordsList(file);
            }
            createDateListandSumsList(CSVRecordsList);

        }

        public List<DateTime> DateList
        {
            get; set;
        }
        public Dictionary<DateTime, decimal> SumsList { get; internal set; }
        protected void createDateListandSumsList(List<StampsRecord> inList)
        {
            for (int i = 0; i < inList.Count; i++)
            {
                StampsRecord workingRecord = inList.ElementAt(i);
                DateTime date = workingRecord.Date;
                if (workingRecord.User != "eBay Amazon"
                    && workingRecord.User != "JBMorris master account"
                    && workingRecord.User != "Kickstarter 2")
                {
                    if (SumsList.ContainsKey(date))
                    {
                        SumsList[date] += workingRecord.Cost;
                    }
                    else
                    {
                        SumsList.Add(date, workingRecord.Cost);
                        DateList.Add(date);
                    }
                }
            }
        }

        private List<StampsRecord> CSVRecordsList;
        private void createCSVRecordsList(StreamReader file)
        {
            CsvReader csvreader = new CsvReader(file);
            csvreader.Configuration.RegisterClassMap<StampsRecordMap>();
            CSVRecordsList = csvreader.GetRecords<StampsRecord>().ToList();
        }

    }

    class StampsRecord
    {
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
        public string Class { get; set; }
        public string CostCode { get; set; }
        public string User { get; set; }
    }
    class StampsRecordMap : CsvHelper.Configuration.CsvClassMap<StampsRecord>
    {
        public StampsRecordMap()
        {
            Map(m => m.Date).Name("Date");
            Map(m => m.Cost).Name("Cost").ConvertUsing(row =>
            {
                decimal rowdec = decimal.Parse(row.GetField("Cost"), System.Globalization.NumberStyles.Currency);
                return rowdec;
            });
            Map(m => m.Class).Name("Class/Service");
            Map(m => m.CostCode).Name("Cost Code");
            Map(m => m.User).Name("User");

        }
    }

    class OutPutRecord
    {
        public DateTime Date { get; set; }
        public Decimal Cost { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<OutPutRecord> outputRecords = new List<OutPutRecord>();
            string filepath = @"C:\Users\neeli\OneDrive - Card Kingdom\Shipping Management CSVs 9.16-10.27\Stamps CSV\";
            StampsProcessor stampproc = new StampsProcessor(filepath + @"PrintsTransactions_563173_09012016_to_10312016.csv");
            foreach (DateTime date in stampproc.DateList)
            {
                OutPutRecord record = new OutPutRecord();
                record.Date = date;
                record.Cost = stampproc.SumsList[date];
                Console.WriteLine(date + " " + stampproc.SumsList[date]);
                outputRecords.Add(record);
            }
            using (var csv = new CsvWriter(new StreamWriter(filepath + @"\StampsOutput.csv")))
            {
                csv.WriteRecords(outputRecords);
            }
            Console.ReadLine();
        }
    }
}
