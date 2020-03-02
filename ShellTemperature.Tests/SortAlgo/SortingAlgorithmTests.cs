using NUnit.Framework;
using ShellTemperature.ViewModels.Interfaces;
using System;
using System.Linq;

namespace ShellTemperature.Tests.SortAlgo
{
    public class SortingAlgorithmTests
    {
        private readonly ISorter _sorter;

        public SortingAlgorithmTests()
        {
            _sorter = new ViewModels.Statistics.SortingAlgorithm();
        }

        /// <summary>
        /// Bubble sort a list and compare each item at each index
        /// to make sure they are the same
        /// </summary>
        [Test]
        public void BubbleSort_AlreadyOrdered_Test()
        {
            // Arrange
            Random random = new Random();
            double[] orderedValues = new double[10]
            {
                1,2,3,4,5,6,7,8,9,10
            };

            double[] unorderedValues = new double[10]
            {
                10,9,8,7,6,5,4,3,2,1
            };

            // Act
            double[] bubbleSortedValues = _sorter.BubbleSort(unorderedValues);

            // Assert
            Assert.IsTrue(unorderedValues.Length == bubbleSortedValues.Length);

            // Check that each value is the same at each index
            for (int i = 0; i < bubbleSortedValues.Length; i++)
            {
                Assert.AreEqual(orderedValues[i], bubbleSortedValues[i]);
            }
        }

        /// <summary>
        /// Bubble sort a collection
        /// </summary>
        [Test]
        public void BubbleSort_Test()
        {
            // Arrange
            Random random = new Random();
            double[] values = new double[100];

            for (int i = 0; i < 100; i++)
            {
                int num = random.Next(0, 99);
                double dec = random.NextDouble();

                double value = num + dec;
                values[i] = value;
            }

            double[] sortedValues = values.OrderBy(x => x).ToArray();

            // Act
            double[] bubbleSortedValues = _sorter.BubbleSort(values);

            // Assert
            Assert.IsTrue(sortedValues.Length == bubbleSortedValues.Length);

            // Check that each value is the same at each index
            for (int i = 0; i < bubbleSortedValues.Length; i++)
            {
                Assert.AreEqual(sortedValues[i], bubbleSortedValues[i]);
            }
        }
    }
}
