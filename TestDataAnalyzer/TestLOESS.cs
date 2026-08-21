/*
 * Created by SharpDevelop.
 * User: e46221
 * Date: 6/18/2007
 * Time: 12:58 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DataAnalyzer.Math;

namespace TestStressStrainData
{
	[TestClass]
	public class TestLOESS
	{
		
		[TestMethod]
		public void TestLOESSAnalysis()
		{

			double [] inX = new double[] {1,2,3,4,5,6,7,8,9,10,11,12,13};
    		double [] inY = new double[] {1,4,6,7,8,9,9,8.5,8,7,6,4,2};
    		double acceptablePrecision = 1.0E-9;
    		int inPolynomialOrder = 1;
    		double loessSpan = 3;
    		double dNumberofIntervals = (Math.Floor(2*(inX[inX.Length-1] - inX[0])/(loessSpan))) - 1;
    		int numberofIntervals = System.Convert.ToInt16(dNumberofIntervals);
    		double [] Ybar = new double [numberofIntervals];
    		double [] Sigma = new double [numberofIntervals];
    		double [] Xbar = new double [numberofIntervals];
    		double [] N = new double [numberofIntervals];
    		double [,] coefficients = new double [numberofIntervals, inPolynomialOrder+1];
    		double [,] seCoefficients = new double [numberofIntervals, inPolynomialOrder+1];
    		
    		LOESS myLOESS = new LOESS();
			myLOESS.LOESSAnalysis(inPolynomialOrder, loessSpan, inX, inY, ref Ybar,
    		                      ref Xbar, ref N, ref Sigma, ref coefficients, ref seCoefficients);
			
			Assert.AreEqual(4.916666666667, Ybar[0] ,acceptablePrecision);
			Assert.AreEqual(8.833333333333333, Ybar[3] ,acceptablePrecision);

			// Interval centres: the window starts at inX[0] and advances by half a span each step,
			// so Xbar[i] = (Xstart + Xend)/2 with Xstart = 1 + 1.5i and Xend = Xstart + 3.
			Assert.AreEqual(7, numberofIntervals, "floor(2*(13-1)/3) - 1 intervals");
			Assert.AreEqual(2.5, Xbar[0], acceptablePrecision);
			Assert.AreEqual(4.0, Xbar[1], acceptablePrecision);
			Assert.AreEqual(7.0, Xbar[3], acceptablePrecision);
			Assert.AreEqual(10.0, Xbar[5], acceptablePrecision);

			// The loop stops once the window reaches the last x, cutting the final incomplete
			// interval -- so the last slot the caller allocated is left at zero.  Downstream
			// (Zeroer, Combination) sizes MeanData from Xbar.Length, so that trailing (0,0) row
			// is carried through into the mean data.  Pinned here as existing behaviour.
			Assert.AreEqual(0.0, Xbar[6], acceptablePrecision);
			Assert.AreEqual(0.0, Ybar[6], acceptablePrecision);

			// Each interval of this evenly-spaced data captures exactly 3 points
			Assert.AreEqual(3.0, N[0], acceptablePrecision);
			Assert.AreEqual(3.0, N[3], acceptablePrecision);

			// Local linear fit over interval 0, which holds (2,4), (3,6), (4,7) recentred on
			// Xbar[0]: intercept is Ybar[0] and the slope is the local tangent modulus.
			Assert.AreEqual(4.916666666667, coefficients[0, 0], acceptablePrecision);
			Assert.AreEqual(1.5, coefficients[0, 1], acceptablePrecision);

			// Interval 3 holds (6,9), (7,9), (8,8.5), centred so the mean x is exactly zero
			Assert.AreEqual(8.833333333333333, coefficients[3, 0], acceptablePrecision);
			Assert.AreEqual(-0.25, coefficients[3, 1], acceptablePrecision);

			// Sigma is sqrt(residual sum of squares / (n - (order+1))), which for interval 0 is
			// sqrt(0.1666.../1)
			Assert.AreEqual(0.408248290463863, Sigma[0], acceptablePrecision);
		}

		/// <summary>
		/// An interval far too small for the data would overrun the caller-sized Xbar array.
		/// LOESS throws ArgumentNullException instead, which MainForm reports as
		/// "Invalid Interval, Please reduce and try again".
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestUndersizedOutputArraysThrow()
		{
			double [] inX = new double[] {1,2,3,4,5,6,7,8,9,10,11,12,13};
			double [] inY = new double[] {1,4,6,7,8,9,9,8.5,8,7,6,4,2};
			int inPolynomialOrder = 1;
			double loessSpan = 3;

			// Deliberately size the outputs for a single interval when the data needs seven
			double [] Ybar = new double [1];
			double [] Sigma = new double [1];
			double [] Xbar = new double [1];
			double [] N = new double [1];
			double [,] coefficients = new double [1, inPolynomialOrder+1];
			double [,] seCoefficients = new double [1, inPolynomialOrder+1];

			LOESS myLOESS = new LOESS();
			myLOESS.LOESSAnalysis(inPolynomialOrder, loessSpan, inX, inY, ref Ybar,
			                      ref Xbar, ref N, ref Sigma, ref coefficients, ref seCoefficients);
		}
	}
}
