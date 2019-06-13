using Cassandra;
using System;
using System.IO;

namespace ExampleCassandra
{
    class Program
    {
        const string sampleFile = @"D:\Workspace\VESSWebApi\VESSWebApi\sample\sc.csv";
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Cassandra World!");
            
            var cluster = Cluster.Builder()
                .AddContactPoint("192.168.0.40")
                .Build();
            //new TemperatureRepository()
            var session = cluster.Connect("peiuhub");
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            int rowCount = 0;
            using (StreamReader reader = new StreamReader(sampleFile))
            {
                var track = session.Prepare("INSERT INTO peiuhub.pcs_log2(id, timestamp, pcs_meter, pcs_volt, pcs_current, pcs_status, pv_meter, soc, soh, rcc, source, deviceid) VALUES " +
                    "(?,?,?,?,?,?,?,?,?,?,?,?)");
                reader.ReadLine();

                var batch = new BatchStatement();
                ushort batchCount = 0;
                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    string[] columns = line.Split(',');
                    DateTime time = DateTime.Parse(columns[0]);
                    float pcs_meter = float.Parse(columns[1]);
                    float pcs_volt = float.Parse(columns[2]);
                    float pcs_current = float.Parse(columns[3]);
                    int pcs_status = int.Parse(columns[4]);
                    float pv_meter = float.Parse(columns[5]);
                    float soc = float.Parse(columns[6]);
                    float soh = float.Parse(columns[7]);
                    int rcc = int.Parse(columns[8]);
                    int source = 0;
                    string deviceid = columns[10];
                    //Int32 unixTimestamp = (Int32)(time.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    Guid id = Guid.NewGuid();

                    batch.Add(track.Bind(id, time, pcs_meter, pcs_volt, pcs_current, pcs_status, pv_meter, soc, soh, rcc, source, deviceid));
                    batchCount++;
                    rowCount++;

                    if(rowCount % 200 == 0)
                    {
                        session.Execute(batch);
                        batch = new BatchStatement();
                        batchCount = 0;
                    }

                    


                }
                session.Execute(batch);

            }

            Console.WriteLine($"완료. 걸린시간:{watch.Elapsed} row갯수:{rowCount}");
            Console.ReadKey();
        }

    }
}
