using System;
using taa;
using Xunit;
using taa.Parameter;

namespace UnitTest {
    public class ParameterTest {
        [Fact]
        public void GenerateKeyTest() {
            var data = new[] {
                new {expect = "A/1.0000000000E-001", signal = "A", time = 1e-1M},
                new {expect = "B/2.5000000000E-003", signal = "B", time = 2.5e-3M},
                new {expect = "C/2.0600000000E-008", signal = "C", time = 20.6e-9M},
                new {expect = "ABC/1.0000000000E+002", signal = "ABC", time = 100M},
            };

            foreach (var t in data) {
                Assert.Equal(t.expect, Parameter.EncodeKey(t.signal, t.time));
            }
        }

        [Fact]
        public void ConstructorTest() {
            var actual = new Parameter(0.1, 0.2, 0.3, 1, 2, 3, 5, "signal", 7M);
            Assert.Equal(new Transistor(0.1, 0.2, 0.3), actual.Vtn);
            Assert.Equal(new Transistor(1, 2, 3), actual.Vtp);
            Assert.Equal(new Transistor(0.1, 0.2, 0.3), actual.Vtn);
            Assert.Equal(Parameter.EncodeKey("signal", 7M), actual.Key);
        }

        [Fact]
        public void DecodeKeyTest() {
            var data = new[] {
                new {source = "A/1.0000000000E-001", signal = "A", time = 1e-1M},
                new {source = "B/2.5000000000E-003", signal = "B", time = 2.5e-3M},
                new {source = "C/2.0600000000E-008", signal = "C", time = 20.6e-9M},
                new {source = "ABC/1.0000000000E+002", signal = "ABC", time = 100M},
            };

            foreach (var t in data) {
                Assert.Equal(Tuple.Create(t.signal,t.time), Parameter.DecodeKey(t.source));
            }
        }
    }
}