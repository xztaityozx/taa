using System.Linq;
using CommandLine;
using taa.Extension;
using taa.Factory;
using taa.Repository;

namespace taa.Verb {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : SubCommand {
        [Value(0, Required = true, HelpText = "入力ファイルです", MetaName = "input")]
        public string InputFile { get; set; }

        [Option('S',"seed",HelpText = "シード値です", Default = 1)]
        public int Seed { get; set; }

        public override bool Run() {
            var records = RecordFactory.BuildFromCsv(InputFile, Seed);
            records.OrderBy(x=>x.Sweep).WL();
            var param = new Parameter.Parameter(
                VtnVoltage, VtnSigma, VtnDeviation,
                VtpVoltage, VtpSigma, VtpDeviation
            );

            var repo = new PgSqlRepository();
            repo.BulkWrite(param, records);

            return true;
        }
    }
}