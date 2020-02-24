using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using NUnit.Framework;
using ShellTemperature.ViewModels.Interfaces;
using ShellTemperature.ViewModels.Outliers;
using ShellTemperature.ViewModels.Statistics;

namespace ShellTemperature.Tests.Outliers
{
    public class OutlierTests
    {
        /// <summary>
        /// Test if the current value is an outlier
        /// </summary>
        [Test]
        public void IsOutlier()
        {
            //Arrange
            ISorter sorter = new SortingAlgorithm();
            IBasicStats basicStats = new BasicStats(); ;
            IMeasureSpreadStats measureSpreadStats = new MeasureSpreadStats(sorter, basicStats);
            OutlierDetector detector = new OutlierDetector(measureSpreadStats);

            List<double> temps = new List<double>();
            Random random = new Random();

            for (int i = 0; i < 11; i++)
            {
                double rand = random.NextDouble();
                int randomInt = random.Next(22, 24);

                double num = randomInt + rand;
                temps.Add(num);
            }


            for (int i = 0; i < 100; i++)
            {
                int latestReading = random.Next(0, 9);
                // Act
                bool isOutlier = detector.IsOutlier(temps, latestReading);

                // Assert
                Assert.IsTrue(isOutlier);
            }
        }

        /// <summary>
        /// Test if the current value is an outlier
        /// </summary>
        [Test]
        public void IsNotOutlier()
        {
            ISorter sorter = new SortingAlgorithm();
            IBasicStats basicStats = new BasicStats(); ;
            IMeasureSpreadStats measureSpreadStats = new MeasureSpreadStats(sorter, basicStats);
            OutlierDetector detector = new OutlierDetector(measureSpreadStats);

            Random random = new Random();

            //1,2,3,4,2,3,1,4
            List<double> temps = new List<double>();

            for (int i = 0; i < 11; i++)
            {
                int num = random.Next(0, 4);
                double randDouble = random.NextDouble();

                double value = num + randDouble;

                temps.Add(value);
            }

            for (int i = 0; i < 5; i++)
            {
                bool isOutlier = detector.IsOutlier(temps, i);
                Assert.IsFalse(isOutlier);
            }
        }
    }
}