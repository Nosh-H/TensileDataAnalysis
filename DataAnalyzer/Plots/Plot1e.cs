/*
 * Created by SharpDevelop.
 * User: e46221
 * Date: 6/28/2007
 * Time: 4:01 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;

namespace DataAnalyzer.Plots
{
	/// <summary>
	/// Description of Plot.
	/// </summary>
	public partial class Plot1e : Form
	{
		PointPairList list1 = new PointPairList();
		PointPairList list1e = new PointPairList();
		string title = "Titleless";
		string xTitle = "Titleless";
		string yTitle = "Titleless";
		string dataLabel1 = "No Label";
		string dataLabel1e = "No Label";

		public Plot1e(PointPairList list1, PointPairList list1e,
		             string title, string xTitle, string yTitle,
		             string dataLabel1, string dataLabel1e)
		{
			//inX1,inY1 are the raw data, inX2,inY2 represents the data after outliers have been removed,
			// and inX3, inY3 represent the LOESS points for the data.
			//
			this.list1 = list1;
			this.list1e = list1e;
			this.title = title;
			this.xTitle = xTitle;
			this.yTitle = yTitle;
			this.dataLabel1 = dataLabel1;
			this.dataLabel1e = dataLabel1e;

			InitializeComponent();
			Activate();
			Show();
		}
		
		void PlotLoad(object sender, EventArgs e)
		{
			CreateChart( zg1 );
   			SetSize();
		}
		// Call this method from the Form_Load method, passing your ZedGraphControl
		public void CreateChart( ZedGraphControl zgc ){
			GraphPane myPane = zgc.GraphPane;

   			// Set the title and axis labels
   			myPane.Title.Text = title + " : " + DateTime.Now.ToString();
   			myPane.XAxis.Title.Text = xTitle;
   			myPane.YAxis.Title.Text = yTitle;


   			LineItem myCurve1 = myPane.AddCurve( dataLabel1, list1, Color.Black, SymbolType.Diamond );
   			   			   			  			
   			// Don't display the line (This makes a scatter plot)
    		myCurve1.Line.IsVisible = false;
    		// Hide the symbol outline
    		myCurve1.Symbol.Border.IsVisible = false;
    		// Fill the symbol interior with color
    		myCurve1.Symbol.Fill = new Fill( Color.Red );
    		
    		// Generate a red bar with "Curve 1" in the legend
   			ErrorBarItem myCurve1e = myPane.AddErrorBar( dataLabel1e, list1e, Color.Black );
   			myCurve1e.Bar.PenWidth = 1f;
   			// Use the HDash symbol so that the error bars look like I-beams
   			myCurve1e.Bar.Symbol.Type = SymbolType.HDash;
  			myCurve1e.Bar.Symbol.IsVisible = true;
  			
    		// Fill the background of the chart rect and pane
    		//myPane.Chart.Fill = new Fill( Color.White, Color.LightGoldenrodYellow, 45.0f );
    		myPane.Chart.Fill = new Fill( Color.LightGoldenrodYellow);
    		//myPane.Fill = new Fill( Color.SlateGray,Color.BlueViolet);
			// Hide the legend
    		myPane.Legend.IsVisible = true;
    		// turn off the opposite tics so the Y tics don't show up on the Y2 axis
   			myPane.YAxis.MajorTic.IsOpposite = false;
   			myPane.YAxis.MinorTic.IsOpposite = false;
   			myPane.XAxis.MajorTic.IsOpposite = false;
   			myPane.XAxis.MinorTic.IsOpposite = false;
   			// Display the Y2 axis grid lines
   			myPane.YAxis.MajorGrid.IsVisible = true;
   			myPane.XAxis.MajorGrid.IsVisible = true;
   		
   			// Calculate the Axis Scale Ranges
   			zgc.AxisChange();
		}
		
		void PlotResize(object sender, EventArgs e){
			SetSize();
		}
		private void SetSize(){
   			zg1.Location = new Point( 10, 10 );
   			// Leave a small margin around the outside of the control
   			zg1.Size = new Size( this.ClientRectangle.Width - 20, this.ClientRectangle.Height - 20 );
		}
	}
}
