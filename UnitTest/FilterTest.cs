using System;
using System.Collections.Generic;
using Xunit;
using taa;

namespace UnitTest {
    public class FilterTest {
        [Fact]
        public void ParseDecimalWithSIUnitTest() {
            Assert.Equal(1e9M, Filter.ParseDecimalWithSiUnit("1G"));
            Assert.Equal(1e6M, Filter.ParseDecimalWithSiUnit("1M"));
            Assert.Equal(1e3M, Filter.ParseDecimalWithSiUnit("1K"));
            Assert.Equal(1e-3M, Filter.ParseDecimalWithSiUnit("1m"));
            Assert.Equal(1e-6M, Filter.ParseDecimalWithSiUnit("1u"));
            Assert.Equal(1e-9M, Filter.ParseDecimalWithSiUnit("1n"));
            Assert.Equal(1e-12M, Filter.ParseDecimalWithSiUnit("1p"));

            Assert.Equal(1.5e9M, Filter.ParseDecimalWithSiUnit("1.5G"));
            Assert.Equal(1.5e6M, Filter.ParseDecimalWithSiUnit("1.5M"));
            Assert.Equal(1.5e3M, Filter.ParseDecimalWithSiUnit("1.5K"));
            Assert.Equal(1.5e-3M, Filter.ParseDecimalWithSiUnit("1.5m"));
            Assert.Equal(1.5e-6M, Filter.ParseDecimalWithSiUnit("1.5u"));
            Assert.Equal(1.5e-9M, Filter.ParseDecimalWithSiUnit("1.5n"));
            Assert.Equal(1.5e-12M, Filter.ParseDecimalWithSiUnit("1.5p"));
        }

        [Fact]
        public void BuildTest() {
            var f = new Filter(new Dictionary<string, string> {
                ["A"] = "BLB[4n]>0.6",
                ["B"] = "BL[4n]<0.6",
            }, new List<string> {
                "!(A&&B)", "A&&B"
            });

            f.Build();

            var (sa, da, ca) = f["A"];

            Assert.Equal("BLB",sa);
            Assert.Equal(4e-9M, da);
            Assert.Equal(">0.6", ca);

            var (sb, db, cb) = f["B"];

            Assert.Equal("BL", sb);
            Assert.Equal(4e-9M, db);
            Assert.Equal("<0.6", cb);

            Assert.Equal(new[]{"not","(","A","and","B",")"}, f[0]);
            Assert.Equal(new[]{"A","and","B"}, f[1]);

        }
    }
}
