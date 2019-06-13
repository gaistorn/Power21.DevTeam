using System;
using System.Collections.Generic;
using System.Text;

namespace MongoCSVExport
{
    public class ArguementsParser
    {
        private Dictionary<string, string> dictMap = new Dictionary<string, string>();
        public ArguementsParser(string[] argue)
        {
            foreach(string line in argue)
            {
                string[] splits = line.Split('=');
                dictMap.Add(splits[0].TrimStart("--".ToCharArray()), splits[1]);
            }
        }

        public string GetValue(string argument, string defaultValue = null)
        {

            string v = this[argument];
            return v ?? defaultValue;
        }

        public string this[string argument]
        {
            get
            {
                if(dictMap.ContainsKey(argument))
                {
                    return dictMap[argument];
                }
               
                return null;
            }
        }
    }
}
