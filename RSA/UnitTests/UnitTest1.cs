using NUnit.Framework;
using RSA;
using static RSA.Program;

namespace Tests {
public class Tests {
    [SetUp]
    public void Setup() { }

    [Test]
    public void TestRndBigIntegerWithLimits() {
        for (var i = 0; i < 1000; i++) {
            var min = Algorithms.GenerateRandomNumber(i);
            var max = Algorithms.GenerateRandomNumber(i);

            if (min > max) {
                var tmp = min;
                min = max;
                max = tmp;
            }

            var gen = Algorithms.GenerateRandomNumber(min, max);
            Assert.GreaterOrEqual(gen, min);
            Assert.LessOrEqual(gen, max);
        }
        
    }
}
}