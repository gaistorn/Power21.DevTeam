using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
namespace MongoCSVExport
{
    class Program
    {
        static  void Main(string[] args)
        {
            if(args.Length == 0 || args.Contains("--help"))
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  mongocsvexport <options>");
                Console.WriteLine();
                Console.WriteLine("Export data from MongoDB in CSV format");
                Console.WriteLine("");
                Console.WriteLine("connection options:");
                Console.WriteLine("  --host=<hostname>                      Mongodb에 접속하기 위한 호스트이름 설정 (--host:hostname:port)");
                Console.WriteLine("");
                Console.WriteLine("namespace options:");
                Console.WriteLine("  --database=<databasename>              데이터 베이스 이름");
                Console.WriteLine("  --collection=<collectionname>          컬렉션 이름");
                Console.WriteLine("");
                Console.WriteLine("output options:");
                Console.WriteLine("  --out=<outputfilename>                 저장할 파일명");
                Console.WriteLine("");
                return;
            }
        
            Exporting(args);
            Console.ReadKey();
        }

        static async void Exporting(string[] args)
        {
            ArguementsParser argue = new ArguementsParser(args);

            string host = argue.GetValue("host", "127.0.0.1");
            string db = argue.GetValue("database");
            string cols_name = argue.GetValue("collection");
            string outFile = argue.GetValue("out", "result.csv");
            string filter = argue.GetValue("filter");
            string conn_str = $"mongodb://{host}";
            MongoClient mongoClient = new MongoDB.Driver.MongoClient(conn_str);
            var mapreduce_db = mongoClient.GetDatabase(db);
            var cols = mapreduce_db.GetCollection<BsonDocument>(cols_name);
            Console.WriteLine("Exporting data from mongodb...");
            IAsyncCursor<BsonDocument> cursor = null;
            if (filter != null)
            {
                cursor = await cols.FindAsync(filter);
            }
            else
            {
                cursor = await cols.FindAsync(new BsonDocument());
            }

            while (cursor.MoveNext())
            {
                IEnumerable<BsonDocument> batch = cursor.Current;
                List<string> header = new List<string>();
                bool IsFirstRow = true;
                foreach (var row in batch)
                {
                    foreach(var ele_row in row.Elements)
                    {
                        if(IsFirstRow)
                        {
                            header.Add(ele_row.Name);
                        }
                    }
                    Console.WriteLine(row);
                    Console.WriteLine();
                }
            }
            Console.WriteLine("Completed");
        }

        //private void ReadElement(BsonDocument document, ref List<string> headers, ref List<string> cols)
    }
}
