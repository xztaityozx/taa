using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace taa {
    public class Record :IEnumerable<decimal[]> {

        // signal-time => index
        private readonly Map<string, int> signalIndexMap = new Map<string, int>();
        private readonly List<decimal[]> dataList = new List<decimal[]>();

        public Record(int seeds, int times, IEnumerable<Tuple<string,IEnumerable<decimal>>> keyList) {
            var index = 0;
            foreach (var (signal, timeSet) in keyList) {
                    foreach (var time in timeSet) {
                        var k = GenKeyString(signal, time);
                        signalIndexMap[k] = index;
                        index++;
                }
            }

            for (var i = 0; i < seeds * times; i++) {
                dataList.Add(new decimal[index]);
            }
        }

        public static string GenKeyString(string signal, decimal time) => $"{signal}-{time:E10}";

        public decimal this[int index, string signal, decimal time] {
            get {
                var k = signalIndexMap[GenKeyString(signal, time)];
                return dataList[index][k];
            }
            set {
                var k = signalIndexMap[GenKeyString(signal, time)];
                dataList[index][k] = value;
            }
        }

        public IEnumerator<decimal[]> GetEnumerator() {
            return dataList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendLine($"Index, {string.Join(", ", signalIndexMap.Keys)}");

            return sb.ToString();
        }
    }
}