using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Core;
using System.Globalization;
using DevExpress.Xpf.Editors.Settings;
using System.IO;
using System.Data;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Filtering;
using DXGrid_AssignComboBoxToColumn;
using DevExpress.Data.Filtering.Helpers;
using System.Collections;
using System.Windows.Markup;
using System.Windows.Threading;
using DevExpress.Xpf.Bars;
using System.Diagnostics;
using DevExpress.Data.Linq;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.Xpf.Grid.LookUp;
using System.Reflection;
using DevExpress.XtraReports.UI;

namespace DevExpressGrid_Flashing
{
    /// <summary>
    /// Interaction logic for Window17.xaml
    /// </summary>

    public partial class Window17 : Window
    {
        ObservableCollection<MyObj> coll;
        public Window17()
        {
            InitializeComponent();
            coll = new ObservableCollection<MyObj>();
            coll.Add(new MyObj() { Text = "n0", Number = 1, Group = "A", MyDateTime = DateTime.Now });
            coll.Add(new MyObj() { Text = "n1", Number = 123456789.12, Group = "A", MyDateTime = DateTime.Now });
            coll.Add(new MyObj() { Text = "n2", Number = 123456.78, Group = "B", MyBool = true, MyDateTime = DateTime.Now });
            coll.Add(new MyObj() { Text = "n3", Number = 1245.56, Group = "B", MyBool = false, MyDateTime = DateTime.Now });
            grid.ItemsSource = coll;
            grid.View.FocusedRowHandle = 2;
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ReportHelper.ReportGeneratonHelper helper = new ReportHelper.ReportGeneratonHelper();
            XtraReport report = helper.GenerateReport(grid, grid.ItemsSource);
            report.CreateDocument(false);
            DocumentPreviewWindow dp = new DocumentPreviewWindow() { Owner = this };
            dp.Model = new XtraReportPreviewModel(report);
            dp.ShowDialog();
        }

    }
  
    public class MyObj
    {
        private bool _MyBool;
        public bool MyBool
        {
            get { return _MyBool; }
            set
            {
                _MyBool = value;
            }
        }

        private string _String;
        public string Text
        {
            get { return _String; }
            set
            {
                _String = value;
            }
        }
        private double _Number;
        public double Number
        {
            get { return _Number; }
            set
            {
                _Number = value;
            }
        }
        private string _Group;
        public string Group
        {
            get { return _Group; }
            set
            {
                _Group = value;
            }
        }
        private DateTime _MyDateTime;
        public DateTime MyDateTime
        {
            get { return _MyDateTime; }
            set
            {
                _MyDateTime = value;
            }
        }
        public DateTime MyTime
        {
            get { return _MyDateTime; }
            set
            {
                _MyDateTime = value;
            }
        }



    }

}
