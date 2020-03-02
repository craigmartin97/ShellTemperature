using NUnit.Framework;
using ShellTemperature.ViewModels.Interfaces;
using ShellTemperature.ViewModels.Statistics;
using System;
using System.Linq;

namespace ShellTemperature.Tests.Statistics
{
    public class BasicStatsTests
    {
        private readonly IBasicStats _basicStats;

        public BasicStatsTests()
        {
            _basicStats = new BasicStats(new ViewModels.Statistics.SortingAlgorithm());
        }

        /// <summary>
        /// Check that the minimum algorithm finds the minimum for the data set
        /// </summary>
        [Test]
        public void Minimum_Test()
        {
            // Arrange
            double[] values = new double[]
            {
                10,9,2,11,1,2,3,4,12,33
            };
            double expectedMin = values.Min();

            // Act
            double min = _basicStats.Minimum(values);

            // Assert
            Assert.AreEqual(expectedMin, min);
        }

        /// <summary>
        /// Pass a null double array and a null argument exception should be thrown
        /// </summary>
        [Test]
        public void Minimum_NullSet_Test()
        {
            // Arrange
            double[] nullSet = null;

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _basicStats.Minimum(nullSet);
            });
        }

        /// <summary>
        /// Input a blank double array which should cause an 
        /// argument null expcetion
        /// </summary>
        [Test]
        public void Minimum_BlankSet_Test()
        {
            // Arrange
            double[] nullSet = new double[] { };

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _basicStats.Minimum(nullSet);
            });
        }

        /// <summary>
        /// Test that the maximum function finds the max for the data set
        /// </summary>
        [Test]
        public void Maximum_Test()
        {
            // Arrange
            double[] values = new double[]
            {
                10,9,2,11,1,2,3,4,12,33
            };

            double expectedMax = values.Max();

            // Act
            double max = _basicStats.Maximum(values);

            // Assert
            Assert.AreEqual(expectedMax, max);
        }

        /// <summary>
        /// Pass a null double array and a null argument exception should be thrown
        /// </summary>
        [Test]
        public void Maximum_NullSet_Test()
        {
            // Arrange
            double[] nullSet = null;

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _basicStats.Maximum(nullSet);
            });
        }

        /// <summary>
        /// Input a blank double array which should cause an 
        /// argument null expcetion
        /// </summary>
        [Test]
        public void Maximum_BlankSet_Test()
        {
            // Arrange
            double[] nullSet = new double[] { };

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _basicStats.Maximum(nullSet);
            });
        }

        /// <summary>
        /// Calculate the mean average for a set of doubles
        /// numerous times to ensure it is correct
        /// </summary>
        [Test]
        public void Mean_Test()
        {
            // Arrange
            Random random = new Random();

            double[] values = new double[10];
            int counter = 0;

            // test multiple times
            for (int i = 0; i < 300; i++)
            {
                if (i % 10 == 0) // div by 10
                {
                    // Calculate mean average, round to 2 decimal places
                    double expectedMean = Math.Round(values.Sum() / values.Length, 2);

                    // Act
                    double mean = _basicStats.Mean(values);

                    // Assert
                    Assert.AreEqual(expectedMean, mean);

                    values = new double[10];

                    counter = 0; // Reset counter
                }
                else
                {
                    int num = random.Next(0, 9);
                    double dec = random.NextDouble();

                    double value = num + dec;
                    values[counter] = value;

                    counter++;
                }
            }
        }

        /// <summary>
        /// Pass a null double array and a null argument exception should be thrown
        /// </summary>
        [Test]
        public void Mean_NullSet_Test()
        {
            // Arrange
            double[] nullSet = null;

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _basicStats.Mean(nullSet);
            });
        }

        /// <summary>
        /// Input a blank double array which should cause an 
        /// argument null expcetion
        /// </summary>
        [Test]
        public void Mean_BlankSet_Test()
        {
            // Arrange
            double[] nullSet = new double[] { };

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _basicStats.Mean(nullSet);
            });
        }

        /// <summary>
        /// Test that the mode function is working correctly
        /// </summary>
        [Test]
        public void Mode_Test()
        {
            // Arrange
            double[] values = new double[]
            {
                1, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 5, 6, 7
            };

            double expectedMode = 3; // Mode is 3 as double array above most often is clearly 3

            // Act
            double mode = _basicStats.Mode(values);

            // Assert
            Assert.AreEqual(expectedMode, mode);
        }

        /// <summary>
        /// Pass a null double array and a null argument exception should be thrown
        /// </summary>
        [Test]
        public void Mode_NullSet_Test()
        {
            // Arrange
            double[] nullSet = null;

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _basicStats.Mode(nullSet);
            });
        }

        /// <summary>
        /// Input a blank double array which should cause an 
        /// argument null expcetion
        /// </summary>
        [Test]
        public void Mode_BlankSet_Test()
        {
            // Arrange
            double[] nullSet = new double[] { };

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _basicStats.Mode(nullSet);
            });
        }

        /// <summary>
        /// Ensure that the median function is working as expected
        /// for an even set of data
        /// </summary>
        [Test]
        public void Median_EvenSet_Test()
        {
            // Arrange
            double[] values = new double[]
            {
                10, 11, 15, 19, 1, 12, 16, 15, 10, 2, 5, 7
            };

            double expectedMedian = 10.5;

            // Act
            double median = _basicStats.Median(values);

            // Assert
            Assert.AreEqual(expectedMedian, median);
        }

        /// <summary>
        /// Ensure that the median function is working as expected
        /// for an even set of data
        /// </summary>
        [Test]
        public void Median_OddSet_Test()
        {
            // Arrange
            double[] values = new double[]
            {
                10, 11, 15, 1, 12, 16, 15, 10, 2, 5, 7
            };

            double expectedMedian = 10;

            // Act
            double median = _basicStats.Median(values);

            // Assert
            Assert.AreEqual(expectedMedian, median);
        }

        /// <summary>
        /// Pass a null double array and a null argument exception should be thrown
        /// </summary>
        [Test]
        public void Median_NullSet_Test()
        {
            // Arrange
            double[] nullSet = null;

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _basicStats.Median(nullSet);
            });
        }

        /// <summary>
        /// Input a blank double array which should cause an 
        /// argument null expcetion
        /// </summary>
        [Test]
        public void Median_BlankSet_Test()
        {
            // Arrange
            double[] nullSet = new double[] { };

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _basicStats.Median(nullSet);
            });
        }
    }
}
