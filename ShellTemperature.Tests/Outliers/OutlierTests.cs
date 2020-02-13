using System;
using System.Collections.Generic;
using NUnit.Framework;
using ShellTemperature.ViewModels.Outliers;

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
            OutlierDetector detector = new OutlierDetector();
            int outlierWeight = 5;

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
                bool isOutlier = detector.IsOutlier(latestReading, outlierWeight, temps);

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
            OutlierDetector detector = new OutlierDetector();

        }
    }
}