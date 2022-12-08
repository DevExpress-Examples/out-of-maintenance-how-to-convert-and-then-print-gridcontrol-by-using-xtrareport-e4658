Imports System.Windows
Imports System.Collections.ObjectModel
Imports DevExpress.Xpf.Printing
Imports DevExpress.XtraReports.UI

Namespace DevExpressGrid_Flashing

    Public Partial Class Window17
        Inherits Window

        Private coll As ObservableCollection(Of MyObj)

        Public Sub New()
            Me.InitializeComponent()
            coll = New ObservableCollection(Of MyObj)()
            coll.Add(New MyObj() With {.Text = "n0", .Number = 1, .Group = "A", .MyDateTime = Date.Now})
            coll.Add(New MyObj() With {.Text = "n1", .Number = 123456789.12, .Group = "A", .MyDateTime = Date.Now})
            coll.Add(New MyObj() With {.Text = "n2", .Number = 123456.78, .Group = "B", .MyBool = True, .MyDateTime = Date.Now})
            coll.Add(New MyObj() With {.Text = "n3", .Number = 1245.56, .Group = "B", .MyBool = False, .MyDateTime = Date.Now})
            Me.grid.ItemsSource = coll
            Me.grid.View.FocusedRowHandle = 2
        End Sub

        Private Sub button1_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim helper As ReportHelper.ReportGeneratonHelper = New ReportHelper.ReportGeneratonHelper()
            Dim report As XtraReport = helper.GenerateReport(Me.grid, Me.grid.ItemsSource)
            Dim dp As DocumentPreviewWindow = New DocumentPreviewWindow() With {.Owner = Me}
            dp.PreviewControl.DocumentSource = report
            report.CreateDocument(False)
            dp.ShowDialog()
        End Sub
    End Class

    Public Class MyObj

        Public Property MyBool As Boolean

        Public Property Text As String

        Public Property Number As Double

        Public Property Group As String

        Public Property MyDateTime As Date

        Public Property MyTime As Date
    End Class
End Namespace
