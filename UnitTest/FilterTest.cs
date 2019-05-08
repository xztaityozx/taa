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

    }
}
