using NUnit.Framework;
using ShellTemperature.ViewModels.Interfaces;
using ShellTemperature.ViewModels.Statistics;
using System;
using System.Linq;

namespace ShellTemperature.Tests.Statistics
{
    public class MeasureSpreadStatsTests
    {
        private readonly ISorter _sorter;
        private readonly IBasicStats _basicStats;
        private readonly MeasureSpreadStats _measureSpreadStats;

        public MeasureSpreadStatsTests()
        {
            _sorter = new ViewModels.Statistics.SortingAlgorithm();
            _basicStats = new BasicStats(_sorter);
            _measureSpreadStats = new MeasureSpreadStats(_sorter, _basicStats);
        }

        /// <summary>
        /// Calculate the range of a data set and ensure the algorithm is working 
        /// correctly
        /// </summary>
        [Test]
        public void Range_Test()
        {
            // Arrange
            double[] values = new double[]
            {
                10, 9, 12, 22, 11, 1, 10, 9, 3, 4, 10, 13
            };

            double expectedRange = 22 - 1;

            // Act
            double range = _measureSpreadStats.Range(values);

            // Assert
            Assert.AreEqual(expectedRange, range);
        }

        /// <summary>
        /// Calculate the range when there are only two values in
        /// the data set
        /// </summary>
        [Test]
        public void Range_TwoValues_Test()
        {
            // Arrange
            double[] values = new double[]
            {
                22,11
            };

            double expectedRange = 22 - 11;

            // Act
            double range = _measureSpreadStats.Range(values);

            // Assert
            Assert.AreEqual(expectedRange, range);
        }

        /// <summary>
        /// Input a null double array and the expected result
        /// should be a argument null exception
        /// </summary>
        [Test]
        public void Range_NullSet_Test()
        {
            // Arrange
            double[] nullSet = null;

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _measureSpreadStats.Range(nullSet);
            });
        }

        /// <summary>
        /// Input a blank double array and the expected result
        /// should be a argument null exception
        /// </summary>
        [Test]
        public void Range_BlankSet_Test()
        {
            // Arrange
            double[] nullSet = new double[] { };

            // Assert
            Assert.Throws<ArgumentNullException>(delegate
            {
                _measureSpreadStats.Range(nullSet);
            });
        }

        /// <summary>
        /// Try and calculate the range with only one value in the data set.
        /// This is an invalid operation because you cannot calculate the range with only one
        /// value
        /// </summary>
        [Test]
        public void Range_OneValue_Test()
        {
            // Arrange
            double[] nullSet = new double[]
            {
                100
            };

            // Assert
            Assert.Throws<InvalidOperationException>(delegate
            {
                _measureSpreadStats.Range(nullSet);
            });
        }

        /// <summary>
        /// Manually calculate the mean deviation and
        /// compare it with the calculated mean deviation to ensure
        /// they are the same
        /// </summary>
        [Test]
        public void MeanDeviation_Test()
        {
            // Arrange
            double[] values = new double[]
            {
                1, 2, 3, 4, 5
            };

            double valuesMean = 3; // 5+4+3+2+1 / 5

            double[] distances = new double[]
            {
                Math.Abs(valuesMean - 1), // ABS: 3-1 = 2
                Math.Abs(valuesMean - 2), // ABS: 3-2 = 1
                Math.Abs(valuesMean - 3), // ABS: 3-3 = 0
                Math.Abs(valuesMean - 4), // ABS: 3-4 = 1
                Math.Abs(valuesMean - 5)  // ABS: 3-5 = 2
            };

            //2+1+0+1+2 / 5 = 6 / 5 = 1.2
            double meanDistance = (double)6 / 5;
            double expectedMeanDeviation = Math.Round(meanDistance, 2);

            // Act
            double meanDeviation = _measureSpreadStats.MeanDeviation(values);

            // Assert
            Assert.AreEqual(expectedMeanDeviation, meanDeviation);
        }

        /// <summary>
        /// Manually calculate the standard deviation and compare
        /// with the calculated version to ensure they are the same
        /// </summary>
        [Test]
        public void StandardDeviation_Test()
        {
            // Arrange
            double[] values = new double[]
            {
                1, 2, 3, 4, 5
            };

            double valuesMean = 3; // 5+4+3+2+1 / 5

            double[] variances = new double[]
            {
                (3-1)*(3-1), // 3-1 = 2 || 2*2 = 4
                (3-2)*(3-2), // 3-2 = 1 || 1*1 = 1
                (3-3)*(3-3), // 3-3 = 0 || 0*0 = 0
                (3-4)*(3-4), // 3-4 = -1 || -1*-1 = 1
                (3-5)*(3-5)  // 3-5 = -2 || -2*-2 = 4
            };

            double totalVariance = (double)(4 + 1 + 0 + 1 + 4) / 5;
            double expectedStandardDeviation = Math.Round(Math.Sqrt(totalVariance), 2);

            // Act
            double standardDeviation = _measureSpreadStats.StandardDeviation(values);

            // Assert
            Assert.AreEqual(expectedStandardDeviation, standardDeviation);
        }

        [Test]
        public void GetQuartiles_OddSet_Test()
        {
            Random random = new Random();
            // Arrange
            double[] values = new double[19];

            // Generate random numbers between 18 and 25
            for (int i = 0; i < 19; i++)
            {
                int num = random.Next(18, 24);
                double dec = random.NextDouble();

                double value = num + dec;
                values[i] = value;
            }

            // Order the values in numerical order
            values = values.OrderBy(x => x).ToArray();

            //5 Elements     2 | 2   Two either side of median
            //7     3 | 3 (values.Length - 1) / 2
            //9     4 | 4   
            //11    5 | 5
            //13    6 | 6
            //15    7 | 7
            //17    8 | 8
            //19    9 | 9   
            // Get the size of the array. Table above explains
            int size = (values.Length - 1) / 2;
            double[] expectedFirstQuantile = new double[size];
            double[] expectedThirdQuantile = new double[size];

            // Add values to exFirstQuartile
            for (int i = 0; i < size; i++)
            {
                expectedFirstQuantile[i] = values[i];
            }

            // Add values to exThirdQuartile
            int counter = 0;
            for (int i = size + 1; i < values.Length; i++)
            {
                expectedThirdQuantile[counter] = values[i];
                counter++;
            }

            // Act
            _measureSpreadStats.GetQuartiles(values, out double[] firstQuantile, out double[] thirdQuantile);

            // Assert
            Assert.IsNotNull(firstQuantile);
            Assert.IsNotNull(thirdQuantile);

            Assert.IsTrue(firstQuantile.Length == 9);
            Assert.IsTrue(thirdQuantile.Length == 9);

            Assert.AreEqual(expectedFirstQuantile, firstQuantile);
            Assert.AreEqual(expectedThirdQuantile, thirdQuantile);
        }

        [Test]
        public void GetQuartiles_EvenSet_Test()
        {
            Random random = new Random();
            // Arrange
            double[] values = new double[20];

            // Generate random numbers between 18 and 25
            for (int i = 0; i < 20; i++)
            {
                int num = random.Next(18, 24);
                double dec = random.NextDouble();

                double value = num + dec;
                values[i] = value;
            }

            // Order the values in numerical order
            values = values.OrderBy(x => x).ToArray();

            //6     3 | 3
            //8     4 | 4
            //10    5 | 5 
            //20    10 | 10

            // Get the size of the array. Table above explains
            int size = values.Length / 2;
            double[] expectedFirstQuantile = new double[size];
            double[] expectedThirdQuantile = new double[size];

            // Add values to exFirstQuartile
            for (int i = 0; i < size; i++)
            {
                expectedFirstQuantile[i] = values[i];
            }

            // Add values to exThirdQuartile
            int counter = 0;
            for (int i = size; i < values.Length; i++)
            {
                expectedThirdQuantile[counter] = values[i];
                counter++;
            }

            // Act
            _measureSpreadStats.GetQuartiles(values, out double[] firstQuantile, out double[] thirdQuantile);

            // Assert
            Assert.IsNotNull(firstQuantile);
            Assert.IsNotNull(thirdQuantile);

            Assert.IsTrue(firstQuantile.Length == 10);
            Assert.IsTrue(thirdQuantile.Length == 10);

            Assert.AreEqual(expectedFirstQuantile, firstQuantile);
            Assert.AreEqual(expectedThirdQuantile, thirdQuantile);
        }
    }
}
