using NUnit.Framework;
using ShellTemperature.ViewModels.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ShellTemperature.ViewModels.Statistics;

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
            _sorter.BubbleSort(unorderedValues);

            // Assert
            Assert.IsTrue(unorderedValues.Length == unorderedValues.Length);

            // Check that each value is the same at each index
            for (int i = 0; i < unorderedValues.Length; i++)
            {
                Assert.AreEqual(orderedValues[i], unorderedValues[i]);
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
            _sorter.BubbleSort(values);

            // Assert
            Assert.IsTrue(sortedValues.Length == values.Length);

            // Check that each value is the same at each index
            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(sortedValues[i], values[i]);
            }
        }

        [Test]
        public void Quick_Sort()
        {
            // 3,4,7,9,12,20,21,22,25
            double[] values = new double[1000];
            Random random = new Random();
            for (int i = 0; i < values.Length; i++)
            {
                int num = random.Next(0, 200);
                double dec = random.NextDouble();

                double val = num + dec;
                values[i] = val;
            }

            double[] copy = new double[values.Length];
            values.CopyTo(copy,0);

            SortingAlgorithm sortingAlgorithm = new SortingAlgorithm();
            sortingAlgorithm.QuickSort(values, 0, values.Length - 1);

            copy = copy.OrderBy(x => x).ToArray(); // trusted computing base

            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(copy[i], values[i]);
            }
        }

        [Test]
        public void BB()
        {
            // 3,4,7,9,12,20,21,22,25
            double[] values = new double[1000];
            Random random = new Random();
            for (int i = 0; i < values.Length; i++)
            {
                int num = random.Next(0, 200);
                double dec = random.NextDouble();

                double val = num + dec;
                values[i] = val;
            }

            double[] copy = new double[values.Length];
            values.CopyTo(copy, 0);

            SortingAlgorithm sortingAlgorithm = new SortingAlgorithm();
            sortingAlgorithm.BubbleSort(values);

            copy = copy.OrderBy(x => x).ToArray(); // trusted computing base

            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(copy[i], values[i]);
            }
        }
    }
}
