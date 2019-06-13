using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace PES.Toolkit.Config
{
    public class MongoMapReduceTask
    {
        public string MapFunctionFileName { get; set; }
        public string ReduceFunctionFileName { get; set; }
        public TimeSpan Interval { get; set; }
        public string Database { get; set; }
        public string Collection { get; set; }
        public string ResultDatabase { get; set; }
        public string ResultCollection { get; set; }
        /*
        "ConnectionString": "mongodb://192.168.0.40:27088",
      "MapFunctionFileName": "statBySite_Map.js",
      "ReduceFunctionFileName": "statBySite_Reduce.js",
      "Interval": "00:01:00",
      "Database": "PEIU",
      "Collection": "daegun_meter",
      "ResultDatabase": "PEIU_stat",
      "ResultCollection": "StatisticsBySite"
        */

        public string GetMapFunctionFileName(params string[] folders)
        {
            List<string> folder_list = new List<string>(folders);
            folder_list.Add(MapFunctionFileName);
            return GetPath(folder_list.ToArray());
        }

        public string GetReduceFunctionFileName(params string[] folders)
        {
            List<string> folder_list = new List<string>(folders);
            folder_list.Add(ReduceFunctionFileName);
            return GetPath(folder_list.ToArray());
        }

        string GetPath(params string[] pathes)
        {
            string fullPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            foreach (string path in pathes)
            {
                fullPath = Path.Combine(fullPath, path);
            }
            return fullPath;


        }
    }
    public class MongoMapReduceConfig
    {
        public string ConnectionString { get; set; }

        public MongoMapReduceTask[] MapReduceTasks { get; set; }

    }
}
