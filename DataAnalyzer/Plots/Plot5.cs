/*
 * Created by SharpDevelop.
 * User: e46221
 * Date: 6/28/2007
 * Time: 4:01 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;

namespace DataAnalyzer.Plots
{
	/// <summary>
	/// Description of Plot.  Plot 3 dot lists, and two lines
	/// </summary>
	public partial class Plot5 : Form
	{
		PointPairList list1 = new PointPairList();
		PointPairList list2 = new PointPairList();
		PointPairList list3 = new PointPairList();
        PointPairList lineList1 = new PointPairList();
        PointPairList lineList2 = new PointPairList();
        string title = "Titleless";
		string xTitle = "Titleless";
		string yTitle = "Titleless";
		string dataLabel1 = "No Label";
		string dataLabel2 = "No Label";
		string dataLabel3 = "No Label";
        string dataLabelLine1 = "No Label";
        string dataLabelLine2 = "No Label";

        public Plot5(PointPairList list1, PointPairList list2, PointPairList list3, PointPairList listLine1,
                     PointPairList listLine2, string title, string xTitle, string yTitle,
		             string dataLabel1, string dataLabel2, string dataLabel3, string dataLabelLine1, string dataLabelLine2)
		{
			//inX1,inY1 are the raw data, inX2,inY2 represents the data after outliers have been removed,
			// and inX3, inY3 represent the LOESS points for the data.
			//
			this.list1 = list1;
			this.list2 = list2;
			this.list3 = list3;
            lineList1 = listLine1;
            lineList2 = listLine2;
            this.title = title;
			this.xTitle = xTitle;
			this.yTitle = yTitle;
			this.dataLabel1 = dataLabel1;
			this.dataLabel2 = dataLabel2;
			this.dataLabel3 = dataLabel3;
            this.dataLabelLine1 = dataLabelLine1;
            this.dataLabelLine2 = dataLabelLine2;

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
    
   			// Make up some data arrays based on the Sine function
   			/*PointPairList list1 = new PointPairList();
   			PointPairList list2 = new PointPairList();
   			PointPairList list3 = new PointPairList();
   			
   			for ( int i=0; i<X1.Length; i++ ){
   				list1.Add( X1[i], Y1[i] );
   			}
   			for ( int i=0; i<X2.Length; i++ ){
   				list2.Add( X2[i], Y2[i] );
   			}
   			for ( int i=0; i<X3.Length; i++ ){
   				list3.Add( X3[i], Y3[i] );
   			}
    */
   			// Generate a red curve with diamond
   			LineItem myCurve3 = myPane.AddCurve( dataLabel3, list3, Color.Black, SymbolType.Diamond );
   			LineItem myCurve2 = myPane.AddCurve( dataLabel2,	list2, Color.Blue, SymbolType.Circle );
   			LineItem myCurve1 = myPane.AddCurve( dataLabel1,	list1, Color.Red, SymbolType.Circle );

            LineItem myLine1 = myPane.AddCurve(dataLabelLine1, lineList1, Color.Green, SymbolType.None);
            LineItem myLine2 = myPane.AddCurve(dataLabelLine2, lineList2, Color.Blue, SymbolType.None);

            // Don't display the line (This makes a scatter plot)
            myCurve1.Line.IsVisible = false;
    		myCurve2.Line.IsVisible = false;
    		myCurve3.Line.IsVisible = false;
    		// Hide the symbol outline
    		myCurve1.Symbol.Border.IsVisible = false;
    		myCurve2.Symbol.Border.IsVisible = false;
    		myCurve3.Symbol.Border.IsVisible = false;
    		// Fill the symbol interior with color
    		myCurve1.Symbol.Fill = new Fill( Color.Red );
    		myCurve2.Symbol.Fill = new Fill( Color.Blue );
    		myCurve3.Symbol.Fill = new Fill( Color.Black );
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
