Imports System
Imports System.Drawing
Imports System.Collections
Imports System.Collections.Generic
Imports DevExpress.Data
Imports DevExpress.Xpf.Grid
Imports DevExpress.XtraPrinting
Imports DevExpress.XtraReports.UI

Namespace ReportHelper

    Public Delegate Sub CustomizeColumnsCollectionEventHandler(ByVal source As Object, ByVal e As ReportHelper.ColumnsCreationEventArgs)

    Public Delegate Sub CustomizeColumnEventHandler(ByVal source As Object, ByVal e As ReportHelper.ControlCustomizationEventArgs)

    Public Class ReportGeneratonHelper

        Private report As DevExpress.XtraReports.UI.XtraReport

        Const initialGroupOffset As Integer = 0

        Const subGroupOffset As Integer = 10

        Const bandHeight As Integer = 20

        Const shouldRepeatGroupHeadersOnEveryPage As Boolean = False

        Private detailsInfo As System.Collections.Hashtable = New System.Collections.Hashtable()

        Public Event CustomizeColumnsCollection As ReportHelper.CustomizeColumnsCollectionEventHandler

        Public Event CustomizeColumn As ReportHelper.CustomizeColumnEventHandler

        Public Function GenerateReport(ByVal grid As DevExpress.Xpf.Grid.GridControl, ByVal aspDataSource As Object) As XtraReport
            Me.report = New DevExpress.XtraReports.UI.XtraReport()
            Me.report.Landscape = True
            Me.report.PaperKind = System.Drawing.Printing.PaperKind.Letter
            Me.InitDataSource(aspDataSource)
            Me.InitDetailsAndPageHeader(grid)
            Me.InitSortings(grid)
            Me.InitGroupHeaders(grid)
            Me.InitFilters(grid)
            Me.InitTotalSummaries(grid)
            Return Me.report
        End Function

        Private Sub InitTotalSummaries(ByVal grid As DevExpress.Xpf.Grid.GridControl)
            If grid.TotalSummary.Count > 0 Then
                Me.report.Bands.Add(New DevExpress.XtraReports.UI.ReportFooterBand() With {.HeightF = ReportHelper.ReportGeneratonHelper.bandHeight})
                For Each item As DevExpress.Xpf.Grid.GridSummaryItem In grid.TotalSummary
                    Dim col As DevExpress.Xpf.Grid.GridColumn = grid.Columns(If(Equals(item.ShowInColumn, String.Empty), item.FieldName, item.ShowInColumn))
                    If col IsNot Nothing Then
                        If Me.detailsInfo.Contains(col) Then
                            Dim label As DevExpress.XtraReports.UI.XRLabel = New DevExpress.XtraReports.UI.XRLabel()
                            label.LocationF = CType(Me.detailsInfo(CObj((col))), DevExpress.XtraReports.UI.XRTableCell).LocationF
                            label.SizeF = CType(Me.detailsInfo(CObj((col))), DevExpress.XtraReports.UI.XRTableCell).SizeF
                            label.DataBindings.Add("Text", Nothing, col.FieldName)
                            label.Summary = New DevExpress.XtraReports.UI.XRSummary() With {.Running = DevExpress.XtraReports.UI.SummaryRunning.Report}
                            label.Summary.FormatString = item.DisplayFormat
                            label.Summary.Func = Me.GetSummaryFunc(item.SummaryType)
                            Me.report.Bands(CType((DevExpress.XtraReports.UI.BandKind.ReportFooter), DevExpress.XtraReports.UI.BandKind)).Controls.Add(label)
                        End If
                    End If
                Next
            End If
        End Sub

        Private Sub InitDataSource(ByVal DataSource As Object)
            Me.report.DataSource = DataSource
        End Sub

        Private Sub InitGroupHeaders(ByVal grid As DevExpress.Xpf.Grid.GridControl)
            For i As Integer = CType(grid.View, DevExpress.Xpf.Grid.TableView).GroupedColumns.Count - 1 To 0 Step -1
                Dim groupedColumn As DevExpress.Xpf.Grid.GridColumn = CType(grid.View, DevExpress.Xpf.Grid.TableView).GroupedColumns(i)
                Dim gb As DevExpress.XtraReports.UI.GroupHeaderBand = New DevExpress.XtraReports.UI.GroupHeaderBand()
                gb.Height = ReportHelper.ReportGeneratonHelper.bandHeight
                Dim l As DevExpress.XtraReports.UI.XRLabel = New DevExpress.XtraReports.UI.XRLabel()
                l.Text = groupedColumn.FieldName & ": [" & groupedColumn.FieldName & "]"
                l.LocationF = New System.Drawing.PointF(ReportHelper.ReportGeneratonHelper.initialGroupOffset + i * 10, 0)
                l.BackColor = System.Drawing.Color.Beige
                l.SizeF = New System.Drawing.SizeF((Me.report.PageWidth - (Me.report.Margins.Left + Me.report.Margins.Right)) - (ReportHelper.ReportGeneratonHelper.initialGroupOffset + i * ReportHelper.ReportGeneratonHelper.subGroupOffset), ReportHelper.ReportGeneratonHelper.bandHeight)
                gb.Controls.Add(l)
                gb.RepeatEveryPage = ReportHelper.ReportGeneratonHelper.shouldRepeatGroupHeadersOnEveryPage
                Dim gf As DevExpress.XtraReports.UI.GroupField = New DevExpress.XtraReports.UI.GroupField(groupedColumn.FieldName, If(groupedColumn.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending, DevExpress.XtraReports.UI.XRColumnSortOrder.Ascending, DevExpress.XtraReports.UI.XRColumnSortOrder.Descending))
                gb.GroupFields.Add(gf)
                Me.report.Bands.Add(gb)
            Next
        End Sub

        Private Sub InitSortings(ByVal grid As DevExpress.Xpf.Grid.GridControl)
            Dim columns As System.Collections.Generic.IList(Of DevExpress.Xpf.Grid.GridColumn) = Me.GetVisibleDataColumns(grid)
            For i As Integer = 0 To columns.Count - 1
                If Not CType(grid.View, DevExpress.Xpf.Grid.TableView).GroupedColumns.Contains(columns(i)) Then
                    If columns(CInt((i))).SortOrder <> DevExpress.Data.ColumnSortOrder.None Then CType(Me.report.Bands(CType((DevExpress.XtraReports.UI.BandKind.Detail), DevExpress.XtraReports.UI.BandKind)), DevExpress.XtraReports.UI.DetailBand).SortFields.Add(New DevExpress.XtraReports.UI.GroupField(columns(CInt((i))).FieldName, If(columns(CInt((i))).SortOrder = DevExpress.Data.ColumnSortOrder.Ascending, DevExpress.XtraReports.UI.XRColumnSortOrder.Ascending, DevExpress.XtraReports.UI.XRColumnSortOrder.Descending)))
                End If
            Next
        End Sub

        Private Sub InitFilters(ByVal grid As DevExpress.Xpf.Grid.GridControl)
            Me.report.FilterString = grid.FilterString
        End Sub

        Private Sub InitDetailsAndPageHeader(ByVal grid As DevExpress.Xpf.Grid.GridControl)
            Dim groupedColumns As System.Collections.Generic.IList(Of DevExpress.Xpf.Grid.GridColumn) = CType(grid.View, DevExpress.Xpf.Grid.TableView).GroupedColumns
            Dim pagewidth As Integer =(Me.report.PageWidth - (Me.report.Margins.Left + Me.report.Margins.Right)) - groupedColumns.Count * ReportHelper.ReportGeneratonHelper.subGroupOffset
            Dim columns As System.Collections.Generic.List(Of ReportHelper.ColumnInfo) = Me.GetColumnsInfo(grid, pagewidth)
            RaiseEvent CustomizeColumnsCollection(Me.report, New ReportHelper.ColumnsCreationEventArgs(pagewidth) With {.ColumnsInfo = columns})
            Me.report.Bands.Add(New DevExpress.XtraReports.UI.DetailBand() With {.HeightF = ReportHelper.ReportGeneratonHelper.bandHeight})
            Me.report.Bands.Add(New DevExpress.XtraReports.UI.PageHeaderBand() With {.HeightF = ReportHelper.ReportGeneratonHelper.bandHeight})
            Dim headerTable As DevExpress.XtraReports.UI.XRTable = New DevExpress.XtraReports.UI.XRTable()
            Dim row As DevExpress.XtraReports.UI.XRTableRow = New DevExpress.XtraReports.UI.XRTableRow()
            Dim detailTable As DevExpress.XtraReports.UI.XRTable = New DevExpress.XtraReports.UI.XRTable()
            Dim row2 As DevExpress.XtraReports.UI.XRTableRow = New DevExpress.XtraReports.UI.XRTableRow()
            For i As Integer = 0 To columns.Count - 1
                If columns(CInt((i))).IsVisible Then
                    Dim cell As DevExpress.XtraReports.UI.XRTableCell = New DevExpress.XtraReports.UI.XRTableCell()
                    cell.Width = columns(CInt((i))).ColumnWidth
                    cell.Text = columns(CInt((i))).FieldName
                    row.Cells.Add(cell)
                    Dim cell2 As DevExpress.XtraReports.UI.XRTableCell = New DevExpress.XtraReports.UI.XRTableCell()
                    cell2.Width = columns(CInt((i))).ColumnWidth
                    Dim cc As ReportHelper.ControlCustomizationEventArgs = New ReportHelper.ControlCustomizationEventArgs() With {.FieldName = columns(CInt((i))).FieldName, .IsModified = False, .Owner = cell2}
                    RaiseEvent CustomizeColumn(Me.report, cc)
                    If cc.IsModified = False Then cell2.DataBindings.Add("Text", Nothing, columns(CInt((i))).FieldName)
                    Me.detailsInfo.Add(columns(CInt((i))).GridViewColumn, cell2)
                    row2.Cells.Add(cell2)
                End If
            Next

            headerTable.Rows.Add(row)
            headerTable.Width = pagewidth
            headerTable.LocationF = New System.Drawing.PointF(groupedColumns.Count * ReportHelper.ReportGeneratonHelper.subGroupOffset, 0)
            headerTable.Borders = DevExpress.XtraPrinting.BorderSide.Bottom
            detailTable.Rows.Add(row2)
            detailTable.LocationF = New System.Drawing.PointF(groupedColumns.Count * ReportHelper.ReportGeneratonHelper.subGroupOffset, 0)
            detailTable.Width = pagewidth
            Me.report.Bands(CType((DevExpress.XtraReports.UI.BandKind.PageHeader), DevExpress.XtraReports.UI.BandKind)).Controls.Add(headerTable)
            Me.report.Bands(CType((DevExpress.XtraReports.UI.BandKind.Detail), DevExpress.XtraReports.UI.BandKind)).Controls.Add(detailTable)
        End Sub

        Private Function GetColumnsInfo(ByVal grid As DevExpress.Xpf.Grid.GridControl, ByVal pagewidth As Integer) As List(Of ReportHelper.ColumnInfo)
            Dim columns As System.Collections.Generic.List(Of ReportHelper.ColumnInfo) = New System.Collections.Generic.List(Of ReportHelper.ColumnInfo)()
            Dim visibleColumns As System.Collections.Generic.IList(Of DevExpress.Xpf.Grid.GridColumn) = Me.GetVisibleDataColumns(grid)
            For Each dataColumn As DevExpress.Xpf.Grid.GridColumn In visibleColumns
                Dim column As ReportHelper.ColumnInfo = New ReportHelper.ColumnInfo(dataColumn) With {.ColumnCaption = If(String.IsNullOrEmpty(dataColumn.HeaderCaption.ToString()), dataColumn.FieldName, dataColumn.HeaderCaption.ToString()), .ColumnWidth =(pagewidth \ visibleColumns.Count), .FieldName = dataColumn.FieldName, .IsVisible = True}
                columns.Add(column)
            Next

            Return columns
        End Function

        Private Function GetVisibleDataColumns(ByVal grid As DevExpress.Xpf.Grid.GridControl) As IList(Of DevExpress.Xpf.Grid.GridColumn)
            Return CType(grid.View, DevExpress.Xpf.Grid.TableView).VisibleColumns
        End Function

        Private Function GetSummaryFunc(ByVal summaryItemType As DevExpress.Data.SummaryItemType) As SummaryFunc
            Select Case summaryItemType
                Case DevExpress.Data.SummaryItemType.Sum
                    Return DevExpress.XtraReports.UI.SummaryFunc.Sum
                Case DevExpress.Data.SummaryItemType.Average
                    Return DevExpress.XtraReports.UI.SummaryFunc.Avg
                Case DevExpress.Data.SummaryItemType.Max
                    Return DevExpress.XtraReports.UI.SummaryFunc.Max
                Case DevExpress.Data.SummaryItemType.Min
                    Return DevExpress.XtraReports.UI.SummaryFunc.Min
                Case Else
                    Return DevExpress.XtraReports.UI.SummaryFunc.Custom
            End Select
        End Function
    End Class

    Public Class ControlCustomizationEventArgs
        Inherits System.EventArgs

        Public Property Owner As XRControl

        Public Property IsModified As Boolean

        Public Property FieldName As String
    End Class

    Public Class ColumnsCreationEventArgs
        Inherits System.EventArgs

        Private _PageWidth As Integer

        Public Sub New(ByVal pageWidth As Integer)
            Me.PageWidth = pageWidth
        End Sub

        Public Property PageWidth As Integer
            Get
                Return _PageWidth
            End Get

            Private Set(ByVal value As Integer)
                _PageWidth = value
            End Set
        End Property

        Public Property ColumnsInfo As List(Of ReportHelper.ColumnInfo)
    End Class

    Public Class ColumnInfo

        Public Sub New(ByVal gridViewColumn As DevExpress.Xpf.Grid.GridColumn)
            Me.gridViewColumnField = gridViewColumn
        End Sub

        Private gridViewColumnField As DevExpress.Xpf.Grid.GridColumn

        Public ReadOnly Property GridViewColumn As GridColumn
            Get
                Return Me.gridViewColumnField
            End Get
        End Property

        Public Property ColumnCaption As String

        Public Property FieldName As String

        Public Property ColumnWidth As Integer

        Public Property IsVisible As Boolean
    End Class
End Namespace
