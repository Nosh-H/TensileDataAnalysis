using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataAnalyzer.Math;

namespace TestStressStrainData
{
	/// <summary>
	/// Offset is normally exercised indirectly through Zeroer (see TestZeroer's TestOffset, which
	/// covers the curved-data case).  This class constructs Offset directly to pin its simplest,
	/// least ambiguous behaviour: a strain offset computed from perfectly linear data.
	///
	/// The yield-stress search (offsetArray[0] == 1) is NOT covered here.  It turned out to be
	/// hard to pin with a small, clean synthetic dataset: Offset's linear-region search always
	/// accepts one point past the last truly linear one before it stops extending the fit (see
	/// the comment in TestZeroer's TestOffset), which skews the kept fit's intercept away from
	/// zero whenever the "yielded" region doesn't extrapolate back through the origin -- and that
	/// skew then interacts with the "fit starts above the data" guard at j=0 (Offset.cs:135-139)
	/// in a way that's sensitive to exactly how much data precedes the yield point.  A small,
	/// exact test for that path needs a more careful dataset than this pass had time to construct.
	/// </summary>
	[TestClass]
	public class TestOffset
	{
		private const double AcceptablePrecision = 1.0E-9;

		[TestMethod]
		public void TestExactlyLinearDataHasZeroOffset()
		{
			// y = 3x through the origin: back-extrapolating a perfectly linear region to zero
			// load should return exactly to zero strain.
			double[] x = new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			double[] y = new double[x.Length];
			for (int i = 0; i < x.Length; i++)
			{
				y[i] = 3.0 * x[i];
			}

			// flag=0 skips the yield-stress search; rMin/minPtsForFit don't matter here since
			// R^2 is exactly 1.0 for every subset of perfectly linear data.
			double[] offsetArray = new double[] { 0, 0, 0, 0 };

			Offset offset = new Offset(x, y, offsetArray);

			Assert.AreEqual(0.0, offset.StrainOffset, AcceptablePrecision);
			Assert.AreEqual(0.0, offset.COut_Linear_PreZero[0, 0], AcceptablePrecision, "intercept");
			Assert.AreEqual(3.0, offset.COut_Linear_PreZero[1, 0], AcceptablePrecision, "slope");
		}

		[TestMethod]
		public void TestOffsetShiftsInterceptToZero()
		{
			// y = 4x + 5: a line NOT through the origin.  strainOffset should be -intercept/slope
			// = -5/4, and Cout_Linear (the POST-adjustment coefficients Offset.cs:113 computes)
			// should evaluate to zero intercept at that offset -- i.e. shifting the x-axis by
			// strainOffset re-zeroes the line, which is the whole point of computing it.
			double[] x = new double[] { 0, 1, 2, 3, 4, 5 };
			double[] y = new double[x.Length];
			for (int i = 0; i < x.Length; i++)
			{
				y[i] = 4.0 * x[i] + 5.0;
			}

			double[] offsetArray = new double[] { 0, 0, 0, 0 };
			Offset offset = new Offset(x, y, offsetArray);

			Assert.AreEqual(-1.25, offset.StrainOffset, AcceptablePrecision);
			// Cout_Linear[0,0] += Cout_Linear[1,0] * strainOffset
			//   = intercept + slope*(-intercept/slope) = 0, algebraically, for any line
			Assert.AreEqual(0.0, offset.COut_Linear[0, 0], AcceptablePrecision);
		}
	}
}
