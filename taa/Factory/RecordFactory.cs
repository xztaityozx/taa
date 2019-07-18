using System;
using System.Collections.Generic;
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
            var rt = new RecordModel[sweeps];

            var param = new Parameter.Parameter(
                vtn: new Transistor(vtnThreshold, vtnSigma, vtnThreshold),
                vtp: new Transistor(vtpThreshold, vtpSigma, vtpThreshold),
                seed: seed, "", 0).ToString();

            string doc;
            using (var sr = new StreamReader(path)) doc = sr.ReadToEnd();

            var container = doc.Split("#", StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();

            // 信号名のリスト
            var signals = container[0].Trim(' ').Split(",", StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();


            return rt;
        }
    }
}
