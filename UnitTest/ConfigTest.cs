using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using taa;

namespace UnitTest {
    public class ConfigTest {
        private const string Yml = @"conditions: 
    A: N1[2.5n]<=10
    B: N2[10.5n]<=0.7n
    C: N3[17.5]<=0.1
expressions:
    - A&&B
    - A&&!B&&C
parallel: 10
database: 
    host: localhost
    port: 27017
    dbName: test
    collectionName: result";
        readonly string path = Path.Combine(Environment.CurrentDirectory, "test.yml");

        private void Write() {
            using (var sw = new StreamWriter(path)) {
                sw.WriteLine(Yml);
            }
        }
        [Fact]
        public void ConstructTest1() {
            Write();
            var actual = new Config(path, 0, "", 0, "", "");

            Assert.Equal(10, actual.Parallel);
            Assert.Equal("localhost",actual.Database.Host);
            Assert.Equal(27017, actual.Database.Port);
            Assert.Equal("test",actual.Database.DataBaseName);
            Assert.Equal("result", actual.Database.CollectionName);
            Assert.Equal(2, actual.Expressions.Count);
            Assert.Equal("A&&B", actual.Expressions[0]);
            Assert.Equal("A&&!B&&C", actual.Expressions[1]);
            Assert.True(
                new Dictionary<string, string> {
                    ["A"] = "N1[2.5n]<=10",
                    ["B"] = "N2[10.5n]<=0.7n",
                    ["C"] = "N3[17.5]<=0.1"
                }.All(x => x.Value == actual.Conditions[x.Key]));
        }

        [Fact]
        public void ConstructTest2() {
            Write();

            var actual = new Config(path, 1, "111.222.333.444", 8000, "db", "col");
            Assert.Equal(1, actual.Parallel);
            Assert.Equal("111.222.333.444", actual.Database.Host);
            Assert.Equal(8000, actual.Database.Port);
            Assert.Equal("db", actual.Database.DataBaseName);
            Assert.Equal("col", actual.Database.CollectionName);
            Assert.Equal(2, actual.Expressions.Count);
            Assert.Equal("A&&B", actual.Expressions[0]);
            Assert.Equal("A&&!B&&C", actual.Expressions[1]);
            Assert.True(
                new Dictionary<string, string> {
                    ["A"] = "N1[2.5n]<=10",
                    ["B"] = "N2[10.5n]<=0.7n",
                    ["C"] = "N3[17.5]<=0.1"
                }.All(x => x.Value == actual.Conditions[x.Key]));
        }

        [Fact]
        public void DataBaseConfigToStringTest() {
            Write();
            var actual = new Config(path, 0, "", 0, "", "").Database;
            Assert.Equal("mongodb://localhost:27017", actual.ToString());
        }

        [Fact]
        public void Finish() {
            File.Delete(path);
        }
    }
}
