using System;
using System.Collections.Generic;
using System.Text;
using taa;
using Xunit;

namespace UnitTest {
    public class CounterLuaTest {
        [Fact]
        public void AddStatusTest() {
            var cl = new CounterLua();
            cl.AddStatus("true or false");

            Assert.True(cl[0]);

            cl.AddStatus("false or false");

            Assert.True(cl[0]);
            Assert.False(cl[1]);

            cl.AddStatus("true and true");

            Assert.True(cl[0]);
            Assert.False(cl[1]);
            Assert.True(cl[2]);
        }

        [Fact]
        public void GetResultsTest() {
            var cl = new CounterLua();
            cl.AddStatus("true or false");
            cl.AddStatus("false or false");
            cl.AddStatus("true and true");

            Assert.Equal(new[] {true, false, true}, cl.GetResults());
        }
    }
}
