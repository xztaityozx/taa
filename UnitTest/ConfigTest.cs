using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using taa.Config;

namespace UnitTest {
    public class ConfigTest {
        private const string Yml = @"conditions: 
    A: N1[2.5n]<=10
    B: N2[10.5n]<=0.7n
    C: N3[17.5]<=0.1
    D: N3[2.5m]<=N1[2.5m]
expressions:
    - A&&B
    - A&&!B&&C
logDir: ~/Log
connection: connection";

        private readonly string path = Path.Combine(Environment.CurrentDirectory, "test.yml");

        [Fact]
        public void LoadTest() {
            using (var sw = new StreamWriter(path)) {
                sw.WriteLine(Yml);
            }

            var actual = Config.GetInstance(path);

            Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Log"),
                actual.LogDir);
            Assert.True(new List<string> {
                "A&&B", "A&&!B&&C"
            }.SequenceEqual(actual.Expressions));

            Assert.True(new Dictionary<string, string> {
                {"A", "N1[2.5n]<=10"},
                {"B", "N2[10.5n]<=0.7n"},
                {"C", "N3[17.5]<=0.1"},
                {"D", "N3[2.5m]<=N1[2.5m]"}
            }.SequenceEqual(actual.Conditions));

            Assert.Equal("connection", actual.ConnectionsString);
            File.Delete(path);
        }
    }
}
