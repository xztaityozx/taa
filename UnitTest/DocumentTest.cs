using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using taa;
using Xunit;

namespace UnitTest {
    public class DocumentTest {
        [Fact]
        public void ParseDecimalWithSiUnitTest() {
            Assert.Equal(1e9M, Document.ParseDecimalWithSiUnit("1G"));
            Assert.Equal(1e6M, Document.ParseDecimalWithSiUnit("1M"));
            Assert.Equal(1e3M, Document.ParseDecimalWithSiUnit("1K"));
            Assert.Equal(1e-3M, Document.ParseDecimalWithSiUnit("1m"));
            Assert.Equal(1e-6M, Document.ParseDecimalWithSiUnit("1u"));
            Assert.Equal(1e-9M, Document.ParseDecimalWithSiUnit("1n"));
            Assert.Equal(1e-12M, Document.ParseDecimalWithSiUnit("1p"));

            Assert.Equal(1.5e9M, Document.ParseDecimalWithSiUnit("1.5G"));
            Assert.Equal(1.5e6M, Document.ParseDecimalWithSiUnit("1.5M"));
            Assert.Equal(1.5e3M, Document.ParseDecimalWithSiUnit("1.5K"));
            Assert.Equal(1.5e-3M, Document.ParseDecimalWithSiUnit("1.5m"));
            Assert.Equal(1.5e-6M, Document.ParseDecimalWithSiUnit("1.5u"));
            Assert.Equal(1.5e-9M, Document.ParseDecimalWithSiUnit("1.5n"));
            Assert.Equal(1.5e-12M, Document.ParseDecimalWithSiUnit("1.5p"));
        }

        [Fact]
        public void ConstructTest1() {
            var actual = new Document(1, 5000);
            Assert.Equal(1, actual.Seed);
            Assert.Equal(5000, actual.Sweeps);
        }

        [Fact]
        public void ConstructTest2() {
            const string str = @"#
TIME , A , B , C
# sweeps 1
1.0E1 , 2 , 3 , 4
2.0E1 , 5 , 6 , 7
3.0E1 , 8 , 9 , 0";
            var p = Path.Combine(Environment.CurrentDirectory, "test.csv");
            using (var sw = new StreamWriter(p, false, Encoding.UTF8)) sw.WriteLine(str);

            var actual = new Document(p, 1, 1);
            var first = actual.First();

            Assert.True(actual.KeyList.OrderBy(x => x).Zip(new[] {
                "A/1.0000000000E+001", "B/1.0000000000E+001", "C/1.0000000000E+001",
                "A/2.0000000000E+001", "B/2.0000000000E+001", "C/2.0000000000E+001",
                "A/3.0000000000E+001", "B/3.0000000000E+001", "C/3.0000000000E+001"
            }.OrderBy(x=>x), (s, t) => s == t).All(x => x));


            Assert.Equal(2, first[Document.EncodeKey("A", (decimal)1.0E1)]);
            Assert.Equal(3, first[Document.EncodeKey("B", (decimal)1.0E1)]);
            Assert.Equal(4, first[Document.EncodeKey("C", (decimal)1.0E1)]);

            Assert.Equal(5, first[Document.EncodeKey("A", (decimal)2.0E1)]);
            Assert.Equal(6, first[Document.EncodeKey("B", (decimal)2.0E1)]);
            Assert.Equal(7, first[Document.EncodeKey("C", (decimal)2.0E1)]);

            Assert.Equal(8, first[Document.EncodeKey("A", (decimal)3.0E1)]);
            Assert.Equal(9, first[Document.EncodeKey("B", (decimal)3.0E1)]);
            Assert.Equal(0, first[Document.EncodeKey("C", (decimal)3.0E1)]);

            File.Delete(p);
        }

        [Fact]
        public void EncodeKeyTest() {
            Assert.Equal("A/1.0000000000E+001", Document.EncodeKey("A", 10M));
        }

        [Fact]
        public void GenerateRecords() {
            const string str = @"#
TIME , A , B , C
# sweeps 1
1.0E1 , 2 , 3 , 4
2.0E1 , 5 , 6 , 7
3.0E1 , 8 , 9 , 0";
            var p = Path.Combine(Environment.CurrentDirectory, "test.csv");
            using (var sw = new StreamWriter(p, false, Encoding.UTF8)) sw.WriteLine(str);

            var d = new Document(p, 1, 1);
            var vtn = new Transistor(1, 2, 3);
            var vtp = new Transistor(4, 5, 6);
            var actual = d.GenerateRecords(vtn,vtp);

            foreach (var record in actual) {
                Assert.Equal(1, record.Seed);
                Assert.Equal(1,record.Sweeps);
                Assert.Equal(vtn, record.Vtn);
                Assert.Equal(vtp, record.Vtp);
            }

            Assert.Equal(new[] {
                    "A/1.0000000000E+001", "B/1.0000000000E+001", "C/1.0000000000E+001",
                    "A/2.0000000000E+001", "B/2.0000000000E+001", "C/2.0000000000E+001",
                    "A/3.0000000000E+001", "B/3.0000000000E+001", "C/3.0000000000E+001"
                }.OrderBy(x => x),
                actual.Select(x => x.Key).OrderBy(x => x)
            );

            File.Delete(p);
        }

        [Fact]
        public void BuildDocumentsTest() {
            const int sweep = 5;
            var values = new List<decimal[]>();
            const string signal = "A";
            var times = new[] {1M, 2M, 3M};
            var records = new List<taa.Record>();

            for (var i = 0; i < 3; i++) {
                values.Add(Enumerable.Repeat(0, sweep)
                    .Select(_ => (decimal) new Random(DateTime.Now.Millisecond).Next()).ToArray());
                records.Add(new taa.Record {
                    Values = values.Last(),
                    Sweeps = sweep,
                    Key = Document.EncodeKey(signal,times[i]),
                });
            }

            var d = Document.BuildDocuments(records);
            var actual = d.First();
            Assert.True(1 == d.Count());
            Assert.Equal(
                new[] {
                    Document.EncodeKey(signal, 1M), Document.EncodeKey(signal, 2M), Document.EncodeKey(signal, 3M),
                }, actual.KeyList
            );

            for (var i = 0; i < sweep; i++) {
                Assert.Equal(values[0][i], actual.ElementAt(i)[Document.EncodeKey(signal, 1M)]);
                Assert.Equal(values[1][i], actual.ElementAt(i)[Document.EncodeKey(signal, 2M)]);
                Assert.Equal(values[2][i], actual.ElementAt(i)[Document.EncodeKey(signal, 3M)]);
            }
        }
    }
}
