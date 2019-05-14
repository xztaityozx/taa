using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace taa {
    public class Record : IEnumerable<Map<string, decimal>> {
        private readonly List<Map<string, decimal>> dataMap;

        public Record(int seeds, int times) {
            dataMap=new List<Map<string, decimal>>();
            for (var i = 0; i < seeds*times; i++) {
                dataMap.Add(new Map<string, decimal>());
            }
        }

        public decimal this[int index, string signal, decimal time] {
            get => dataMap[index][GetKey(signal, time)];
            set => dataMap[index][GetKey(signal, time)] = value;
        }

        public static string GetKey(string signal, decimal time) => $"{signal}-{time:E10}";

        public IEnumerator<Map<string, decimal>> GetEnumerator() {
            return dataMap.GetEnumerator();
        }

        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendLine($"Index, {string.Join(", ", dataMap[0].Keys)}");

            //foreach (var r in dataMap.Select((m,idx)=>new{m,idx})) {
            //    sb.AppendLine($"{r.idx}, {string.Join(", ", r.m.Select(item => $"{item.Value:E5}"))}");
            //}
            
            return sb.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}