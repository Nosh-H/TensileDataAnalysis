using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataAnalyzer.Math;

namespace TestStressStrainData
{
	/// <summary>
	/// Averager collapses multiple axial/transverse strain channels down to one column each.
	/// Its input layout is FileReader's: column 0 stress, then axChan axial strain columns,
	/// then tranChan transverse strain columns.
	/// </summary>
	[TestClass]
	public class TestAverager
	{
		private const double AcceptablePrecision = 1.0E-9;

		[TestMethod]
		public void TestAveragesMultipleAxialChannels()
		{
			// stress, ax1, ax2
			double[,] input = new double[,] {
				{ 10.0, 100.0, 200.0 },
				{ 20.0, 300.0, 500.0 }
			};

			Averager averager = new Averager(input, inAxChan: 2, inTranChan: 0);

			// No transverse channels, so the output is two columns: stress, averaged axial strain
			Assert.AreEqual(2, averager.AveragedData.GetLength(1));
			Assert.AreEqual(10.0, averager.AveragedData[0, 0], AcceptablePrecision);
			Assert.AreEqual(150.0, averager.AveragedData[0, 1], AcceptablePrecision);
			Assert.AreEqual(20.0, averager.AveragedData[1, 0], AcceptablePrecision);
			Assert.AreEqual(400.0, averager.AveragedData[1, 1], AcceptablePrecision);
		}

		[TestMethod]
		public void TestAveragesAxialAndTransverseChannels()
		{
			// stress, ax1, ax2, tran1, tran2
			double[,] input = new double[,] {
				{ 10.0, 100.0, 200.0, 40.0, 60.0 },
				{ 20.0, 300.0, 500.0, 70.0, 90.0 }
			};

			Averager averager = new Averager(input, inAxChan: 2, inTranChan: 2);

			// Transverse channels present, so a third column is added
			Assert.AreEqual(3, averager.AveragedData.GetLength(1));
			Assert.AreEqual(150.0, averager.AveragedData[0, 1], AcceptablePrecision);
			Assert.AreEqual(50.0, averager.AveragedData[0, 2], AcceptablePrecision);
			Assert.AreEqual(400.0, averager.AveragedData[1, 1], AcceptablePrecision);
			Assert.AreEqual(80.0, averager.AveragedData[1, 2], AcceptablePrecision);
		}

		[TestMethod]
		public void TestSingleChannelPassesValuesThrough()
		{
			double[,] input = new double[,] {
				{ 1.5, 2.5 },
				{ 3.5, 4.5 }
			};

			Averager averager = new Averager(input, inAxChan: 1, inTranChan: 0);

			Assert.AreEqual(1.5, averager.AveragedData[0, 0], AcceptablePrecision);
			Assert.AreEqual(2.5, averager.AveragedData[0, 1], AcceptablePrecision);
			Assert.AreEqual(3.5, averager.AveragedData[1, 0], AcceptablePrecision);
			Assert.AreEqual(4.5, averager.AveragedData[1, 1], AcceptablePrecision);
		}
	}
}
