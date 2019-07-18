using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using taa;
using taa.Extension;
using Xunit;

namespace UnitTest
{
    public class ExtensionTest {
        [Fact]
        public void ExpandTest() {
            var expect = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Test"
            );

            var actual = FilePath.Expand(Path.Combine("~", "Test"));
            Assert.Equal(expect, actual);

            Assert.Equal(expect, FilePath.Expand("~/Test"));
        }
    }
}
