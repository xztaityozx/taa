using System;
using System.Collections.Generic;
using System.Text;
using taa;
using Xunit;
using Record = taa.Record;

namespace UnitTest {
    public class WvCsvParserTest {
        [Fact]
        public void Test() {
            Assert.Equal(1e9M, Record.ParseDecimalWithSiUnit("1G"));
            Assert.Equal(1e6M, Record.ParseDecimalWithSiUnit("1M"));
            Assert.Equal(1e3M, Record.ParseDecimalWithSiUnit("1K"));
            Assert.Equal(1e-3M, Record.ParseDecimalWithSiUnit("1m"));
            Assert.Equal(1e-6M, Record.ParseDecimalWithSiUnit("1u"));
            Assert.Equal(1e-9M, Record.ParseDecimalWithSiUnit("1n"));
            Assert.Equal(1e-12M, Record.ParseDecimalWithSiUnit("1p"));

            Assert.Equal(1.5e9M, Record.ParseDecimalWithSiUnit("1.5G"));
            Assert.Equal(1.5e6M, Record.ParseDecimalWithSiUnit("1.5M"));
            Assert.Equal(1.5e3M, Record.ParseDecimalWithSiUnit("1.5K"));
            Assert.Equal(1.5e-3M, Record.ParseDecimalWithSiUnit("1.5m"));
            Assert.Equal(1.5e-6M, Record.ParseDecimalWithSiUnit("1.5u"));
            Assert.Equal(1.5e-9M, Record.ParseDecimalWithSiUnit("1.5n"));
            Assert.Equal(1.5e-12M, Record.ParseDecimalWithSiUnit("1.5p"));
        }
    }
}
