using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace taa {
    public class Record {
        public IEnumerable<string> SignalList { get; }
        // dataMap[index, signalName, time] = value
        private List<Map<string, Map<decimal, decimal>>> dataMap;

        public Record(IEnumerable<string> signals, int cnt) {
            SignalList = new List<string>(signals);
            dataMap = new List<Map<string, Map<decimal, decimal>>>();

            for (var i = 0; i < cnt; i++) {
                dataMap.Add(new Map<string, Map<decimal, decimal>>());

                // 信号のMap追加
                foreach (var signal in signals) {
                    dataMap[i][signal] = new Map<decimal, decimal>();
                }
            }
        }

        public decimal this[int index, string signal, decimal time] {
            get => dataMap[index][signal][time];
            set => dataMap[index][signal][time] = value;
        }

        public override string ToString() {
            var rt = new StringBuilder();

            var timeSet = dataMap[0][SignalList.First()].Keys;

            var col = SignalList.Select(s => $", {s}").Select(e => timeSet.OrderBy(x => x).Aggregate(e, (current, t) => current + $":{t:E5}")).Aggregate("index", (current1, e) => current1 + e);

            rt.AppendLine(col);

            var len = dataMap.Count;
            for (var i = 0; i < len; i++) {
                var line = $"{i+1}";
                foreach (var s in SignalList) {
                    line += $", {s}";
                    line = timeSet.OrderBy(x => x).Aggregate(line, (current, t) => current + $":{this[i, s, t]:E5}");
                }

                rt.AppendLine(line);
            }

            return rt.ToString();

        }
    }
}