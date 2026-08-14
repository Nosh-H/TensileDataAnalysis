/*
 * Created by SharpDevelop.
 * User: e46221
 * Date: 6/18/2007
 * Time: 12:57 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;


namespace DataAnalyzer.Math
{
	/// <summary>
	/// Description of LOESS ANALYSIS.
	/// </summary>
	public class LOESS
	{
		public static void ReDim(ref double[] inX, int length){
			/*(SES) method to redimension an array
			from http://www.dotnetspider.com/namespace/ShowClass.aspx?ClassId=7*/
			double[] inXTemp=new double[length];
			if (length > inX.Length){
				Array.Copy(inX, 0, inXTemp, 0, inX.Length);
				inX = inXTemp;
			}		
			else{
				Array.Copy(inX, 0, inXTemp, 0, length);
				inX = inXTemp;
			}
		}		
		
		public static void ReDimInt(ref int[] inX, int length){
			/*(SES) method to redimension an array
			from http://www.dotnetspider.com/namespace/ShowClass.aspx?ClassId=7*/
			int[] inXTemp=new int[length];
			if (length > inX.Length){
				Array.Copy(inX, 0, inXTemp, 0, inX.Length);
				inX = inXTemp;
			}		
			else{
				Array.Copy(inX, 0, inXTemp, 0, length);
				inX = inXTemp;
			}
		}	
	
		public void LOESSAnalysis(int inPolynomialOrder, double LOESSSpan, 
		                          double[] inX, double[] inY, ref double[] Ybar, 
		                          ref double[] Xbar, ref double[] N, ref double[] Sigma,
		                          ref double[,] Coefficients, ref double[,] SECoefficients){
			/* (SES) This program first calls the SortData program to sort the data. 
			 * Next, the data is broken up into intervals, and a polynomial fit is devised 
			 * of the order defined be inPolynomialOrder for the interval.
			 * The mean is identified, along with the slope at the mean, associated errors,
			 * along with all of the polynomial coefficients and their associated errors.
			 * inX and inY are the data input arrays.  
			 * LOESSSpan is the x width of the interval considered for the LOESS analysis.
			 * There is no cap on the number of points per interval: each interval's temporary
			 * arrays are sized to the number of points that actually fall inside it.*/
			int i = 0;
			int j, k, l, Count;
			int Flag = 0;
			double Xstart, Xend;
			double Rsquared,  residualSumSquared;
			double[] TempX = new double[0];
			double[] TempY = new double[0];
			double [] SEi;
			double [,] Cout;
				
			Array.Sort(inX, inY);
			//Get the data sorted in x ascending order)
			Count = inX.Length;
			Xstart = inX[0];
			Xend = Xstart + LOESSSpan;
			while (Flag == 0){
				k = 0;
				if (i >= Xbar.Length){
					//The caller sized Xbar from the span, so overrunning it means the interval is
					//too small for this data.  MainForm turns this into "Invalid Interval, Please
					//reduce and try again" rather than an out-of-bounds write.
					throw new ArgumentNullException();
				}
				Xbar[i] = (Xend + Xstart)/2;

				//Size the temp arrays to the points that actually fall in this interval
				int intervalCount = 0;
				for (j = 0; j < Count; j++){
					if (inX[j] > Xstart && inX[j] <= Xend){
						intervalCount++;
					}
				}
				// Redimension the temp arrays
				ReDim(ref TempX, intervalCount);
				ReDim(ref TempY, intervalCount);
				for (j = 0; j < Count; j++){
					//Cycles through all x to pick out those within the interval
					if (inX[j] > Xstart && inX[j] <= Xend){
						//assigns the x and y in the interval into temporary arrays, and
						//causes xbar to be zero (apparently, gets rid of error)
						TempX[k] = inX[j]-Xbar[i];
						TempY[k] = inY[j];
						k=k+1;
					}
				}

				Polynomial LOESSPoly = new Polynomial();
				LOESSPoly.PolynomialFit(inPolynomialOrder, TempX, TempY, out Cout, 
				                        out SEi, out Rsquared, out residualSumSquared);
				Ybar[i] = Polynomial.EvaluatePolynomial(0, Cout);
				//Should be the same as a0 because xbar is scaled to be zero
				N[i] = TempX.Length;
				double temp = (residualSumSquared/(N[i]-(inPolynomialOrder+1)));
				Sigma[i] = System.Math.Pow(temp,0.5);
				for (l=0; l < inPolynomialOrder + 1; l++){
					Coefficients[i,l] = Cout[l,0];
					SECoefficients[i,l] = SEi[l];
				}
				i=i+1;
				Xstart = Xstart+LOESSSpan/2;
				Xend = Xstart + LOESSSpan;
				if (Xend >= inX[Count-1]){
					Flag = 1;
					//This cuts off the last incomplete interval
				}
			}
			
			
		}
	}
}
