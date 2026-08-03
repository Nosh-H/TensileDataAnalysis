/*
 * Created by SharpDevelop.
 * User: e46221
 * Date: 7/11/2007
 * Time: 11:23 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using DataAnalyzer;

namespace DataAnalyzer.Math
{
	
	public class Analyze
	{
		private string [] root;
		private int [] tranChan, axChan, rowStart, rowEnd, chanStrain, chanStress, dispflag;
		private double [] length, xSecArea;
		private double cutoff, interval;
		private string temperature;
		private int locPolyOrder, globPolyOrder, numberofFiles, numberofTranFiles;
		private FileReader[] rawData;
		private Averager[] aveData;
		private Zeroer[] zeroData;
		private Zeroer[] zeroNUData;
		private double [,] combinedData;
		private double [,] combinedNUData;
		private Combination total;
		private Combination nu;
		
		/// <summary>
		/// takes in an inputArray where each column is the input data for each file scanned,
		/// with the 1st row being the root, 2nd the # of axial strain channels, 3rd the #
		/// of transverse strain channels, 4th the specimen length, and 5th the x-sec area.
		/// The last column is for the group inputs, with the 1st row being the cutoff strain,
		/// 2nd the polynomial order, 3rd the LOESS interval, 4th the temperature, and 5th the
		/// # of files scanned in.  Also, takes in the offest array: 1st element is 0 if not an option
		/// </summary>
		/// <param name="individualInputsList"></param>
		/// <param name="groupInputsList"></param>
		/// <param name="offsetArray"></param>
		public Analyze(List<string[]> individualInputsList, string[] groupInputsList, double [] offsetArray){

			// numberofFiles = how many specimens were loaded; numberofTranFiles = how many of those
			// also have transverse strain gauges (needed to compute Poisson's ratio, "NU").
			numberofFiles = Convert.ToInt32(groupInputsList[5]);
			numberofTranFiles = Convert.ToInt32(groupInputsList[6]);

			int i,j, k, sum;
			// One slot per specimen for each stage of the pipeline: raw CSV data -> channel-averaged
			// data -> zeroed/cut/LOESS-smoothed data. zeroNUData is the transverse-strain equivalent
			// of zeroData, used only for specimens that have transverse gauges.
			rawData = new FileReader[numberofFiles];
			aveData = new Averager[numberofFiles];
			zeroData = new Zeroer[numberofFiles];
			zeroNUData = new Zeroer[numberofTranFiles];
			root = new string [numberofFiles];
			axChan = new int [numberofFiles];
			tranChan = new int [numberofFiles];
			length = new double [numberofFiles];
			xSecArea = new double [numberofFiles];
			rowStart = new int [numberofFiles];
			rowEnd = new int [numberofFiles];
			chanStrain = new int [numberofFiles];
			chanStress = new int [numberofFiles];
			dispflag = new int [numberofFiles];

			//split up the input array into seperate arrays and variables

			// Group-level settings that apply to every specimen in this run (set once from the
			// "group inputs" the user entered, not per-file).
			cutoff = Convert.ToDouble(groupInputsList[0]);       // strain value past which data is discarded
			locPolyOrder = Convert.ToInt32(groupInputsList[1]);  // polynomial order used for each local LOESS fit
			globPolyOrder = Convert.ToInt32(groupInputsList[2]); // polynomial order used for the single fit across all pooled data
			interval = Convert.ToDouble(groupInputsList[3]);     // LOESS window width (span) along the x-axis
			temperature = groupInputsList[4].ToString();

			// Unpack each specimen's individual settings (file path/root, channel counts, gauge
			// geometry, which CSV rows/columns to read from, and whether axial data is raw
			// displacement vs. already-computed strain).
			for (i = 0; i < (numberofFiles); i++){
				root[i] = individualInputsList[i][0];
				axChan[i] = Convert.ToInt32(individualInputsList[i][1]);
				tranChan[i] = Convert.ToInt32(individualInputsList[i][2]);
				length[i] = Convert.ToDouble(individualInputsList[i][3]);
				xSecArea[i] = Convert.ToDouble(individualInputsList[i][4]);
				rowStart[i] = Convert.ToInt32(individualInputsList[i][5]);
				rowEnd[i] = Convert.ToInt32(individualInputsList[i][6]);
				chanStrain[i] = Convert.ToInt32(individualInputsList[i][7]);
				chanStress[i] = Convert.ToInt32(individualInputsList[i][8]);
				dispflag[i] = Convert.ToInt32(individualInputsList[i][9]);
			}


			// Per-specimen pipeline: for each loaded file, read + convert the raw CSV (FileReader),
			// average multiple axial/transverse channels down to one column each (Averager), then
			// zero, cut off at `cutoff` strain, and LOESS-smooth the stress/strain curve (Zeroer,
			// x=column 1 strain, y=column 0 stress). `sum` tallies the total number of surviving
			// (zeroed) data points across all specimens so combinedData can be sized correctly below.
			sum = 0;
			for (i = 0; i < (numberofFiles); i++){
				//Average all of the channels if there are more than one
				rawData[i] =new FileReader(root[i], axChan[i], tranChan[i], length[i], xSecArea[i],
				                           rowStart[i], rowEnd[i], chanStrain[i], chanStress[i], dispflag[i]);
				aveData[i] =new Averager(rawData[i].RawData, rawData[i].AxChan, rawData[i].TranChan);
				//Now find the offset to zero the data
				//offsetData = new Offset(total,offsetArray);
				zeroData[i] = new Zeroer(aveData[i].AveragedData, locPolyOrder, globPolyOrder, interval, cutoff, 1, 0, offsetArray);
				sum = sum + zeroData[i].ZeroedData.GetUpperBound(0)+1;
			}

			// Pool every specimen's zeroed stress/strain points into one big array (combinedData:
			// column 0 = strain, column 1 = stress), so they can be fit as a single combined curve
			// rather than specimen-by-specimen.
			j = 0;
			combinedData = new double [sum, 2];
			for (i = 0; i < (numberofFiles); i++){
				for (k = 0; k < (zeroData[i].ZeroedData.GetUpperBound(0)+1); k++){
					combinedData[j,0] = zeroData[i].ZeroedData[k,0];
					combinedData[j,1] = zeroData[i].ZeroedData[k,1];
					j++;
				}
			}

			// `total` re-runs LOESS on the pooled data and fits one global polynomial through all
			// specimens combined, giving the material's overall stress-strain response (mean curve,
			// secant/tangent modulus, R^2, etc.). This is the object exposed as Analyze.Total below;
			// FileWriter5 writes it out to "TotalLoessDataE.csv", ResultsWindow prints its fit
			// coefficients/R^2 as the summary modulus results, and PlotMaker reads its MeanData/
			// Coefficients/FinalCout to draw the pooled stress-vs-strain and modulus-vs-strain plots.
			total = new Combination(combinedData, locPolyOrder, globPolyOrder, interval);

			//now do the same thing for transverse strain data (to find NU)
			if (numberofTranFiles != 0){
				// Same idea as above, but using transverse strain (x) vs. axial strain (y) instead of
				// stress vs. strain, only for specimens with transverse gauges (TranChan != 0). The
				// resulting fit slope is Poisson's ratio (NU) rather than a modulus.
				sum = 0;
				for (i = 0; i < (numberofFiles); i++){
					if (rawData[i].TranChan != 0){
						zeroNUData[i] = new Zeroer(aveData[i].AveragedData, locPolyOrder, globPolyOrder, interval, cutoff, 1, 2, offsetArray);
						sum = sum + zeroNUData[i].ZeroedData.GetUpperBound(0)+1;
					}
				}

				j = 0;
				//with combinedNUData, first comes transverse [0], then axial [1]
				combinedNUData = new double [sum, 2];
				for (i = 0; i < (numberofTranFiles); i++){
					for (k = 0; k < (zeroNUData[i].ZeroedData.GetUpperBound(0)+1); k++){
						combinedNUData[j,0] = zeroNUData[i].ZeroedData[k,0];
						combinedNUData[j,1] = zeroNUData[i].ZeroedData[k,1];
						j++;
					}
				}
				// `nu` is the transverse/axial-strain counterpart of `total`: same pooled LOESS +
				// global-polynomial fit, but its slope represents Poisson's ratio. Exposed as
				// Analyze.NU; FileWriter6 writes it to "TotalLoessDataNU.csv", ResultsWindow reports
				// its coefficients as the NU summary, and PlotMaker uses its MeanData/Coefficients to
				// draw the transverse-vs-axial-strain plots.
				nu = new Combination(combinedNUData, locPolyOrder, globPolyOrder, interval);

			}
			//Now find the offset if it was checked in the input window
			//offsetData = new Offset(total,offsetArray);
			
		}
		
		//Accessors (so that I can get to them from MainForm
		public FileReader[] RawData{
			get{return rawData;}
		}
		public Averager[] AveData{
			get{return aveData;}
		}
		public Zeroer[] ZeroData{
			get{return zeroData;}
		}
		public double [,] CombinedData{
			get{return combinedData;}
		}
		public Combination Total{
			get{return total;}
		}
		public Combination NU{
			get{return nu;}
		}
		public double [,] CombinedNUData{
			get{return combinedNUData;}
		}
		public Zeroer[] ZeroNUData{
			get{return zeroNUData;}
		}
		public double Cutoff{
			get{return cutoff;}
		}
		public double Interval{
			get{return interval;}
		}
		public double GlobPolyOrder{
			get{return globPolyOrder;}
		}
		public double LocPolyOrder{
			get{return locPolyOrder;}
		}
		public int NumberofTranFiles{
			get{return numberofTranFiles;}
		}
	}
}
