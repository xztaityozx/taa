using CommandLine;

namespace taa.Verb {
    [Verb("push", HelpText = "DBにデータをPushします")]
    public class Push : SubCommand {
        [Value(0, Required = true, HelpText = "入力ファイルです", MetaName = "input")]
        public string InputFile { get; set; }

        [Option('S',"seed",HelpText = "シード値です", Default = 1)]
        public int Seed { get; set; }

        public override bool Run() {
            
            return true;
        }

        public override void Bind() {
            throw new System.NotImplementedException();
        }

        public override string ToString() {
            return $"Host: {Host}, Port: {Port}";
        }
    }
}