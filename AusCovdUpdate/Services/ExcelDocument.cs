﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using AusCovdUpdate.Model;
using AusCovdUpdate.ServiceInterfaces;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AusCovdUpdate.Services
{
    public class ExcelDocument : IExcelDocument, IDisposable
    {
        private SpreadsheetDocument spreadsheetDocument;
        private bool disposedValue;

        private const string ActCases = "C";
        private const string ActRecovered = "D";
        private const string ActDeaths = "E";

        private const string NswCases = "F";
        private const string NswRecovered = "G";
        private const string NswDeaths = "H";

        private const string NtCases = "I";
        private const string NtRecovered = "J";
        private const string NtDeaths = "K";

        private const string QldCases = "L";
        private const string QldRecovered = "M";
        private const string QldDeaths = "N";

        private const string SaCases = "O";
        private const string SaRecovered = "P";
        private const string SaDeaths = "Q";

        private const string TasCases = "R";
        private const string TasRecovered = "S";
        private const string TasDeaths = "T";

        private const string VicCases = "U";
        private const string VicRecovered = "V";
        private const string VicDeaths = "W";

        private const string WaCases = "X";
        private const string WaRecovered = "Y";
        private const string WaDeaths = "Z";

        public void OpenDocument (string path)
        {

            this.spreadsheetDocument = SpreadsheetDocument.Open (path, true);

        }

        public void UpdateSpreadsheet (Dictionary<DateTime, Covid19Aus> items)
        {
            Contract.Assert (items != null);

            var dailyDataSheets = this.spreadsheetDocument.WorkbookPart
                .Workbook
                .Descendants<Sheet> ()
                .Where (s => s.Name == "Daily");

            var dailyData = ((WorksheetPart) this.spreadsheetDocument.WorkbookPart.GetPartById (dailyDataSheets.First ().Id)).Worksheet;

            var firstDate = items.Keys
                .OrderBy (x => x)
                .FirstOrDefault ();

            uint lastSeenRow = 0;
            DateTime? lastDate = null;

            foreach (var row in dailyData.Descendants<Row> ().Where (r => r.RowIndex.Value >= 3).OrderBy(x=>x.RowIndex.Value))
            {
                lastSeenRow = GetRowIndex (row.RowIndex);

                var cell = GetCellForColumn (row, "A");

                // get the date for the current row
                var currentRowDate = DateTime.FromOADate (double.Parse (cell.CellValue.Text, CultureInfo.InvariantCulture));

                lastDate = currentRowDate;

                if (currentRowDate < firstDate)
                {
                    // Clear out the deaths and recovereies
                    GetCellForColumn (row, ActRecovered)?.Remove ();
                    GetCellForColumn (row, ActDeaths)?.Remove ();

                    GetCellForColumn (row, NswRecovered)?.Remove ();
                    GetCellForColumn (row, NswDeaths)?.Remove ();

                    GetCellForColumn (row, NtRecovered)?.Remove ();
                    GetCellForColumn (row, NtDeaths)?.Remove ();

                    GetCellForColumn (row, QldRecovered)?.Remove ();
                    GetCellForColumn (row, QldDeaths)?.Remove ();

                    GetCellForColumn (row, SaRecovered)?.Remove ();
                    GetCellForColumn (row, SaDeaths)?.Remove ();

                    GetCellForColumn (row, TasRecovered)?.Remove ();
                    GetCellForColumn (row, TasDeaths)?.Remove ();

                    GetCellForColumn (row, VicRecovered)?.Remove ();
                    GetCellForColumn (row, VicDeaths)?.Remove ();

                    GetCellForColumn (row, WaRecovered)?.Remove ();
                    GetCellForColumn (row, WaDeaths)?.Remove ();

                }
                else
                {
                    if (items.ContainsKey (currentRowDate))
                    {
                        var values = items[currentRowDate];
                        UpdateStatisticsForState (row, ActCases, ActRecovered, ActDeaths, values.ACT);
                        UpdateStatisticsForState (row, NswCases, NswRecovered, NswDeaths, values.NSW);
                        UpdateStatisticsForState (row, NtCases, NtRecovered, NtDeaths, values.NT);
                        UpdateStatisticsForState (row, QldCases, QldRecovered, QldDeaths, values.QLD);
                        UpdateStatisticsForState (row, SaCases, SaRecovered, SaDeaths, values.SA);
                        UpdateStatisticsForState (row, TasCases, TasRecovered, TasDeaths, values.TAS);
                        UpdateStatisticsForState (row, VicCases, VicRecovered, VicDeaths, values.VIC);
                        UpdateStatisticsForState (row, WaCases, WaRecovered, WaDeaths, values.WA);
                    }
                }
            }
            var calculationProperties = this.spreadsheetDocument.WorkbookPart.Workbook.CalculationProperties;
            calculationProperties.ForceFullCalculation = true;
            calculationProperties.FullCalculationOnLoad = true;
        }

        public static Cell GetCellForColumn (Row row, string column)
        {
            Contract.Assert (row != null);

            var cell = row
                .Elements<Cell> ()
                .Where (x => GetColumnName (x.CellReference) == column)
                .FirstOrDefault ();

            return cell;
        }

        // Given a cell name, parses the specified cell to get the row index.
        private static uint GetRowIndex (string cellName)
        {
            // Create a regular expression to match the row index portion the cell name.
            var regex = new Regex (@"\d+");
            var match = regex.Match (cellName);

            return uint.Parse (match.Value, CultureInfo.InvariantCulture);
        }

        // Given a cell name, parses the specified cell to get the column name.
        private static string GetColumnName (string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            var regex = new Regex ("[A-Za-z]+");
            var match = regex.Match (cellName);

            return match.Value;
        }

        private static void UpdateStatisticsForState (Row row, string casesColumn, string recoveredColumn, string deathColumn, AusCovid19State state)
        {
            UpdateCell (row, casesColumn, state.Cases);
            UpdateCell (row, recoveredColumn, state.Recovered);
            UpdateCell (row, deathColumn, state.Deaths);
        }

        private static void UpdateCell (Row row, string column, int value)
        {
            var rowNumber = row.RowIndex;
            var cell = GetCellForColumn (row, column);

            if (value == 0)
            {
                if (cell != null)
                {
                    cell.Remove ();
                }
            }
            else
            {
                if (cell == null)
                {
                    var cellName = column + rowNumber;
                    var nextCell = row.Elements<Cell> ()
                        .Where (x => string.Compare (x.CellReference.Value, cellName, true, CultureInfo.InvariantCulture) > 0)
                        .OrderBy (x => x.CellReference.Value)
                        .FirstOrDefault ();
                    cell = new Cell () { CellReference = cellName };

                    _ = row.InsertBefore (cell, nextCell);

                }
                cell.CellValue = new CellValue (value.ToString (CultureInfo.InvariantCulture));
            }

        }

        protected virtual void Dispose (bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.spreadsheetDocument.Dispose ();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ExcelDocument()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose ()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose (disposing: true);
            GC.SuppressFinalize (this);
        }
    }
}
