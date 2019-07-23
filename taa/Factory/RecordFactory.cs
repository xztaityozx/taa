using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using taa.Model;

namespace taa.Factory {
    public static class RecordFactory {
        //TODO: ここ実装しろ
        public static RecordModel[] BuildFromCSV(string path,
            double vtnThreshold, double vtnSigma, double vtnDeviation,
            double vtpThreshold, double vtpSigma, double vtpDeviation,
            int seed, int sweeps
        ) {
            var rt = new List<RecordModel>();

            var param = new Parameter.Parameter(
                vtn: new Transistor(vtnThreshold, vtnSigma, vtnThreshold),
                vtp: new Transistor(vtpThreshold, vtpSigma, vtpThreshold),
                seed: seed, "", 0).ToString();

            string doc;
            using (var sr = new StreamReader(path)) doc = sr.ReadToEnd();
            doc = doc.Trim(' ');

            var container = doc.Split("#", StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();

            // 信号名のリスト
            var signals = container[0].Split(",", StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();

            container.Skip(1).AsParallel()
                .WithDegreeOfParallelism(10)
                .ForAll(block => {
                    // ブロックを改行で分割
                    var box = block.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                    // 1行目は#sweep[num] [num]を取り出してsweep値とする
                    var sweep = int.Parse(box[0].Replace("#sweep", ""));
                    for (var i = 1; i < box.Length; i++) {
                        var split = box[i].Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => decimal.Parse(s, NumberStyles.Float)).ToArray();
                        var time = split.First();
                        for (var j = 1; j < split.Length; j++) {
                            rt.Add(new RecordModel {
                                Key = Parameter.Parameter.EncodeKey(signals[j - 1], time),
                                Sweep = sweep,
                                Value = split[j]
                            });
                        }
                    }
                });

            return rt.ToArray();
        }
    }
}
