using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.IO;
using System.Reflection;

namespace MongoMapReduceExample
{
    class Program
    {
        static void Main(string[] args)
        {
            DoMapReduce();


        }

        static async void DoMapReduce()
        {
            try
            {
                string mapFile = File.ReadAllText(GetPath("MapReducer", "Map.js"));
                string ReduceFile = File.ReadAllText(GetPath("MapReducer", "Reduce.js"));
                string FinalizeFile = File.ReadAllText(GetPath("MapReducer", "Finalize.js"));
                Console.WriteLine("Hello World!");
                MongoDB.Driver.MongoClient client = new MongoDB.Driver.MongoClient("mongodb://192.168.0.40:27088");
                var db = client.GetDatabase("PEIU");
                var collections = db.GetCollection<BsonDocument>("daegun_meter");
                var options = new MapReduceOptions<BsonDocument, BsonDocument>();
                options.JavaScriptMode = true;
                options.OutputOptions.ToJson();
                options.OutputOptions = MapReduceOutputOptions.Merge("statistics");
                var asyncResults = collections.MapReduce<BsonDocument>(mapFile, ReduceFile, options);
                var list = asyncResults.ToList();
                foreach (var list_element in list)
                    Console.WriteLine(list_element.ToString());
                Console.ReadKey();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static string GetPath(params string[] pathes)
        {
            string fullPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            foreach (string path in pathes)
            {
                fullPath = Path.Combine(fullPath, path);
            }
            return fullPath;


        }


    }
}
