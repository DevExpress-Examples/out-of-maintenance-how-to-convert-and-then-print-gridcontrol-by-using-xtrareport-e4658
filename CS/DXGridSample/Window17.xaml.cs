using DevExpress.Xpf.Printing;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace DevExpressGrid_Flashing {
    public partial class Window17 : Window {
        ObservableCollection<MyObj> coll;
        public Window17() {
            InitializeComponent();
            coll = new ObservableCollection<MyObj>();
            coll.Add(new MyObj() { Text = "n0", Number = 1, Group = "A", MyDateTime = DateTime.Now });
            coll.Add(new MyObj() { Text = "n1", Number = 123456789.12, Group = "A", MyDateTime = DateTime.Now });
            coll.Add(new MyObj() { Text = "n2", Number = 123456.78, Group = "B", MyBool = true, MyDateTime = DateTime.Now });
            coll.Add(new MyObj() { Text = "n3", Number = 1245.56, Group = "B", MyBool = false, MyDateTime = DateTime.Now });
            grid.ItemsSource = coll;
            grid.View.FocusedRowHandle = 2;
        }
        private void button1_Click(object sender, RoutedEventArgs e) {
            ReportHelper.ReportGeneratonHelper helper = new ReportHelper.ReportGeneratonHelper();
            XtraReport report = helper.GenerateReport(grid, grid.ItemsSource);
            DocumentPreviewWindow dp = new DocumentPreviewWindow() { Owner = this };
            dp.PreviewControl.DocumentSource = report;
            report.CreateDocument(false);
            dp.ShowDialog();
        }
    }

    public class MyObj {
        public bool MyBool { get; set; }
        public string Text { get; set; }
        public double Number { get; set; }
        public string Group { get; set; }
        public DateTime MyDateTime { get; set; }
        public DateTime MyTime { get; set; }
    }
}
