using System.Linq;
using CommandLine;
using taa.Config;
using taa.Extension;
using taa.Factory;
using taa.Repository;

namespace taa.Verb {
    [Verb("get", HelpText = "数え上げます")]
    public class Get : SubCommand {

        [Option("start", Default = 1, HelpText = "Seedの開始値です")]
        public int SeedStart { get; set; }

        [Option("end", Default = 2000, HelpText = "Seedの終了値です")]
        public int SeedEnd { get; set; }

        [Option('x',"pullParallel", Default = 1, HelpText = "データベースからデータを取り出すパイプラインの並列数です")]
        public int PullParallel { get; set; }

        [Option('y', "buildParallel", Default = 1, HelpText = "取り出したデータをまとめるパイプラインの並列数です")]
        public int BuildParallel { get; set; }

        [Option('z', "aggregateParallel", Default = 1, HelpText="数え上げの並列数です")]
        public int AggregateParallel { get; set; }

        public override bool Run() {
            var repo = new PgSqlRepository();
            var param = new Parameter.Parameter(
                VtnVoltage, VtnSigma, VtnDeviation,
                VtpVoltage, VtpSigma, VtpDeviation
            );
            var records = repo.Get(param, r => SeedStart <= r.Seed && r.Seed <= SeedEnd);
            var doc = new Document.Document(param, records, records.Max(x => x.Sweep));

            doc.WL();

            var filter = new Filter(Config.Config.GetInstance().Conditions, Config.Config.GetInstance().Expressions);
            filter.Aggregate(doc).WL();
            return true;
        }

    }
}