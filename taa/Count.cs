using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace taa {
    public class Counter {
        private readonly Filter Filter;
        private readonly Dictionary<string, WvCSV> signalDictionary;

        public Counter(Filter f) {
            Filter = f;
            signalDictionary=new Dictionary<string, WvCSV>();
        }

        public void AddWvCSV(string name, WvCSV csv) {
            if(signalDictionary.ContainsKey(name)) throw new Exception(
                $"Already inserted signal csv: {name}");

            signalDictionary.Add(name, csv);
        }

        public long Aggregate() {
            var rt = 0L;

            Filter.Build();

            var len = signalDictionary.First().Value.Length;
            if(signalDictionary.Any(x => x.Value.Length != len)) throw new Exception("長さの違うCSVがあります");

            for (var i = 0; i < len; i++) {
                
            }

            return rt;
        } 

    }
}
