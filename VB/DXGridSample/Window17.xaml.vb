Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Shapes
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports DevExpress.Xpf.Grid
Imports DevExpress.Xpf.Core
Imports System.Globalization
Imports DevExpress.Xpf.Editors.Settings
Imports System.IO
Imports System.Data
Imports DevExpress.Xpf.Editors
Imports DevExpress.Xpf.Editors.Filtering
Imports DXGrid_AssignComboBoxToColumn
Imports DevExpress.Data.Filtering.Helpers
Imports System.Collections
Imports System.Windows.Markup
Imports System.Windows.Threading
Imports DevExpress.Xpf.Bars
Imports System.Diagnostics
Imports DevExpress.Data.Linq
Imports DevExpress.Xpf.Printing
Imports DevExpress.Xpf.Core.Serialization
Imports DevExpress.XtraEditors.DXErrorProvider
Imports DevExpress.Xpf.Grid.LookUp
Imports System.Reflection
Imports DevExpress.XtraReports.UI

Namespace DevExpressGrid_Flashing
	''' <summary>
	''' Interaction logic for Window17.xaml
	''' </summary>

	Partial Public Class Window17
		Inherits Window
		Private coll As ObservableCollection(Of MyObj)
		Public Sub New()
			InitializeComponent()
			coll = New ObservableCollection(Of MyObj)()
			coll.Add(New MyObj() With {.Text = "n0", .Number = 1, .Group = "A", .MyDateTime = DateTime.Now})
			coll.Add(New MyObj() With {.Text = "n1", .Number = 123456789.12, .Group = "A", .MyDateTime = DateTime.Now})
			coll.Add(New MyObj() With {.Text = "n2", .Number = 123456.78, .Group = "B", .MyBool = True, .MyDateTime = DateTime.Now})
			coll.Add(New MyObj() With {.Text = "n3", .Number = 1245.56, .Group = "B", .MyBool = False, .MyDateTime = DateTime.Now})
			grid.ItemsSource = coll
			grid.View.FocusedRowHandle = 2
		End Sub
		Private Sub button1_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim helper As New ReportHelper.ReportGeneratonHelper()
			Dim report As XtraReport = helper.GenerateReport(grid, grid.ItemsSource)
			report.CreateDocument(False)
			Dim dp As New DocumentPreviewWindow() With {.Owner = Me}
			dp.Model = New XtraReportPreviewModel(report)
			dp.ShowDialog()
		End Sub

	End Class

	Public Class MyObj
		Private _MyBool As Boolean
		Public Property MyBool() As Boolean
			Get
				Return _MyBool
			End Get
			Set(ByVal value As Boolean)
				_MyBool = value
			End Set
		End Property

		Private _String As String
		Public Property Text() As String
			Get
				Return _String
			End Get
			Set(ByVal value As String)
				_String = value
			End Set
		End Property
		Private _Number As Double
		Public Property Number() As Double
			Get
				Return _Number
			End Get
			Set(ByVal value As Double)
				_Number = value
			End Set
		End Property
		Private _Group As String
		Public Property Group() As String
			Get
				Return _Group
			End Get
			Set(ByVal value As String)
				_Group = value
			End Set
		End Property
		Private _MyDateTime As DateTime
		Public Property MyDateTime() As DateTime
			Get
				Return _MyDateTime
			End Get
			Set(ByVal value As DateTime)
				_MyDateTime = value
			End Set
		End Property
		Public Property MyTime() As DateTime
			Get
				Return _MyDateTime
			End Get
			Set(ByVal value As DateTime)
				_MyDateTime = value
			End Set
		End Property



	End Class

End Namespace
