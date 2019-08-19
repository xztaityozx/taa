using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using taa.PipeLine;
using Xunit;

namespace UnitTest {
    public class PipeLineTest {
        [Fact]
        public void ConstructorTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                Assert.NotNull(p);
            }
        }

        [Fact]
        public void AddTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var first = p.Add(10, 10, new[] {10, 20}, i => i * 2);
                Assert.Equal(PipeLineStageMode.Select, first.Mode);
                var second = p.Add(10, 10, first.Results, i => $"{i}");
                Assert.Equal(PipeLineStageMode.Select, second.Mode);
            }
        }

        [Fact]
        public void AddSelectManyTest() {
            using (var p = new PipeLine(CancellationToken.None)) {
                var first = p.AddSelectMany(10, 10, new[] {10, 20}, i => Enumerable.Range(0, i));
                Assert.Equal(PipeLineStageMode.SelectMany, first.Mode);
            }
        }

        [Fact]
        public void InvokeTest() {
            var source = new[] {1, 2, 3, 4, 5};
            using (var p = new PipeLine(CancellationToken.None)) {
                var first = p.Add(1, 10, source, i => i * 10);
                var second = p.AddSelectMany(1, 100, first.Results, i => Enumerable.Range(0, i));
                var list = new List<int>();
                p.Invoke(() => { list.AddRange(second.Results.GetConsumingEnumerable()); });

                foreach (var x in list.OrderBy(x => x).Zip(source.Select(x => x * 10).SelectMany(x => Enumerable.Range(0, x)).OrderBy(x=>x),
                    (s, t) => new {s, t})) {
                    Assert.Equal(x.s, x.t);
                }
            }
        }
    }
}
