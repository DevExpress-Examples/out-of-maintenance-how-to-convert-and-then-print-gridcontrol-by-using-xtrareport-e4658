using DevExpress.Data;
using DevExpress.Drawing.Printing;
using DevExpress.Xpf.Grid;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace ReportHelper {

    public delegate void CustomizeColumnsCollectionEventHandler(object source, ColumnsCreationEventArgs e);
    public delegate void CustomizeColumnEventHandler(object source, ControlCustomizationEventArgs e);

    public class ReportGeneratonHelper {
        XtraReport report;
        const int initialGroupOffset = 0;
        const int subGroupOffset = 10;
        const int bandHeight = 20;
        const bool shouldRepeatGroupHeadersOnEveryPage = false;
        Hashtable detailsInfo = new Hashtable();

        public event CustomizeColumnsCollectionEventHandler CustomizeColumnsCollection;
        public event CustomizeColumnEventHandler CustomizeColumn;


        public XtraReport GenerateReport(GridControl grid, object aspDataSource) {
            report = new XtraReport();
            report.Landscape = true;
            report.PaperKind = DXPaperKind.Letter;

            InitDataSource(aspDataSource);
            InitDetailsAndPageHeader(grid);
            InitSortings(grid);
            InitGroupHeaders(grid);
            InitFilters(grid);
            InitTotalSummaries(grid);
            return report;
        }

        void InitTotalSummaries(GridControl grid) {
            if (grid.TotalSummary.Count > 0) {
                report.Bands.Add(new ReportFooterBand() { HeightF = bandHeight });
                foreach (GridSummaryItem item in grid.TotalSummary) {
                    GridColumn col = grid.Columns[item.ShowInColumn == string.Empty ? item.FieldName : item.ShowInColumn];
                    if (col != null) if (detailsInfo.Contains(col)) {
                            XRLabel label = new XRLabel();
                            label.LocationF = ((XRTableCell)detailsInfo[col]).LocationF;
                            label.SizeF = ((XRTableCell)detailsInfo[col]).SizeF;
                            label.DataBindings.Add("Text", null, col.FieldName);
                            label.Summary = new XRSummary() { Running = SummaryRunning.Report };
                            label.Summary.FormatString = item.DisplayFormat;
                            label.Summary.Func = GetSummaryFunc(item.SummaryType);
                            report.Bands[BandKind.ReportFooter].Controls.Add(label);
                        }
                }
            }
        }

        void InitDataSource(object DataSource) {
            report.DataSource = DataSource;
        }

        void InitGroupHeaders(GridControl grid) {
            for (int i = ((TableView)grid.View).GroupedColumns.Count - 1; i >= 0; i--) {
                GridColumn groupedColumn = ((TableView)grid.View).GroupedColumns[i];
                GroupHeaderBand gb = new GroupHeaderBand();
                gb.Height = bandHeight;
                XRLabel l = new XRLabel();
                l.Text = groupedColumn.FieldName + ": [" + groupedColumn.FieldName + "]";
                l.LocationF = new PointF(initialGroupOffset + i * 10, 0);
                l.BackColor = Color.Beige;
                l.SizeF = new SizeF((report.PageWidth - (report.Margins.Left + report.Margins.Right)) - (initialGroupOffset + i * subGroupOffset), bandHeight);
                gb.Controls.Add(l);
                gb.RepeatEveryPage = shouldRepeatGroupHeadersOnEveryPage;
                GroupField gf = new GroupField(
                    groupedColumn.FieldName,
                    groupedColumn.SortOrder == ColumnSortOrder.Ascending ? XRColumnSortOrder.Ascending : XRColumnSortOrder.Descending);
                gb.GroupFields.Add(gf);
                report.Bands.Add(gb);
            }
        }
        void InitSortings(GridControl grid) {
            IList<GridColumn> columns = GetVisibleDataColumns(grid);
            for (int i = 0; i < columns.Count; i++)
                if (!((TableView)grid.View).GroupedColumns.Contains(columns[i]))
                    if (columns[i].SortOrder != ColumnSortOrder.None)
                        ((DetailBand)report.Bands[BandKind.Detail]).SortFields.Add(
                            new GroupField(columns[i].FieldName,
                            columns[i].SortOrder == ColumnSortOrder.Ascending ? XRColumnSortOrder.Ascending : XRColumnSortOrder.Descending));
        }
        void InitFilters(GridControl grid) {
            report.FilterString = grid.FilterString;
        }
        void InitDetailsAndPageHeader(GridControl grid) {
            IList<GridColumn> groupedColumns = ((TableView)grid.View).GroupedColumns;

            int pagewidth = (int)(report.PageWidth - (report.Margins.Left + report.Margins.Right)) - groupedColumns.Count * subGroupOffset;
            List<ColumnInfo> columns = GetColumnsInfo(grid, pagewidth);
            if (CustomizeColumnsCollection != null)
                CustomizeColumnsCollection(report, new ColumnsCreationEventArgs(pagewidth) { ColumnsInfo = columns });

            report.Bands.Add(new DetailBand() { HeightF = bandHeight });
            report.Bands.Add(new PageHeaderBand() { HeightF = bandHeight });

            XRTable headerTable = new XRTable();
            XRTableRow row = new XRTableRow();
            XRTable detailTable = new XRTable();
            XRTableRow row2 = new XRTableRow();

            for (int i = 0; i < columns.Count; i++) if (columns[i].IsVisible) {
                    XRTableCell cell = new XRTableCell();
                    cell.Width = columns[i].ColumnWidth;
                    cell.Text = columns[i].FieldName;
                    row.Cells.Add(cell);

                    XRTableCell cell2 = new XRTableCell();
                    cell2.Width = columns[i].ColumnWidth;
                    ControlCustomizationEventArgs cc = new ControlCustomizationEventArgs() {
                        FieldName = columns[i].FieldName,
                        IsModified = false,
                        Owner = cell2
                    };
                    if (CustomizeColumn != null)
                        CustomizeColumn(report, cc);
                    if (cc.IsModified == false)
                        cell2.DataBindings.Add("Text", null, columns[i].FieldName);
                    detailsInfo.Add(columns[i].GridViewColumn, cell2);
                    row2.Cells.Add(cell2);
                }
            headerTable.Rows.Add(row);
            headerTable.Width = pagewidth;
            headerTable.LocationF = new PointF(groupedColumns.Count * subGroupOffset, 0);
            headerTable.Borders = BorderSide.Bottom;

            detailTable.Rows.Add(row2);
            detailTable.LocationF = new PointF(groupedColumns.Count * subGroupOffset, 0);
            detailTable.Width = pagewidth;

            report.Bands[BandKind.PageHeader].Controls.Add(headerTable);
            report.Bands[BandKind.Detail].Controls.Add(detailTable);
        }

        private List<ColumnInfo> GetColumnsInfo(GridControl grid, int pagewidth) {
            List<ColumnInfo> columns = new List<ColumnInfo>();
            IList<GridColumn> visibleColumns = GetVisibleDataColumns(grid);
            foreach (GridColumn dataColumn in visibleColumns) {
                ColumnInfo column = new ColumnInfo(dataColumn) {
                    ColumnCaption = string.IsNullOrEmpty(dataColumn.HeaderCaption.ToString()) ? dataColumn.FieldName : dataColumn.HeaderCaption.ToString(),
                    ColumnWidth = (pagewidth / visibleColumns.Count),
                    FieldName = dataColumn.FieldName,
                    IsVisible = true
                };
                columns.Add(column);
            }
            return columns;

        }
        IList<GridColumn> GetVisibleDataColumns(GridControl grid) {
            return ((TableView)grid.View).VisibleColumns;
        }
        private SummaryFunc GetSummaryFunc(SummaryItemType summaryItemType) {
            switch (summaryItemType) {
                case SummaryItemType.Sum:
                    return SummaryFunc.Sum;
                case SummaryItemType.Average:
                    return SummaryFunc.Avg;
                case SummaryItemType.Max:
                    return SummaryFunc.Max;
                case SummaryItemType.Min:
                    return SummaryFunc.Min;
                default:
                    return SummaryFunc.Custom;
            }
        }
    }

    public class ControlCustomizationEventArgs : EventArgs {
        public XRControl Owner { get; set; }
        public bool IsModified { get; set; }
        public string FieldName { get; set; }
    }

    public class ColumnsCreationEventArgs : EventArgs {
        public ColumnsCreationEventArgs(int pageWidth) {
            this.PageWidth = pageWidth;
        }
        public int PageWidth { get; private set; }
        public List<ColumnInfo> ColumnsInfo { get; set; }
    }

    public class ColumnInfo {
        public ColumnInfo(GridColumn gridViewColumn) {
            this.gridViewColumn = gridViewColumn;
        }
        GridColumn gridViewColumn;
        public GridColumn GridViewColumn {
            get { return gridViewColumn; }
        }
        public string ColumnCaption { get; set; }
        public string FieldName { get; set; }
        public int ColumnWidth { get; set; }
        public bool IsVisible { get; set; }
    }
}