using NUnit.Framework;
using ShellTemperature.ViewModels.Interfaces;
using System;

namespace ShellTemperature.Tests.OutlierDetector
{
    public class OutlierDetectorTests
    {
        private readonly ViewModels.Outliers.OutlierDetector outlierDetector;

        public OutlierDetectorTests()
        {
            ISorter sorter = new ViewModels.Statistics.SortingAlgorithm();
            IBasicStats basicStats = new ViewModels.Statistics.BasicStats(sorter);
            IMeasureSpreadStats measureSpreadStats = new ViewModels.Statistics.MeasureSpreadStats(sorter, basicStats);

            outlierDetector = new ViewModels.Outliers.OutlierDetector(measureSpreadStats);
        }

        /// <summary>
        /// Test the IsOutlier method and it should come back
        /// that it is an outlier
        /// </summary>
        [Test]
        public void IsAnOutlier_EvenSet_Test()
        {
            // Arrange
            Random random = new Random();
            double[] values = new double[10];

            for (int i = 0; i < values.Length; i++)
            {
                int num = random.Next(20, 27);
                double dec = random.NextDouble();

                double value = num + dec;
                values[i] = value;
            }

            double[] latestReadings = new double[5];
            for (int i = 0; i < latestReadings.Length; i++)
            {
                latestReadings[i] = i;
            }

            // Act
            foreach (double reading in latestReadings)
            {
                bool isOutlier = outlierDetector.IsOutlier(values, reading);

                // Assert
                Assert.IsTrue(isOutlier);
            }
        }

        /// <summary>
        /// Test the IsOutlier method and it should come back
        /// that it is an outlier
        /// </summary>
        [Test]
        public void IsAnOutlier_OddSet_Test()
        {
            // Arrange
            Random random = new Random();
            double[] values = new double[9];

            for (int i = 0; i < values.Length; i++)
            {
                int num = random.Next(20, 27);
                double dec = random.NextDouble();

                double value = num + dec;
                values[i] = value;
            }

            double[] latestReadings = new double[5];
            for (int i = 0; i < latestReadings.Length; i++)
            {
                latestReadings[i] = i;
            }

            // Act
            foreach(double reading in latestReadings)
            {
                bool isOutlier = outlierDetector.IsOutlier(values, reading);

                // Assert
                Assert.IsTrue(isOutlier);
            }
        }

        /// <summary>
        /// Test the IsOutlier method and it should come back
        /// that it is an outlier
        /// </summary>
        [Test]
        public void IsNotOutlier_EvenSet_Test()
        {
            // Arrange
            Random random = new Random();
            double[] values = new double[10];

            for (int i = 0; i < values.Length; i++)
            {
                int num = random.Next(20, 27);
                double dec = random.NextDouble();

                double value = num + dec;
                values[i] = value;
            }

            double[] latestReadings = new double[2];
            for (int i = 0; i < latestReadings.Length; i++)
            {
                int num = random.Next(20, 27);
                double dec = random.NextDouble();

                double value = num + dec;
                latestReadings[i] = value;
            }

            // Act
            foreach (double reading in latestReadings)
            {
                bool isOutlier = outlierDetector.IsOutlier(values, reading);

                // Assert
                Assert.IsFalse(isOutlier);
            }
        }

        /// <summary>
        /// Test the IsOutlier method and it should come back
        /// that it is an outlier
        /// </summary>
        [Test]
        public void IsNotOutlier_OddSet_Test()
        {
            // Arrange
            Random random = new Random();
            double[] values = new double[9];

            for (int i = 0; i < values.Length; i++)
            {
                int num = random.Next(20, 27);
                double dec = random.NextDouble();

                double value = num + dec;
                values[i] = value;
            }

            double[] latestReadings = new double[3];
            for (int i = 0; i < latestReadings.Length; i++)
            {
                int num = random.Next(20, 27);
                double dec = random.NextDouble();

                double value = num + dec;
                latestReadings[i] = value;
            }

            // Act
            foreach (double reading in latestReadings)
            {
                bool isOutlier = outlierDetector.IsOutlier(values, reading);

                // Assert
                Assert.IsFalse(isOutlier);
            }
        }
    }
}
