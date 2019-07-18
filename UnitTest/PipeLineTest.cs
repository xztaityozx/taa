using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using taa.PipeLine;
using Xunit;

namespace UnitTest {
    public class PipeLineTest {
        [Fact]
        public void PipeLine_InvokeTest() {
            try {
                using (var pp = new PipeLine(CancellationToken.None, 5)) {
                    var source = Enumerable.Range(0, 100).ToArray();
                    var len = source.Length;
                    var first = pp.Add("first", 2, source, i => i * 10);
                    var second = pp.Add("second", 2, first, len, i => i + 1);
                    var box = new List<int>();
                    pp.Add("assert", 1, second, len, i => { box.Add(i); });

                    var res = pp.Invoke(() => { });

                    Assert.Equal(PipeLine.PipeLineState.Completed, res);
                }
            }
            catch (IOException) {

            }
        }
    }
}