Imports System
Imports System.Drawing
Imports System.Collections
Imports System.Collections.Generic
Imports DevExpress.Data
Imports DevExpress.Xpf.Grid
Imports DevExpress.XtraPrinting
Imports DevExpress.XtraReports.UI

Namespace ReportHelper

    Public Delegate Sub CustomizeColumnsCollectionEventHandler(ByVal source As Object, ByVal e As ColumnsCreationEventArgs)
    Public Delegate Sub CustomizeColumnEventHandler(ByVal source As Object, ByVal e As ControlCustomizationEventArgs)

    Public Class ReportGeneratonHelper
        Private report As XtraReport
        Private Const initialGroupOffset As Integer = 0
        Private Const subGroupOffset As Integer = 10
        Private Const bandHeight As Integer = 20
        Private Const shouldRepeatGroupHeadersOnEveryPage As Boolean = False
        Private detailsInfo As New Hashtable()

        Public Event CustomizeColumnsCollection As CustomizeColumnsCollectionEventHandler
        Public Event CustomizeColumn As CustomizeColumnEventHandler


        Public Function GenerateReport(ByVal grid As GridControl, ByVal aspDataSource As Object) As XtraReport
            report = New XtraReport()
            report.Landscape = True
            report.PaperKind = System.Drawing.Printing.PaperKind.Letter

            InitDataSource(aspDataSource)
            InitDetailsAndPageHeader(grid)
            InitSortings(grid)
            InitGroupHeaders(grid)
            InitFilters(grid)
            InitTotalSummaries(grid)
            Return report
        End Function

        Private Sub InitTotalSummaries(ByVal grid As GridControl)
            If grid.TotalSummary.Count > 0 Then
                report.Bands.Add(New ReportFooterBand() With {.HeightF = bandHeight})
                For Each item As GridSummaryItem In grid.TotalSummary
                    Dim col As GridColumn = grid.Columns(If(item.ShowInColumn = String.Empty, item.FieldName, item.ShowInColumn))
                    If col IsNot Nothing Then
                        If detailsInfo.Contains(col) Then
                            Dim label As New XRLabel()
                            label.LocationF = DirectCast(detailsInfo(col), XRTableCell).LocationF
                            label.SizeF = DirectCast(detailsInfo(col), XRTableCell).SizeF
                            label.DataBindings.Add("Text", Nothing, col.FieldName)
                            label.Summary = New XRSummary() With {.Running = SummaryRunning.Report}
                            label.Summary.FormatString = item.DisplayFormat
                            label.Summary.Func = GetSummaryFunc(item.SummaryType)
                            report.Bands(BandKind.ReportFooter).Controls.Add(label)
                        End If
                    End If
                Next item
            End If
        End Sub

        Private Sub InitDataSource(ByVal DataSource As Object)
            report.DataSource = DataSource
        End Sub

        Private Sub InitGroupHeaders(ByVal grid As GridControl)
            For i As Integer = CType(grid.View, TableView).GroupedColumns.Count - 1 To 0 Step -1
                Dim groupedColumn As GridColumn = CType(grid.View, TableView).GroupedColumns(i)
                Dim gb As New GroupHeaderBand()
                gb.Height = bandHeight
                Dim l As New XRLabel()
                l.Text = groupedColumn.FieldName & ": [" & groupedColumn.FieldName & "]"
                l.LocationF = New PointF(initialGroupOffset + i * 10, 0)
                l.BackColor = Color.Beige
                l.SizeF = New SizeF((report.PageWidth - (report.Margins.Left + report.Margins.Right)) - (initialGroupOffset + i * subGroupOffset), bandHeight)
                gb.Controls.Add(l)
                gb.RepeatEveryPage = shouldRepeatGroupHeadersOnEveryPage
                Dim gf As New GroupField(groupedColumn.FieldName,If(groupedColumn.SortOrder = ColumnSortOrder.Ascending, XRColumnSortOrder.Ascending, XRColumnSortOrder.Descending))
                gb.GroupFields.Add(gf)
                report.Bands.Add(gb)
            Next i
        End Sub
        Private Sub InitSortings(ByVal grid As GridControl)
            Dim columns As IList(Of GridColumn) = GetVisibleDataColumns(grid)
            For i As Integer = 0 To columns.Count - 1
                If Not CType(grid.View, TableView).GroupedColumns.Contains(columns(i)) Then
                    If columns(i).SortOrder <> ColumnSortOrder.None Then
                        CType(report.Bands(BandKind.Detail), DetailBand).SortFields.Add(New GroupField(columns(i).FieldName,If(columns(i).SortOrder = ColumnSortOrder.Ascending, XRColumnSortOrder.Ascending, XRColumnSortOrder.Descending)))
                    End If
                End If
            Next i
        End Sub
        Private Sub InitFilters(ByVal grid As GridControl)
            report.FilterString = grid.FilterString
        End Sub
        Private Sub InitDetailsAndPageHeader(ByVal grid As GridControl)
            Dim groupedColumns As IList(Of GridColumn) = CType(grid.View, TableView).GroupedColumns

            Dim pagewidth As Integer = (report.PageWidth - (report.Margins.Left + report.Margins.Right)) - groupedColumns.Count * subGroupOffset
            Dim columns As List(Of ColumnInfo) = GetColumnsInfo(grid, pagewidth)
            RaiseEvent CustomizeColumnsCollection(report, New ColumnsCreationEventArgs(pagewidth) With {.ColumnsInfo = columns})

            report.Bands.Add(New DetailBand() With {.HeightF = bandHeight})
            report.Bands.Add(New PageHeaderBand() With {.HeightF = bandHeight})

            Dim headerTable As New XRTable()
            Dim row As New XRTableRow()
            Dim detailTable As New XRTable()
            Dim row2 As New XRTableRow()

            For i As Integer = 0 To columns.Count - 1
                If columns(i).IsVisible Then
                    Dim cell As New XRTableCell()
                    cell.Width = columns(i).ColumnWidth
                    cell.Text = columns(i).FieldName
                    row.Cells.Add(cell)

                    Dim cell2 As New XRTableCell()
                    cell2.Width = columns(i).ColumnWidth
                    Dim cc As New ControlCustomizationEventArgs() With {.FieldName = columns(i).FieldName, .IsModified = False, .Owner = cell2}
                    RaiseEvent CustomizeColumn(report, cc)
                    If cc.IsModified = False Then
                        cell2.DataBindings.Add("Text", Nothing, columns(i).FieldName)
                    End If
                    detailsInfo.Add(columns(i).GridViewColumn, cell2)
                    row2.Cells.Add(cell2)
                End If
            Next i
            headerTable.Rows.Add(row)
            headerTable.Width = pagewidth
            headerTable.LocationF = New PointF(groupedColumns.Count * subGroupOffset, 0)
            headerTable.Borders = BorderSide.Bottom

            detailTable.Rows.Add(row2)
            detailTable.LocationF = New PointF(groupedColumns.Count * subGroupOffset, 0)
            detailTable.Width = pagewidth

            report.Bands(BandKind.PageHeader).Controls.Add(headerTable)
            report.Bands(BandKind.Detail).Controls.Add(detailTable)
        End Sub

        Private Function GetColumnsInfo(ByVal grid As GridControl, ByVal pagewidth As Integer) As List(Of ColumnInfo)
            Dim columns As New List(Of ColumnInfo)()
            Dim visibleColumns As IList(Of GridColumn) = GetVisibleDataColumns(grid)
            For Each dataColumn As GridColumn In visibleColumns
                Dim column As New ColumnInfo(dataColumn) With {.ColumnCaption = If(String.IsNullOrEmpty(dataColumn.HeaderCaption.ToString()), dataColumn.FieldName, dataColumn.HeaderCaption.ToString()), .ColumnWidth = (pagewidth \ visibleColumns.Count), .FieldName = dataColumn.FieldName, .IsVisible = True}
                columns.Add(column)
            Next dataColumn
            Return columns

        End Function
        Private Function GetVisibleDataColumns(ByVal grid As GridControl) As IList(Of GridColumn)
            Return CType(grid.View, TableView).VisibleColumns
        End Function
        Private Function GetSummaryFunc(ByVal summaryItemType As SummaryItemType) As SummaryFunc
            Select Case summaryItemType
                Case SummaryItemType.Sum
                    Return SummaryFunc.Sum
                Case SummaryItemType.Average
                    Return SummaryFunc.Avg
                Case SummaryItemType.Max
                    Return SummaryFunc.Max
                Case SummaryItemType.Min
                    Return SummaryFunc.Min
                Case Else
                    Return SummaryFunc.Custom
            End Select
        End Function
    End Class

    Public Class ControlCustomizationEventArgs
        Inherits EventArgs

        Public Property Owner() As XRControl
        Public Property IsModified() As Boolean
        Public Property FieldName() As String
    End Class

    Public Class ColumnsCreationEventArgs
        Inherits EventArgs

        Public Sub New(ByVal pageWidth As Integer)
            Me.PageWidth = pageWidth
        End Sub
        Private privatePageWidth As Integer
        Public Property PageWidth() As Integer
            Get
                Return privatePageWidth
            End Get
            Private Set(ByVal value As Integer)
                privatePageWidth = value
            End Set
        End Property
        Public Property ColumnsInfo() As List(Of ColumnInfo)
    End Class

    Public Class ColumnInfo
        Public Sub New(ByVal gridViewColumn As GridColumn)
            Me.gridViewColumn_Renamed = gridViewColumn
        End Sub

        Private gridViewColumn_Renamed As GridColumn
        Public ReadOnly Property GridViewColumn() As GridColumn
            Get
                Return gridViewColumn_Renamed
            End Get
        End Property
        Public Property ColumnCaption() As String
        Public Property FieldName() As String
        Public Property ColumnWidth() As Integer
        Public Property IsVisible() As Boolean
    End Class
End Namespace