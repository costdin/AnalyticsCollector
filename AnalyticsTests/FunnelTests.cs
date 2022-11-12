using AnalyticsCollector.Library;
using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AnalyticsTests
{
    public class FunnelTests
    {
        IEqualityComparer<int> _equalityComparer;
        IFixture _fixture;

        public FunnelTests()
        {
            _equalityComparer = EqualityComparer<int>.Default;
            _fixture = new Fixture();
        }
        
        [Fact]
        public void FindBasicFunnel()
        {
            // Arrange
            var list = new[] { 1, 2, 3 };
            var subject = new FunnelFinder<int>();
            var expectedResult = new[] { 1, 1, 1 };

            // Act
            var result = subject.Find(list, list);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void FindFunnelWithSkips()
        {
            // Arrange
            var list = new[] { 1, 3, 2, 7, 3 };
            var funnel = new[] { 1, 2, 3 };
            var subject = new FunnelFinder<int>();
            var expectedResult = new[] { 1, 1, 1 };

            // Act
            var result = subject.Find(list, funnel);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void FindFunnelWithNoLastElement()
        {
            // Arrange
            var list = new[] { 1, 3, 2, 7, 5 };
            var funnel = new[] { 1, 2, 3 };
            var subject = new FunnelFinder<int>();
            var expectedResult = new[] { 1, 1, 0 };

            // Act
            var result = subject.Find(list, funnel);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void FindFunnelWithFirstElementTwice()
        {
            // Arrange
            var list = new[] { 1, 3, 2, 7, 3, 1 };
            var funnel = new[] { 1, 2, 3 };
            var subject = new FunnelFinder<int>();
            var expectedResult = new[] { 2, 1, 1 };

            // Act
            var result = subject.Find(list, funnel);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void FindFunnelWithTwoCyclesElementTwice()
        {
            // Arrange
            var list = new[] { 1, 3, 2, 7, 3, 1, 2, 1, 2, 3 };
            var funnel = new[] { 1, 2, 3 };
            var subject = new FunnelFinder<int>();
            var expectedResult = new[] { 2, 2, 2 };

            // Act
            var result = subject.Find(list, funnel);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void FindFunnelWithLongSkip()
        {
            // Arrange
            var list = new[] { 1 }
                .Concat(Enumerable.Repeat(2, 20000))
                .Concat(Enumerable.Repeat(1, 20000))
                .Concat(new[] { 3 })
                .ToArray();
            var funnel = new[] { 1, 2, 3 };
            var subject = new FunnelFinder<int>();
            var expectedResult = new[] { 1, 1, 1 };

            // Act
            var result = subject.Find(list, funnel);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void FuzzerTest()
        {
            var funnel = new[] { 1, 2, 3 };
            var list = _fixture.CreateMany<int>(10000)
                .Select(e => e % funnel.Max())
                .Select(e => e + 1)
                .ToArray();

            for (int j = 0; j < 10; j++)
            {
                // Arrange
                list = list
                    .OrderBy(i => Guid.NewGuid())
                    .ToArray();

                funnel = funnel
                    .OrderBy(i => Guid.NewGuid())
                    .ToArray();

                var subject = new FunnelFinder<int>();

                // Act
                var result = subject.Find(list, funnel);

                // Assert
                for (int i = 0; i < result.Length - 1; i++)
                {
                    Assert.True(result[i] >= result[i + 1]);
                }
            }
        }
    }
}
