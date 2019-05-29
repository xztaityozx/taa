using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using taa;
using Xunit;

namespace UnitTest {
    public class RequestTest {
        [Fact]
        public void GenerateRequestsTest() {
            var actual = Request.GenerateRequests(new Transistor(1, 2, 3), new Transistor(4, 5, 6), 7, 8, 9,
                new[] {"A/10", "B/11", "C/12"});

            var enumerable = actual as Request[] ?? actual.ToArray();
            Assert.True(enumerable.All(r => r.Vtn == new Transistor(1, 2, 3)));
            Assert.True(enumerable.All(r => r.Vtp == new Transistor(4, 5, 6)));
            Assert.True(enumerable.All(r => r.Sweeps == 7));
            Assert.True(enumerable.All(r => r.SeedStart == 8));
            Assert.True(enumerable.All(r => r.SeedEnd == 9));
            Assert.Equal(new[] {"A/10", "B/11", "C/12"}, enumerable.Select(r => r.Key));
        }
    }
}
