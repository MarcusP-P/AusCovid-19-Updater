using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using AusCovdUpdate.Model;
using AusCovdUpdate.ServiceInterfaces;

using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AusCovdUpdate.Services
{
    public class ExcelDocument : IExcelDocument, IDisposable
    {
        private SpreadsheetDocument spreadsheetDocument;
        private bool disposedValue;

        #region DailyData
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
        #endregion

        #region InternationalData
        private const string BelgiumColumn = "C";
        private const string BelgiumName = "Belgium";

        private const string DenmarkColumn = "G";
        private const string DenmarkName = "Denmark";

        private const string FranceColumn = "K";
        private const string FranceName = "France";

        private const string ItalyColumn = "O";
        private const string ItalyName = "Italy";

        private const string SouthKoreaColumn = "S";
        private const string SouthKoreaName = "Korea, South";

        private const string SingaporeColumn = "W";
        private const string SingaporeName = "Singapore";

        private const string UnitedKingdomColumn = "AA";
        private const string UnitedKingdomName = "United Kingdom";

        private const string UnitedStatesColumn = "AE";
        private const string UnitedStatesName = "US";
        #endregion

        public void OpenDocument (string path)
        {

            this.spreadsheetDocument = SpreadsheetDocument.Open (path, true);

        }

        public void UpdateDailyData (Dictionary<DateTime, Covid19Aus> items)
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

            uint? dateSyleIndex = null;

            foreach (var row in dailyData.Descendants<Row> ()
                .Where (r => r.RowIndex.Value >= 3)
                .OrderBy (x => x.RowIndex.Value))
            {
                lastSeenRow = GetRowIndex (row.RowIndex);

                var cell = GetCellForColumn (row, "A");

                if (dateSyleIndex == null)
                {
                    dateSyleIndex = cell.StyleIndex.Value;
                }

                lastSeenRow = row.RowIndex.Value;

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
                        UpdateDailyStatisticsRow (row, items[currentRowDate]);
                    }
                }
            }

            var sheetData = dailyData.GetFirstChild<SheetData> ();

            // Go through the remainign dates
            foreach (var newDate in items.Keys
                .Where (x => x > lastDate)
                .OrderBy (x => x))
            {
                var newRow = lastSeenRow + 1;
                var row = dailyData.Descendants<Row> ()
                 .Where (r => r.RowIndex.Value == newRow)
                 .OrderBy (x => x.RowIndex.Value)
                 .FirstOrDefault ();

                if (row == null)
                {
                    row = new Row { RowIndex = newRow };

                    sheetData.Append (row);
                }

                // It's okay to drop the fractional part
                var dateCell = UpdateCell (row, "A", (int) newDate.ToOADate ());

                dateCell.StyleIndex = dateSyleIndex;

                UpdateDailyStatisticsRow (row, items[newDate]);

                lastSeenRow = newRow;

            }

            var calculationProperties = this.spreadsheetDocument.WorkbookPart.Workbook.CalculationProperties;
            calculationProperties.ForceFullCalculation = true;
            calculationProperties.FullCalculationOnLoad = true;
        }

        private static void UpdateDailyStatisticsRow (Row row, Covid19Aus values)
        {
            UpdateStatisticsForState (row, ActCases, ActRecovered, ActDeaths, values.ACT);
            UpdateStatisticsForState (row, NswCases, NswRecovered, NswDeaths, values.NSW);
            UpdateStatisticsForState (row, NtCases, NtRecovered, NtDeaths, values.NT);
            UpdateStatisticsForState (row, QldCases, QldRecovered, QldDeaths, values.QLD);
            UpdateStatisticsForState (row, SaCases, SaRecovered, SaDeaths, values.SA);
            UpdateStatisticsForState (row, TasCases, TasRecovered, TasDeaths, values.TAS);
            UpdateStatisticsForState (row, VicCases, VicRecovered, VicDeaths, values.VIC);
            UpdateStatisticsForState (row, WaCases, WaRecovered, WaDeaths, values.WA);

        }

        private static void UpdateStatisticsForState (Row row, string casesColumn, string recoveredColumn, string deathColumn, AusCovid19State state)
        {
            _ = UpdateCell (row, casesColumn, state.Cases);
            _ = UpdateCell (row, recoveredColumn, state.Recovered);
            _ = UpdateCell (row, deathColumn, state.Deaths);
        }

        public void UpdateInternationalData (Dictionary<DateTime, Dictionary<string, int>> items)
        {
            Contract.Assert (items != null);

            // Get the sheets
            var internationalDataSheets = this.spreadsheetDocument.WorkbookPart
                .Workbook
                .Descendants<Sheet> ()
                .Where (s => s.Name == "International");

            // Get the actual sheet
            var internationalData = ((WorksheetPart) this.spreadsheetDocument.WorkbookPart.GetPartById (internationalDataSheets.First ().Id)).Worksheet;

            // Get the first date from the data
            var firstDate = items.Keys
                .OrderBy (x => x)
                .FirstOrDefault ();

            // Keep track of the last row that we've seen
            uint lastSeenRow = 0;

            // Keep track of the last date we've seen
            DateTime? lastDate = null;

            // Get the style index for dates
            uint? dateSyleIndex = null;

            // Go through the current rows
            foreach (var row in internationalData.Descendants<Row> ()
                .Where (r => r.RowIndex.Value >= 3)
                .OrderBy (x => x.RowIndex.Value))
            {
                // Update last seen
                lastSeenRow = GetRowIndex (row.RowIndex);

                // Get the date column
                var cell = GetCellForColumn (row, "A");

                if (dateSyleIndex == null)
                {
                    dateSyleIndex = cell.StyleIndex.Value;
                }

                // get the date for the current row
                DateTime currentRowDate;
                if (cell != null && cell.CellValue != null)
                {
                    currentRowDate = DateTime.FromOADate (double.Parse (cell.CellValue.Text, CultureInfo.InvariantCulture));
                }
                else
                {
                    // We don't have a date, add it
                    currentRowDate = lastDate?.AddDays (1)??DateTime.Now.Date;

                    // Check that the date is in tthe data
                    if (!items.ContainsKey(currentRowDate))
                    {
                        break;
                    }
                    cell = UpdateCell (row, "A", (int) currentRowDate.ToOADate ());
                    cell.StyleIndex.Value = (uint) dateSyleIndex;
                }

                lastDate = currentRowDate;

                if (currentRowDate >= firstDate)
                {
                    if (items.ContainsKey (currentRowDate))
                    {
                        AddInternationalData (row, items[currentRowDate]);
                    }
                }
            }

            // We need this to append rows
            var sheetData = internationalData.GetFirstChild<SheetData> ();

            // Go through the remainign dates
            foreach (var newDate in items.Keys
                .Where (x => x > lastDate)
                .OrderBy (x => x))
            {
                // calculate the next row's value
                var newRow = lastSeenRow + 1;

                // Load the row
                var row = internationalData.Descendants<Row> ()
                 .Where (r => r.RowIndex.Value == newRow)
                 .OrderBy (x => x.RowIndex.Value)
                 .FirstOrDefault ();

                // If the row doesn't exist, add it
                if (row == null)
                {
                    row = new Row { RowIndex = newRow };

                    sheetData.Append (row);
                }

                // It's okay to drop the fractional part
                var dateCell = UpdateCell (row, "A", (int) newDate.ToOADate ());

                dateCell.StyleIndex = dateSyleIndex;

                AddInternationalData (row, items[newDate]);

                lastSeenRow = newRow;
            }

            var calculationProperties = this.spreadsheetDocument.WorkbookPart.Workbook.CalculationProperties;
            calculationProperties.ForceFullCalculation = true;
            calculationProperties.FullCalculationOnLoad = true;
        }

        private static void AddInternationalData (Row row, Dictionary<string, int> items)
        {
            _ = UpdateCell (row, BelgiumColumn, items[BelgiumName]);
            _ = UpdateCell (row, DenmarkColumn, items[DenmarkName]);
            _ = UpdateCell (row, FranceColumn, items[FranceName]);
            _ = UpdateCell (row, ItalyColumn, items[ItalyName]);
            _ = UpdateCell (row, SouthKoreaColumn, items[SouthKoreaName]);
            _ = UpdateCell (row, SingaporeColumn, items[SingaporeName]);
            _ = UpdateCell (row, UnitedKingdomColumn, items[UnitedKingdomName]);
            _ = UpdateCell (row, UnitedStatesColumn, items[UnitedStatesName]);
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

        private static Cell UpdateCell (Row row, string column, int value)
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
            return cell;
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
