using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DataAnalyzer.Math;

namespace TestStressStrainData
{
	/// <summary>
	/// Characterization tests pinning Polynomial's behaviour before the fitting and evaluation
	/// routines are optimized.  Covers the exact-fit cases (where the answer is analytically
	/// known), the degenerate-data guard, and the too-few-points guard.
	/// </summary>
	[TestClass]
	public class TestPolynomialEdgeCases
	{
		private const double AcceptablePrecision = 1.0E-9;

		[TestMethod]
		public void TestEvaluatePolynomialUsesAscendingPowers()
		{
			// Coefficients are stored low-order first: 3 + 2x + 5x^2
			double[,] coefficients = new double[,] { { 3.0 }, { 2.0 }, { 5.0 } };

			Assert.AreEqual(3.0, Polynomial.EvaluatePolynomial(0.0, coefficients), AcceptablePrecision);
			Assert.AreEqual(10.0, Polynomial.EvaluatePolynomial(1.0, coefficients), AcceptablePrecision);
			Assert.AreEqual(27.0, Polynomial.EvaluatePolynomial(2.0, coefficients), AcceptablePrecision);
			// Negative x exercises the odd/even power signs
			Assert.AreEqual(6.0, Polynomial.EvaluatePolynomial(-1.0, coefficients), AcceptablePrecision);
		}

		[TestMethod]
		public void TestEvaluateConstantPolynomial()
		{
			double[,] coefficients = new double[,] { { 7.25 } };

			Assert.AreEqual(7.25, Polynomial.EvaluatePolynomial(0.0, coefficients), AcceptablePrecision);
			Assert.AreEqual(7.25, Polynomial.EvaluatePolynomial(123.4, coefficients), AcceptablePrecision);
		}

		[TestMethod]
		public void TestZeroOrderFitReturnsMean()
		{
			Polynomial poly = new Polynomial();
			double[] x = new double[] { 1, 2, 3, 4 };
			double[] y = new double[] { 2, 4, 6, 8 };

			poly.PolynomialFit(0, x, y, out double[,] cout, out double[] sei,
			                   out double rSquared, out double residualSumSquared);

			// A zero-order fit is just the mean of y
			Assert.AreEqual(5.0, cout[0, 0], AcceptablePrecision);
			Assert.AreEqual(1, cout.GetLength(0));
		}

		[TestMethod]
		public void TestExactLineFitHasUnitRSquared()
		{
			Polynomial poly = new Polynomial();
			// Exactly y = 1 + 2x, so the fit must reproduce it with no residual
			double[] x = new double[] { 0, 1, 2, 3, 4 };
			double[] y = new double[] { 1, 3, 5, 7, 9 };

			poly.PolynomialFit(1, x, y, out double[,] cout, out double[] sei,
			                   out double rSquared, out double residualSumSquared);

			Assert.AreEqual(1.0, cout[0, 0], AcceptablePrecision);
			Assert.AreEqual(2.0, cout[1, 0], AcceptablePrecision);
			Assert.AreEqual(1.0, rSquared, AcceptablePrecision);
			Assert.AreEqual(0.0, residualSumSquared, AcceptablePrecision);
		}

		[TestMethod]
		public void TestExactQuadraticFit()
		{
			Polynomial poly = new Polynomial();
			// y = 5 - 3x + 2x^2
			double[] x = new double[] { -2, -1, 0, 1, 2, 3 };
			double[] y = new double[6];
			for (int n = 0; n < x.Length; n++)
			{
				y[n] = 5.0 - 3.0 * x[n] + 2.0 * x[n] * x[n];
			}

			poly.PolynomialFit(2, x, y, out double[,] cout, out double[] sei,
			                   out double rSquared, out double residualSumSquared);

			Assert.AreEqual(5.0, cout[0, 0], AcceptablePrecision);
			Assert.AreEqual(-3.0, cout[1, 0], AcceptablePrecision);
			Assert.AreEqual(2.0, cout[2, 0], AcceptablePrecision);
			Assert.AreEqual(1.0, rSquared, AcceptablePrecision);
		}

		/// <summary>
		/// When every y is identical the total sum of squares is zero, which would divide by zero
		/// in the R^2 calculation.  Polynomial guards this and reports a perfect fit instead.
		/// </summary>
		[TestMethod]
		public void TestConstantDataReportsPerfectRSquared()
		{
			Polynomial poly = new Polynomial();
			double[] x = new double[] { 1, 2, 3, 4, 5 };
			double[] y = new double[] { 4, 4, 4, 4, 4 };

			poly.PolynomialFit(1, x, y, out double[,] cout, out double[] sei,
			                   out double rSquared, out double residualSumSquared);

			Assert.AreEqual(1.0, rSquared, AcceptablePrecision);
			Assert.AreEqual(4.0, cout[0, 0], AcceptablePrecision);
			Assert.AreEqual(0.0, cout[1, 0], AcceptablePrecision);
		}

		/// <summary>
		/// Fitting an order-n polynomial needs more than n points.  Fewer throws
		/// AccessViolationException, which MainForm surfaces as the "reduce polynomial order or
		/// increase interval size" message -- so the exception type is load-bearing.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(AccessViolationException))]
		public void TestTooFewPointsThrows()
		{
			Polynomial poly = new Polynomial();
			double[] x = new double[] { 1, 2 };
			double[] y = new double[] { 3, 4 };

			poly.PolynomialFit(2, x, y, out double[,] cout, out double[] sei,
			                   out double rSquared, out double residualSumSquared);
		}
	}
}
