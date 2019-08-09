using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using taa;
using taa.Extension;
using Xunit;

namespace UnitTest
{
    public class ExtensionTest {
        [Fact]
        public void ExpandTest() {
            var expect = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Test"
            );

            var actual = FilePath.Expand(Path.Combine("~", "Test"));
            Assert.Equal(expect, actual);

            Assert.Equal(expect, FilePath.Expand("~/Test"));
        }

        [Fact]
        public void ParseDecimalWithSiUnitTest() {
            var data = new Dictionary<string, decimal> {
                ["1G"] = 1E9M,
                ["2M"] = 2E6M,
                ["3K"] = 3000,
                ["4m"] = 4E-3M,
                ["5u"] = 5E-6M,
                ["6n"] = 6E-9M,
                ["7p"] = 7E-12M
            };

            foreach (var (k,v) in data) {
                Assert.Equal(k.ParseDecimalWithSiUnit(), v);
            }
        }

        [Fact]
        public void TryParseDecimalWithSiUnitTest() {
            var data = new Dictionary<string, decimal> {
                ["1G"] = 1E9M,
                ["2M"] = 2E6M,
                ["3K"] = 3000,
                ["4m"] = 4E-3M,
                ["5u"] = 5E-6M,
                ["6n"] = 6E-9M,
                ["7p"] = 7E-12M
            };

            foreach (var (k, v) in data) {
                Assert.True(k.TryParseDecimalWithSiUnit(out var x));
                Assert.Equal(v, x);
            }

            Assert.False("1o".TryParseDecimalWithSiUnit(out _));
        }

        [Fact]
        public void WithinTest() {
            var data = new[] {
                new {lower = 0.1M, upper = 10.0M, input = 5M, expect = true},
                new {lower = 0.1M, upper = 10.0M, input = -5M, expect = false},
                new {lower = 0.1M, upper = 10.0M, input = 50M, expect = false},
            };
            foreach (var d in data) {
                Assert.Equal(d.input.Within(d.lower,d.upper), d.expect);
            }
        }
    }
}
