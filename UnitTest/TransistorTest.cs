using System;
using System.Collections.Generic;
using System.Text;
using taa;
using taa.Parameter;
using Xunit;

namespace UnitTest {
    public class TransistorTest {
        [Fact]
        public void ConstructTest() {
            var actual = new Transistor(0.6,0.046,1.0);
            Assert.Equal(0.6M, actual.Voltage);
            Assert.Equal(0.046M, actual.Sigma);
            Assert.Equal(1.0M, actual.Deviation);
        }

        [Fact]
        public void EqualTest() {
            Assert.Equal(new Transistor(0.6, 0.046, 1.0), new Transistor(0.6, 0.046, 1.0));
            Assert.NotEqual(new Transistor(0.6, 1, 2), new Transistor(0.6, 1, 3));
            Assert.NotEqual(new Transistor(0.6, 1, 2), new Transistor(0.6, 3, 2));
            Assert.NotEqual(new Transistor(0.6, 1, 2), new Transistor(-0.6, 1, 2));
        }
    }
}
