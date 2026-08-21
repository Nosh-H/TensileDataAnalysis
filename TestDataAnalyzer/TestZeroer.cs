using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DataAnalyzer.Math;

namespace TestStressStrainData
{
	/// <summary>
	/// Zeroer is the highest-value class to pin: a silent indexing bug here would corrupt every
	/// downstream stress-strain result.  These tests use synthetic data chosen so the expected
	/// offset/cutoff/zeroed values can be computed by hand and checked against the algorithm's
	/// actual output.
	/// </summary>
	[TestClass]
    public class TestZeroer
    {
        // public Zeroer(double [,] inData, int locPolyOrder, int globPolyOrder, double interval,
        //  double cutoff, int x, int y, double [] offsetArray)
        // x/y here are COLUMN INDICES into inData, not values -- Analyze.cs calls with x=1
        // (strain) and y=0 (stress).

        [TestMethod]
        public void TestCutoffStrain()
        {
            // CutoffStrain (private, exercised via the constructor) should drop every row past
            // cutoff.  Strain running 0..10 with cutoff=6 means rows with strain 0..6 survive:
            // 7 rows.  (Verified against the actual CutoffStrain logic: it advances past index i
            // where inX[i] first exceeds cutoff, and since strain==index here, that's index 7 --
            // then it truncates back to indices 0..6, i.e. length 7.)
            double[,] inputData = new double[11, 2];
            for (int i = 0; i < 11; i++)
            {
                inputData[i, 1] = i;         // strain
                inputData[i, 0] = 4 * i + 5; // stress = 4*strain + 5, perfectly linear
            }

            // flag=0 skips the yield-offset search (Offset.cs:118-121); rMin/minPtsForFit don't
            // matter here because the data is perfectly linear end to end, so R^2 is exactly 1.0
            // for every subset regardless of threshold -- the fit always extends through all of it.
            double[] offsetArray = new double[4];

            Zeroer zeroer = new Zeroer(inputData, 1, 1, 3, 6, 1, 0, offsetArray);

            Assert.AreEqual(7, zeroer.ZeroedData.GetLength(0));
        }

        [TestMethod]
        public void TestOffset()
        {
            // Build data that is exactly linear (stress = 2*strain) for strain 0..10, then curves
            // sharply away for strain 11..15.  A perfectly linear region back-extrapolates to
            // true zero, so the strain offset from JUST that region would be exactly 0.
            double[,] inputData = new double[16, 2];
            for (int i = 0; i <= 10; i++)
            {
                inputData[i, 1] = i;
                inputData[i, 0] = 2 * i;
            }
            for (int i = 11; i < 16; i++)
            {
                inputData[i, 1] = i;
                // Deliberately large curvature so R^2 drops sharply and unambiguously once the
                // curved points enter the fit -- avoids a threshold choice that's borderline.
                inputData[i, 0] = 2 * i + 5 * Math.Pow(i - 10, 2);
            }

            // rMin=0.999: keep extending the linear fit only while it's (nearly) perfect.
            // minPtsForFit=2: need at least two points to fit a line at all.
            double[] offsetArray = new double[] { 0, 0, 0.999, 2 };

            Zeroer zeroer = new Zeroer(inputData, 1, 1, 4, 16, 1, 0, offsetArray);

            // Offset.cs's search fits through points 0..i, checking R^2 BEFORE trying the next
            // extension -- so by the time it decides to stop, it has already accepted one point
            // past the truly linear region.  Points 0..10 are exact (R^2=1.0); adding point 11
            // (the first curved one) gives R^2 ~= 0.975 for that 12-point fit, which is below
            // 0.999, so the loop stops there and that 12-point fit is what's kept.
            //
            // Hand-computed least-squares line through points (0,0)...(10,20),(11,27):
            //   slope = 57/26, intercept = -25/39, so strainOffset = -intercept/slope = 50/171.
            const double ExpectedOffset = 50.0 / 171.0; // ~0.2923976608
            Assert.AreEqual(ExpectedOffset, zeroer.OffsetData.StrainOffset, 1e-6);
        }

        [TestMethod]
        public void TestZeroedData()
        {
            // WHY spot-check individual (x,y) pairs, not just counts?
            // Verifying row COUNT (as TestCutoffStrain does) only proves the right number of rows
            // survived -- it says nothing about whether each row's x and y are still the correct
            // pair.  A bug that swaps columns, misaligns indices during a sort, or pulls x from
            // one array and y from a different one (exactly the PlotMaker16 bug fixed elsewhere
            // in this cleanup, where RawDataForFit's x was paired with a DIFFERENT array's y)
            // would still produce an array of the right SHAPE while every point is wrong.
            // Spot-checking rows against hand-computed values is the only way to catch that.

            double[,] inputData = new double[11, 2];
            for (int i = 0; i < 11; i++)
            {
                inputData[i, 1] = i;
                inputData[i, 0] = 4 * i + 5;
            }

            double[] offsetArray = new double[4]; // see TestCutoffStrain for why this is safe here
            Zeroer zeroer = new Zeroer(inputData, 1, 1, 3, 6, 1, 0, offsetArray);

            // Because this data is perfectly linear, Offset fits a line through every surviving
            // point: intercept=5, slope=4, so strainOffset = -5/4 = -1.25.  Zeroer then shifts
            // every strain value by SUBTRACTING that offset (x_new = x_old - (-1.25) = x_old +
            // 1.25), while stress passes through unchanged.  Row order survives the shift because
            // adding a constant doesn't change ascending order, and CutoffStrain already sorted
            // ascending by strain -- so row j corresponds directly to original index j.
            const double Tolerance = 1e-9;

            // Row 0: original strain 0, stress 5
            Assert.AreEqual(5.0, zeroer.ZeroedData[0, 0], Tolerance, "stress at row 0");
            Assert.AreEqual(1.25, zeroer.ZeroedData[0, 1], Tolerance, "shifted strain at row 0");

            // Row 6: original strain 6, stress 29 -- the last row surviving the cutoff at 6
            Assert.AreEqual(29.0, zeroer.ZeroedData[6, 0], Tolerance, "stress at row 6");
            Assert.AreEqual(7.25, zeroer.ZeroedData[6, 1], Tolerance, "shifted strain at row 6");
        }
    }
}
