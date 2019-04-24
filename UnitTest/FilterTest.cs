using System;
using System.Collections.Generic;
using Xunit;
using taa;

namespace UnitTest {
    public class FilterTest {
        [Fact]
        public void ParseSIUnitTest() {
            Assert.Equal(1e9M, Filter.ParseSIUnit("1G"));
            Assert.Equal(1e6M, Filter.ParseSIUnit("1M"));
            Assert.Equal(1e3M, Filter.ParseSIUnit("1K"));
            Assert.Equal(1e-3M, Filter.ParseSIUnit("1m"));
            Assert.Equal(1e-6M, Filter.ParseSIUnit("1u"));
            Assert.Equal(1e-9M, Filter.ParseSIUnit("1n"));
            Assert.Equal(1e-12M, Filter.ParseSIUnit("1p"));

            Assert.Equal(1.5e9M, Filter.ParseSIUnit("1.5G"));
            Assert.Equal(1.5e6M, Filter.ParseSIUnit("1.5M"));
            Assert.Equal(1.5e3M, Filter.ParseSIUnit("1.5K"));
            Assert.Equal(1.5e-3M, Filter.ParseSIUnit("1.5m"));
            Assert.Equal(1.5e-6M, Filter.ParseSIUnit("1.5u"));
            Assert.Equal(1.5e-9M, Filter.ParseSIUnit("1.5n"));
            Assert.Equal(1.5e-12M, Filter.ParseSIUnit("1.5p"));
        }

        [Fact]
        public void BuildTest() {
            var f = new Filter(new Dictionary<string, string> {
                ["A"] = "BLB[14n]>0.6",
                ["B"] = "BL[14n]<0.6",
                ["C"] = "N1[4n]<0.6",
                ["D"] = "N2[4n]>0.6",
                ["E"] = "N1[19n]<0.6",
                ["F"] = "N2[19n]>0.6",
            }, new List<string> {
                "!C||!D",
                "A&&B&&C&&D&&E&&F"
            });
            f.Build();
               
            Assert.Equal(Tuple.Create("BLB", 14e-9M), f["A"]);
            Assert.Equal(Tuple.Create("BL", 14e-9M), f["B"]);
            Assert.Equal(Tuple.Create("N1", 4e-9M), f["C"]);
            Assert.Equal(Tuple.Create("N2", 4e-9M), f["D"]);
            Assert.Equal(Tuple.Create("N1", 19e-9M), f["E"]);
            Assert.Equal(Tuple.Create("N2", 19e-9M), f["F"]);

            Assert.True(f["BLB", 14e-9M](0.61M));
            Assert.True(f["BL", 14e-9M](0.59M));
            Assert.True(f["N1", 4e-9M](0.59M));
            Assert.True(f["N2", 4e-9M](0.61M));
            Assert.True(f["N2", 19e-9M](0.61M));
            Assert.True(f["N1", 19e-9M](0.59M));

            Assert.False(f["BLB", 14e-9M](0.59M));
            Assert.False(f["BL", 14e-9M](0.61M));
            Assert.False(f["N1", 4e-9M](0.61M));
            Assert.False(f["N2", 4e-9M](0.59M));
            Assert.False(f["N2", 19e-9M](0.59M));
            Assert.False(f["N1", 19e-9M](0.61M));

            Assert.Equal(new[] {"!C", "||", "!D"}, f[0]);
            Assert.Equal(new[] {"A", "&&", "B", "&&", "C", "&&", "D", "&&", "E", "&&", "F"}, f[1]);
        }
    }
}
