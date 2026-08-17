/*
 * Created by SharpDevelop.
 * User: e46221
 * Date: 7/10/2007
 * Time: 8:05 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Xml;
using System.IO;
using ZedGraph;
using DataAnalyzer.Math;
using DataAnalyzer.Plots;

namespace DataAnalyzer
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form 
	{
		private int dispflag = 0;
		private int numberofFiles = 0;
		private int numberofTranFiles = 0;
		private List<string[]> individualInputsList= new List<string[]>();
		private string[] groupInputsList;
		private Analyze analyze;
		//private string [,] inputArray;
		private int fileNumber = 0;
		private string material;
		private string folder;
		private string temperature;
				
		[STAThread]
		public static void Main(string[] args){
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
		public MainForm(){
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			//
			// start out with an open dialog box.
			//
		}
		//individual file inputs Group
		void BrowseMouseClick(object sender, MouseEventArgs e){
			if (!TryPickSpecimenFile())
			{
				return;
			}

			inputGroupBox.Visible = true;
			inputGroupBox.Enabled = true;
			previewBttn.Enabled = true;
			if (numberofFiles >= 1)
				doneBttn.Enabled = true;
			addBttn.Enabled = false;
		}

		/// <summary>
		/// Opens a file picker for a specimen CSV, resolves the output folder and the "root" path
		/// (full path with the .csv extension stripped, since FileReader appends it back) using
		/// System.IO.Path rather than searching the path text for a literal "v" or ".", and
		/// auto-populates the row-start/row-end fields from the file's actual data extent.
		/// Shared by BrowseMouseClick (first specimen) and AddBttnClick (every specimen after),
		/// so both behave identically instead of silently diverging.
		/// </summary>
		/// <returns>true if a file was picked and parsed successfully; false on Cancel or error
		/// (an error is already shown to the user in the false case)</returns>
		private bool TryPickSpecimenFile()
		{
			openFileTxt.Text = "";
			OpenFileDialog fdlg = new OpenFileDialog();
			fdlg.Title = "Select File To Read From";
			fdlg.Filter = "CSV Files (*.csv*)|*.csv*";
			fdlg.FilterIndex = 2;
			fdlg.RestoreDirectory = true;
			if (fdlg.ShowDialog() != DialogResult.OK)
			{
				return false;
			}

			try
			{
				string directory = Path.GetDirectoryName(fdlg.FileName);
				// folder keeps a trailing separator: FileWriter concatenates it directly with a
				// file name (e.g. folder + "TotalLoessDataE.csv")
				folder = directory + "\\";
				// "root": the full path with the .csv extension stripped -- FileReader appends
				// ".csv" back on when it opens the file
				openFileTxt.Text = Path.Combine(directory, Path.GetFileNameWithoutExtension(fdlg.FileName));

				FindMinAndMax(openFileTxt.Text, 0, 1, out int firstIndex, out int lastIndex);
				rowEndTxtBox.Text = Convert.ToString(lastIndex);
				rowStartTxtBox.Text = Convert.ToString(firstIndex);
			}
			catch
			{
				MessageBox.Show("Error: Try Again!");
				inputGroupBox.Enabled = false;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Validates the six per-specimen fields (area, length, row range, strain/stress columns)
		/// that PreviewBttnClick and AddCurrentBttnClick both require before proceeding. Shared so
		/// the field list and the error message only need to be kept in sync in one place.
		/// </summary>
		private bool TryValidateSpecimenInputs()
		{
			try
			{
				Convert.ToDouble(xSecAreaTxtBox.Text);
				Convert.ToDouble(lengthTxtBox.Text);
				Convert.ToInt32(rowStartTxtBox.Text);
				Convert.ToInt32(rowEndTxtBox.Text);
				Convert.ToInt32(tbStrainCol.Text);
				Convert.ToInt32(tbStressCol.Text);
			}
			catch
			{
				MessageBox.Show("Invalid User Input:  Verify Values Are Correct");
				return false;
			}
			return true;
		}
		void TransCheckBox1CheckedChanged(object sender, EventArgs e){
			tranChanGroupBox.Visible = true;
		}
		void StressRadioBttn1CheckedChanged(object sender, EventArgs e){
			areaGroupBox.Visible = false;
			xSecAreaTxtBox.Text = "1";
		}
		void LoadRadioBttnCheckedChanged(object sender, EventArgs e){
			areaGroupBox.Visible = true;
			lengthTxtBox.Text = "1.1";			
		}
		void StrainRadioBttnCheckedChanged(object sender, EventArgs e){
			lengthGroupBox.Visible = false;
			lengthTxtBox.Text = "1";
			axChanGroupBox.Visible = true;
			dispflag = 0;
		}
		void DispRadioBttnCheckedChanged(object sender, EventArgs e){
			lengthGroupBox.Visible = true;
			axChanGroupBox.Visible = false;
			axChan.Text = "1";
			dispflag = 1;
		}
		void tranStrainRadioButtonCheckedChanged(object sender, EventArgs e){
			tranChanGroupBox.Visible = true;
			tranChan.Text = "1";
		}
		void NoStrainRadioButtonCheckedChanged(object sender, EventArgs e){
			tranChanGroupBox.Visible = false;
			tranChan.Text = "0";
		}
		void PreviewBttnClick(object sender, EventArgs e){
			//first, check to make sure inputs are of the proper form:
			if (!TryValidateSpecimenInputs())
			{
				return;
			}
			try{
				FindMinAndMax(openFileTxt.Text, Convert.ToInt32(axChan.Text), Convert.ToInt32(tbStressCol.Text) - 1, out int firstIndex, out int lastIndex);
				int rowStart = Convert.ToInt32(rowStartTxtBox.Text) > firstIndex  && Convert.ToInt32(rowStartTxtBox.Text) < lastIndex  
					? Convert.ToInt32(rowStartTxtBox.Text) : firstIndex;
				int rowEnd = Convert.ToInt32(rowEndTxtBox.Text) < lastIndex && Convert.ToInt32(rowEndTxtBox.Text)  > rowStart
					? Convert.ToInt32(rowEndTxtBox.Text) : lastIndex;
				PreSreening ps1 = new PreSreening(openFileTxt.Text, Convert.ToInt32(axChan.Text),
			                                  Convert.ToInt32(tranChan.Text),
			                                  Convert.ToDouble(lengthTxtBox.Text), 
			                                  Convert.ToDouble(xSecAreaTxtBox.Text),
											  rowStart, rowEnd,	firstIndex, lastIndex,
				                               Convert.ToInt32(tbStrainCol.Text), Convert.ToInt32(tbStressCol.Text), dispflag);
				
			}
			catch(FormatException){
            	MessageBox.Show("Invalid Data Set: Verify all Data is numerical and starting row is correct.");
            	return;
            }
			catch(IndexOutOfRangeException){
				MessageBox.Show("There are less columns in the data than specified: Verify channels and starting row are correct");
				return;
			}
			catch{
				MessageBox.Show("Error: Try Again!");
				return;
			}
		}
		void AddCurrentBttnClick(object sender, EventArgs e)
		{
			//first, check to make sure that the inputs are the proper format
			if (!TryValidateSpecimenInputs())
			{
				return;
			}

			//Checks to make sure that all data is numerical, and columns are correct
			try{
				FileReader fr = new FileReader(openFileTxt.Text, Convert.ToInt32(axChan.Text), 
				                               Convert.ToInt32(tranChan.Text),
				                               Convert.ToDouble(lengthTxtBox.Text),
				                               Convert.ToDouble(xSecAreaTxtBox.Text),
				                               Convert.ToInt32(rowStartTxtBox.Text), Convert.ToInt32(rowEndTxtBox.Text),
				                               Convert.ToInt32(tbStrainCol.Text), Convert.ToInt32(tbStressCol.Text), dispflag);
			 }
            catch(FormatException){
            	MessageBox.Show("Invalid Data Set: Verify all Data is numerical and starting row is correct.");
            	return;
            }
			catch(IndexOutOfRangeException){
				MessageBox.Show("There are less columns in the data than specified: Verify channels and starting row are correct");
				return;
			}
			catch{
				MessageBox.Show("Error: Try Again!");
				return;
			}
			//put the input in list form so that elements can be added as we go.
			//there will be a group of 6 elements for each file, then after the analyze button
			//is clicked, 6 more inputs for the whole group are stuck on the back of the list
			string [] indiv = new string[10];
			indiv[0] = (openFileTxt.Text);
			indiv[1] = (axChan.Text);
			indiv[2] = (tranChan.Text);
			indiv[3] = (lengthTxtBox.Text);
			indiv[4] = (xSecAreaTxtBox.Text);
			indiv[5] = (rowStartTxtBox.Text);
			indiv[6] = (rowEndTxtBox.Text);
			indiv[7] = (tbStrainCol.Text);
			indiv[8] = (tbStressCol.Text);
			indiv[9] = (dispflag.ToString());
			individualInputsList.Add(indiv);
			
			numberofFiles++;
			if ((Convert.ToInt16(tranChan.Text)) != 0)
				numberofTranFiles++;
			label10.Text = numberofFiles + " file(s) scanned";
			addCurrentBttn.Enabled = false;
			Browse.Enabled = false;
			label2.Enabled = false;
			openFileTxt.Enabled = false;
			doneBttn.Enabled = true;
			addBttn.Enabled = true;
			previewBttn.Enabled = false;
		}
		void AddBttnClick(object sender, EventArgs e){
			//re-enable the Browse controls that AddCurrentBttnClick disabled, so a new file can be picked
			inputGroupBox.Enabled = false;
			addCurrentBttn.Enabled = true;
			Browse.Enabled = true;
			label2.Enabled = true;
			openFileTxt.Enabled = true;

			if (!TryPickSpecimenFile())
			{
				return;
			}

			inputGroupBox.Visible = true;
			inputGroupBox.Enabled = true;
			previewBttn.Enabled = true;
			if (numberofFiles >= 1)
				doneBttn.Enabled = true;
			addBttn.Enabled = false;
		}
		
		void DoneBttnClick(object sender, EventArgs e){
			groupInputsGroupBox.Visible = true;
			addBttn.Enabled = false;
			doneBttn.Enabled = false;
			Browse.Enabled = false;
			addCurrentBttn.Enabled = false;	
			inputGroupBox.Enabled = false;
			
		}
		//All files input Group
		void GlobPolyOrderTxtBxTextChanged(object sender, EventArgs e)
		{
			try
			{Convert.ToInt32(globPolyOrderTxtBx.Text);}
			catch{
				return;
				}
			
			if ((Convert.ToInt32(globPolyOrderTxtBx.Text)) == 1){
				cbYieldStress.Enabled = false;
                cbYieldStress.Checked = false;
				lOffset.Enabled = false;
                offsetPercentTxtBx.Enabled = false;
            }
			else{
                cbYieldStress.Enabled = true;
			}
		}
		
		void AnalyzeBttnClick(object sender, EventArgs e){
			//first, check to make sure inputs are of the proper format
			try{
				Convert.ToDouble(axStrainCutoffTxtBox.Text);
				Convert.ToDouble(intervalTxtBox.Text);
				Convert.ToDouble(globPolyOrderTxtBx.Text);
				Convert.ToDouble(locPolyOrderTxtBx.Text);
				Convert.ToDouble(rMinTxtBx.Text);
				Convert.ToInt32(tbMinPts.Text);
				Convert.ToInt32(tbExtrapPoints.Text);
				Convert.ToDouble(tbExtrapXCommon.Text);
				if (cbYieldStress.Checked){
					Convert.ToDouble(offsetPercentTxtBx.Text);
				}
			}
			catch{
				MessageBox.Show("Invalid User Input:  Verify Values Are Correct");
				return;
			}
			
			//finish off inputlist by adding group inputs!!!
			material = materialTxtBox.Text;
			groupInputsList = new string[9];
			temperature = temperatureTxtBox.Text;
			
			
			groupInputsList[0] = (axStrainCutoffTxtBox.Text);
			groupInputsList[1] = (locPolyOrderTxtBx.Text);
			groupInputsList[2] = (globPolyOrderTxtBx.Text);
			groupInputsList[3] = (intervalTxtBox.Text);
			groupInputsList[4] = (temperatureTxtBox.Text);
			groupInputsList[5] = (numberofFiles.ToString());
			groupInputsList[6] = (numberofTranFiles.ToString());
			groupInputsList[7] = (tbExtrapPoints.Text);
			groupInputsList[8] = (tbExtrapXCommon.Text);
			
			
			double [] offsetArray = new double [4];
			offsetArray[0] = cbYieldStress.Checked ? 1.0 : 0.0;
			offsetArray[2] = (Convert.ToDouble(rMinTxtBx.Text));
            offsetArray[3] = (Convert.ToDouble(tbMinPts.Text));
            if (cbYieldStress.Checked)
            {
				offsetArray[1] =(Convert.ToDouble(offsetPercentTxtBx.Text));
			}
			
			try{
				analyze = new Analyze(individualInputsList, groupInputsList, offsetArray);
			}
			//Use many different catch terms to isolate exactly where the error is, because most errors appear as
			//IndexOutOfRange errors, so I threw different errors in order to distinguish between error locations
			catch(FileNotFoundException){
				MessageBox.Show("Could not find root of polynomial to zero the data: try reducing polynomial order, " +
				                "reducing strain cutoff, or checking data");
				plotGroupBox.Visible = false;
				dataFilesGroupBox.Visible = false;
				return;
			}
			catch(FormatException){
				MessageBox.Show("Could not find root of polynomial to zero the data: root was on the right of the " +
				                "last point or data is negative");
				plotGroupBox.Visible = false;
				dataFilesGroupBox.Visible = false;
				return;
			}
			catch(ArgumentNullException){
				MessageBox.Show("Invalid Interval, Please reduce and try again.");
				plotGroupBox.Visible = false;
				dataFilesGroupBox.Visible = false;
				return;
			}
			catch(AccessViolationException){
				MessageBox.Show("Warning: One or more intervals doesn't have enough points/interval to generate " +
				                "polynomial: reduce polynomial order or increasse interval size");
				plotGroupBox.Visible = false;
				dataFilesGroupBox.Visible = false;
				return;
			}
			catch(ArgumentOutOfRangeException ex){
				if (ex.ParamName == "extrapPoints"){
					MessageBox.Show("Invalid \"Last # Points\": must be at least 2, greater than the global " +
					                "polynomial order, and no more than the number of points in the shortest " +
					                "specimen curve.");
				}
				else{
					MessageBox.Show("Invalid \"Extrapolate out to strain (x)\": must be at least as large as the largest final x " +
					                "value across all specimens.");
				}
				plotGroupBox.Visible = false;
				dataFilesGroupBox.Visible = false;
				return;
			}
			catch(AppDomainUnloadedException){
				MessageBox.Show("Couldn't find offset intersection.  Try changing the offset or checking the input.");
				plotGroupBox.Visible = false;
				dataFilesGroupBox.Visible = false;
				return;
			}
			catch(Exception ex){
				ErrorDialog.Show("Something's not right: check your data/inputs and give it another try!",
				                 ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
				plotGroupBox.Visible = false;
				dataFilesGroupBox.Visible = false;
				return;
			}
			
			
			if (numberofTranFiles == 0){
				tranAxRadioBttn.Enabled = false;
				taLoessCheckBox.Enabled = false;
				tadhCheckBox.Enabled = false;
				taTotalCheckBox.Enabled = false;
				label13.Enabled = false;
				label15.Enabled = false;
			}
			fileNumberBox.Maximum = numberofFiles;
			analyzeBttn.Text = "Re-Analyze";
			plotGroupBox.Visible = true;
			dataFilesGroupBox.Visible = true;

			MessageBox.Show("Done Analyzing");
		}
		//Plotting Options group
		void StressStrainRadioBttnCheckedChanged(object sender, EventArgs e)
		{
			fileNumberBox.Maximum = numberofFiles;
		}
		void FileNumRadioBttn1CheckedChanged(object sender, EventArgs e)
		{
			fileNumberBox.Enabled = true;
			bZeroeingPlot.Enabled = true;
            if (cbYieldStress.Checked == true)
            {
				YieldStressBttn.Enabled = true;
			}
			
			fileNumber = 1;
		}
		void AllRadioBttnCheckedChanged(object sender, EventArgs e)
		{
			fileNumberBox.Enabled = false;
            bZeroeingPlot.Enabled = false;
            YieldStressBttn.Enabled = false;
			fileNumber = 0;
		}
		//Warning: this section is HUGE, so don't get into it unless you really have some time!!!
		//However, it isn't really meaty, and is based on the plotting buttons above.
		void PlotBttnClick(object sender, EventArgs e)
		{
			fileNumber = (int)fileNumberBox.Value;
			PlotMaker pm = new PlotMaker(analyze,numberofFiles, temperature, material, fileNumber);
			// Exactly one of the three radio groups (stress-strain axis, all/single/combined
			// specimen selection, raw/combined/slope view) can be checked at a time, so these
			// twelve conditions are mutually exclusive -- chained with else-if so at most one is
			// evaluated to completion instead of testing all twelve on every click.
			if ((stressStrainRadioBttn.Checked == true) && (allRadioBttn.Checked == true)
			    && (rawRadioBttn.Checked == true)){
			    	pm.PlotMaker1();
			}
			else if ((stressStrainRadioBttn.Checked == true) && (allRadioBttn.Checked == false)
			    && (rawRadioBttn.Checked == true)){
			    	pm.PlotMaker2();
			}
			else if ((stressStrainRadioBttn.Checked == false) && (allRadioBttn.Checked == true)
			    && (rawRadioBttn.Checked == true)){
			    	pm.PlotMaker3();
			}
			else if ((stressStrainRadioBttn.Checked == false) && (allRadioBttn.Checked == false)
			    && (rawRadioBttn.Checked == true)){
			    	pm.PlotMaker4();
			}
			else if ((stressStrainRadioBttn.Checked == true) && (allRadioBttn.Checked == true)
			    && (combinedRadioBttn.Checked == true)){
			    	pm.PlotMaker5();
			}
			else if ((stressStrainRadioBttn.Checked == true) && (allRadioBttn.Checked == false)
			    && (combinedRadioBttn.Checked == true)){
			    	pm.PlotMaker6();
			}
			else if ((stressStrainRadioBttn.Checked == false) && (allRadioBttn.Checked == true)
			    && (combinedRadioBttn.Checked == true)){
			    	pm.PlotMaker7();
			}
			else if ((stressStrainRadioBttn.Checked == false) && (allRadioBttn.Checked == false)
			    && (combinedRadioBttn.Checked == true)){
			    	pm.PlotMaker8();
			}
			else if ((stressStrainRadioBttn.Checked == true) && (allRadioBttn.Checked == true)
			    && (SlopeRadioBttn.Checked == true)){
			    	pm.PlotMaker9();
			}
			else if ((stressStrainRadioBttn.Checked == true) && (allRadioBttn.Checked == false)
			    && (SlopeRadioBttn.Checked == true)){
			    	pm.PlotMaker10();
			}
			else if ((stressStrainRadioBttn.Checked == false) && (allRadioBttn.Checked == true)
			    && (SlopeRadioBttn.Checked == true)){
			    	pm.PlotMaker11();
			}
			else if ((stressStrainRadioBttn.Checked == false) && (allRadioBttn.Checked == false)
			    && (SlopeRadioBttn.Checked == true)){
			    	pm.PlotMaker12();
			}
		}


        //Data Files group
        void FileWriteBttnClick(object sender, EventArgs e)
		{ //writes a file for each checked box
			FileWriter fw = new FileWriter(analyze,numberofFiles, temperature, material, folder);
			if (ssdhCheckBox.Checked == true){
				fw.FileWriter1();
			}
			if (ssLoessCheckBox.Checked == true){
				fw.FileWriter2();
			}
			if (tadhCheckBox.Checked == true){
				fw.FileWriter3();
			}
			if (taLoessCheckBox.Checked == true){
				fw.FileWriter4();
			}
			if (ssTotalCheckBox.Checked == true){
				fw.FileWriter5();
			}
			if (taTotalCheckBox.Checked == true){
				fw.FileWriter6();
			}
			MessageBox.Show("File(s) Written");
			
			
		}
		void ResultsBttnClick(object sender, EventArgs e)
		{
			ResultsWindow rw = new ResultsWindow(analyze, temperature, material);
		}	
		void YieldStressBttnClick(object sender, EventArgs e)
		{
			fileNumber = (int)fileNumberBox.Value;
			PlotMaker pm = new PlotMaker(analyze,numberofFiles, temperature, material, fileNumber);
			pm.PlotMaker15();
		}

        private void cbYieldStress_CheckedChanged(object sender, EventArgs e)
        {
			lOffset.Enabled = cbYieldStress.Checked;
			offsetPercentTxtBx.Enabled = cbYieldStress.Checked;
        }

        private void bExtrapolationPlot_Click(object sender, EventArgs e)
        {
            //Plot the pooled zeroed data extended out to the common strain, plus the LOESS mean
            //points and global fit through it.  Stress/strain, and transverse/axial if it exists.
            fileNumber = (int)fileNumberBox.Value;
            PlotMaker pm = new PlotMaker(analyze, numberofFiles, temperature, material, fileNumber);
            pm.PlotMaker17();
            if (numberofTranFiles != 0){
                pm.PlotMaker18();
            }
        }

        private void bZeroeingPlot_Click(object sender, EventArgs e)
        {
            //Plot raw data, zeroed data, data used for linear fit, and linear fit
            fileNumber = (int)fileNumberBox.Value;
            PlotMaker pm = new PlotMaker(analyze, numberofFiles, temperature, material, fileNumber);
            pm.PlotMaker16();
        }

        private void FindMinAndMax(string root, int indexOfStrain, int indexOfStress, out int firstIndex, out int lastIndex)
        {
            firstIndex = -1;
            lastIndex = -1;

            string[] lines = File.ReadAllLines(root + ".csv");

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] values = line.Split(new char[] { ',', '/', '"' }, StringSplitOptions.RemoveEmptyEntries);

                if (indexOfStrain < values.Length && double.TryParse(values[indexOfStrain], out _))
                {
                    if (indexOfStress < values.Length && double.TryParse(values[indexOfStress], out _))
                    {
                        if (firstIndex == -1)
                        {
                            firstIndex = i + 1;
                        }
                        lastIndex = i + 1;
                    }
                }
            }
        }
    }
}
