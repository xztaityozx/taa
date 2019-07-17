using System.Linq;
using taa.Document;
using Xunit;

namespace UnitTest {
    public class DocumentTest {
        [Fact]
        public void ParseDecimalWithSiUnitTest() {
            var exp = new[] {1e9, 1e6, 1e3, 1e-3, 1e-6, 1e-9, 1e-12};
            var data = new[] {"G", "M", "K", "m", "u", "n", "p"}.Select((si, idx) => new {
                source = $"{idx + 1}{si}",
                expect = (decimal)((idx + 1) * exp[idx])
            });
            
            foreach (var t in data) {
                Assert.Equal(t.expect, Document.ParseDecimalWithSiUnit(t.source));
            }
        }

        [Fact]
        public void TryParseDecimalWithSiUnitTest() {
            var actual = Document.TryParseDecimalWithSiUnit("x", out _);
            Assert.False(actual);

            actual = Document.TryParseDecimalWithSiUnit("1n", out var k);
            Assert.Equal(1e-9M, k);
            Assert.True(actual);
        }
    }
}