/*
 * Created by SharpDevelop.
 * User: e46221
 * Date: 7/11/2007
 * Time: 3:40 PM
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using DataAnalyzer;

namespace TestStressStrainData
{
	[TestClass]
	public class TestFileReader
	{
		private const double AcceptablePrecision = 1.0E-9;

		/// <summary>
		/// Resolves a fixture in TestFiles/ to the "root" form FileReader expects: a full path
		/// with the ".csv" extension stripped, since FileReader appends ".csv" itself.
		/// The CSVs are copied next to the test assembly by TestDataAnalyzer.csproj.
		/// </summary>
		private static string FixtureRoot(string fileNameWithoutExtension)
		{
			string testDirectory = Path.GetDirectoryName(typeof(TestFileReader).Assembly.Location);
			return Path.Combine(testDirectory, "TestFiles", fileNameWithoutExtension);
		}

		/// <summary>
		/// SampleInput_Specimen1.csv has two header rows, then "strain,stress" data pairs.
		/// Reading it with area and gauge length of 1 should pass the values through unchanged,
		/// with column 0 holding stress and column 1 holding axial strain.
		/// </summary>
		[TestMethod]
		public void TestReadsStrainStressColumns()
		{
			// rowStart is the 1-based line number of the first data row; strain is column 1 and
			// stress column 2 (both 1-based). rowEnd is past the end of the file, so all rows load.
			FileReader rawData = new FileReader(FixtureRoot("SampleInput_Specimen1"),
			                                    inaxChan: 1, intranChan: 0,
			                                    inlength: 1.0, inxSecArea: 1.0,
			                                    inrowStart: 3, inrowEnd: 10000,
			                                    inStrainCol: 1, inStressCol: 2, indispflag: 0);

			// Column 0 = stress, column 1 = axial strain, straight from the first data row "0,0.2758"
			Assert.AreEqual(0.2758, rawData.RawData[0, 0], AcceptablePrecision);
			Assert.AreEqual(0.0, rawData.RawData[0, 1], AcceptablePrecision);

			// Second data row: "0.002228758,0.6853"
			Assert.AreEqual(0.6853, rawData.RawData[1, 0], AcceptablePrecision);
			Assert.AreEqual(0.002228758, rawData.RawData[1, 1], AcceptablePrecision);

			// One stress column plus one axial strain channel
			Assert.AreEqual(2, rawData.RawData.GetLength(1));
			Assert.AreEqual(0, rawData.TranChan);
			Assert.AreEqual(1, rawData.AxChan);
		}

		/// <summary>
		/// Force is divided by the cross-sectional area to get stress, and displacement by the
		/// gauge length to get strain. Passing non-unit values must scale both columns.
		/// </summary>
		[TestMethod]
		public void TestConvertsForceAndDisplacement()
		{
			const double area = 4.0;
			const double gaugeLength = 2.0;

			FileReader rawData = new FileReader(FixtureRoot("SampleInput_Specimen1"),
			                                    inaxChan: 1, intranChan: 0,
			                                    inlength: gaugeLength, inxSecArea: area,
			                                    inrowStart: 3, inrowEnd: 10000,
			                                    inStrainCol: 1, inStressCol: 2, indispflag: 0);

			Assert.AreEqual(0.6853 / area, rawData.RawData[1, 0], AcceptablePrecision);
			Assert.AreEqual(0.002228758 / gaugeLength, rawData.RawData[1, 1], AcceptablePrecision);
			Assert.AreEqual(area, rawData.XSecArea, AcceptablePrecision);
			Assert.AreEqual(gaugeLength, rawData.Length, AcceptablePrecision);
		}

		/// <summary>
		/// rowEnd bounds how many rows are read past rowStart, so a narrow window must yield
		/// exactly that many rows rather than the whole file.
		/// </summary>
		[TestMethod]
		public void TestRespectsRowWindow()
		{
			FileReader rawData = new FileReader(FixtureRoot("SampleInput_Specimen1"),
			                                    inaxChan: 1, intranChan: 0,
			                                    inlength: 1.0, inxSecArea: 1.0,
			                                    inrowStart: 3, inrowEnd: 7,
			                                    inStrainCol: 1, inStressCol: 2, indispflag: 0);

			// Rows 3 through 7 inclusive
			Assert.AreEqual(5, rawData.RawData.GetLength(0));
		}

		/// <summary>
		/// Regression test for the quote/backslash handling in FileReader's split: the raw
		/// equipment output wraps values in double quotes, which must not reach Convert.ToDouble.
		/// Specimen_RawData_1.csv is a real capture from the testing machine.
		/// </summary>
		[TestMethod]
		public void TestParsesQuotedEquipmentOutput()
		{
			// Time / Extension / Load, with a units row -- data starts at line 7.
			// Load (column 3) is the force channel, Extension (column 2) the displacement.
			FileReader rawData = new FileReader(FixtureRoot("Specimen_RawData_1"),
			                                    inaxChan: 1, intranChan: 0,
			                                    inlength: 1.0, inxSecArea: 1.0,
			                                    inrowStart: 7, inrowEnd: 10000,
			                                    inStrainCol: 2, inStressCol: 3, indispflag: 1);

			Assert.IsTrue(rawData.RawData.GetLength(0) > 0, "expected at least one data row");
			Assert.AreEqual(2, rawData.RawData.GetLength(1));
		}
	}
}
