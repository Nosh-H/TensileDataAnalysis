using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataAnalyzer.Math;

namespace TestStressStrainData
{
	/// <summary>
	/// Combination pools every specimen's zeroed data into one array and refits a single LOESS +
	/// global polynomial curve across all of it -- this is what Analyze exposes as Total/NU.
	/// Currently untested anywhere in the suite.
	/// </summary>
	[TestClass]
	public class TestCombination
	{
		private const double AcceptablePrecision = 1.0E-9;

		[TestMethod]
		public void TestPerfectLinearDataRecoversSlopeAndInterceptExactly()
		{
			// Pooled data follows the ZeroedData convention: column 0 = y, column 1 = x.
			// y = 3x through the origin, sampled at every integer strain 0..12 -- structurally the
			// same shape Analyze.cs feeds Combination after pooling every specimen's Zeroer output.
			int n = 13;
			double[,] combinedData = new double[n, 2];
			for (int i = 0; i < n; i++)
			{
				combinedData[i, 1] = i;       // x
				combinedData[i, 0] = 3.0 * i; // y
			}

			Combination combination = new Combination(combinedData, locPolyOrder: 1, globPolyOrder: 1, interval: 3);

			// Data is perfectly linear through the origin, so every local LOESS fit, the pooled
			// mean points, and the global fit all reproduce y = 3x exactly -- including the (0,0)
			// point Combination inserts before the global fit (Combination.cs:73-76), which
			// already lies on the line.  R^2 should be exactly 1.0, not just close to it.
			Assert.AreEqual(0.0, combination.FinalCoefficients[0], AcceptablePrecision, "intercept");
			Assert.AreEqual(3.0, combination.FinalCoefficients[1], AcceptablePrecision, "slope");
			Assert.AreEqual(1.0, combination.RSquared, AcceptablePrecision);

			// Secant slope (Ybar/Xbar) at every real mean point should also recover the line's
			// slope.  Skip any point sitting at x=0 -- MeanData is filled (Combination.cs:62-68)
			// BEFORE the (0,0) anchor is appended for the global fit, so it never contains that
			// anchor; the x=0 case here is the trailing all-zero slot LOESS leaves when its last
			// interval gets cut off (see TestLOESS), where Ybar/Xbar is 0/0.
			for (int i = 0; i < combination.SecantSlope.Length; i++)
			{
				if (combination.MeanData[i, 1] != 0.0)
				{
					Assert.AreEqual(3.0, combination.SecantSlope[i], AcceptablePrecision,
					                $"secant slope at mean point {i}");
				}
			}
		}

		[TestMethod]
		public void TestMeanDataRowCountMatchesLOESSIntervalsOnly()
		{
			int n = 13;
			double[,] combinedData = new double[n, 2];
			for (int i = 0; i < n; i++)
			{
				combinedData[i, 1] = i;
				combinedData[i, 0] = 3.0 * i;
			}

			Combination combination = new Combination(combinedData, locPolyOrder: 1, globPolyOrder: 1, interval: 3);

			// Same interval/span as TestLOESS's TestLOESSAnalysis (13 points, span 3): 7 LOESS
			// intervals.  MeanData is filled straight from the LOESS output BEFORE the (0,0)
			// anchor point is appended for the global fit (Combination.cs:62-76), so that anchor
			// does not show up here -- only Total/NU's FinalCout, not MeanData, "sees" it.
			Assert.AreEqual(7, combination.MeanData.GetLength(0));

			// The last LOESS interval is cut off before being filled (see TestLOESS's
			// TestLOESSAnalysis, which pins the identical trailing-zero-slot behaviour), so the
			// last row here is left at its default (0,0) rather than a real mean point.
			Assert.AreEqual(0.0, combination.MeanData[6, 1], AcceptablePrecision);
			Assert.AreEqual(0.0, combination.MeanData[6, 0], AcceptablePrecision);
		}
	}
}
