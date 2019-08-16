using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using taa;
using taa.Config;
using taa.Extension;
using Xunit;

namespace UnitTest {
    public class FilterTest {

        [Fact]
        public void Construct() {
            var map = new Dictionary<string, string> {
                ["A"] = "s[1n]<=t[2G]",
                ["B"] = "s[4n]>=0.6"
            };
            var exp = new[] {"A&&B", "!(B)"};

            var filter = new Filter(map, exp);

            foreach (var a in exp.Zip(filter.Delegates.Select(d => d.Name), (s, t) => new {expect = s, actual = t})) {
                Assert.Equal(a.expect, a.actual);
            }

            var input = new Map<string, decimal> {
                [$"s/{1E-9M:E10}"] = 5,
                [$"t/{2E9M:E10}"] = 7,
                [$"s/{4E-9M:E10}"] = 0.1M
            };
            Assert.False(filter.Delegates[0].Filter(input));
            Assert.True(filter.Delegates[1].Filter(input));
        }

        [Fact]
        public void Aggregate() {
            var map = new Dictionary<string, string>
            {
                ["A"] = "s[1n]<=t[2G]",
                ["B"] = "s[4n]>=10"
            };
            var exp = new[] { "A&&B", "!(B)" };

            var filter = new Filter(map, exp);
            var expect = new[] { 0L, 0L };

            var rnd =new Random(DateTime.Now.Millisecond);
            var inputs = Enumerable.Range(0, 100).Select(_ => {
                var s1 = rnd.Next();
                var s2 = rnd.Next();
                var t = rnd.Next();
                var d = new Map<string, decimal> {
                    [$"s/{1E-9M:E10}"] = s1,
                    [$"t/{2E9M:E10}"] = t,
                    [$"s/{4E-9M:E10}"] = s2
                };

                if (s1 <= t && s2 >= 10) expect[0]++;
                if (!(s2 >= 10)) expect[1]++;

                return d;
            }).ToList();

            var res = filter.Aggregate(inputs);
            Assert.True(exp.SequenceEqual(res.Select(t => t.Item1)));
            Assert.True(expect.SequenceEqual(res.Select(t => t.Item2)));
        }
    }
}
