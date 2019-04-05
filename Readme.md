<!-- default file list -->
*Files to look at*:

* [ReportHelper.cs](./CS/DXGridSample/ReportHelper.cs) (VB: [ReportHelper.vb](./VB/DXGridSample/ReportHelper.vb))
* [Window17.xaml](./CS/DXGridSample/Window17.xaml) (VB: [Window17.xaml](./VB/DXGridSample/Window17.xaml))
* [Window17.xaml.cs](./CS/DXGridSample/Window17.xaml.cs) (VB: [Window17.xaml.vb](./VB/DXGridSample/Window17.xaml.vb))
* [XtraReport1.cs](./CS/DXGridSample/XtraReport1.cs) (VB: [XtraReport1.vb](./VB/DXGridSample/XtraReport1.vb))
<!-- default file list end -->
# How to convert and then print GridControl by using XtraReport


<p>This example demonstrates how to dynamically create a report based on the GridControl at runtime. This means that all filtering, sorting and grouping conditions selected in the grid are also applied in the report. To accomplish this task, it is necessary to create a report with all the necessary bands, bind it to a data source, and adjust all the necessary options. You can use this approach if you need to insert a report based on GridControl into another report.<br><br></p>
<p><strong>[Update]<br></strong>Starting with version <strong>15.2</strong>, you can use <a href="https://documentation.devexpress.com/WPF/CustomDocument115300.aspx">GridReportManagerService</a> to convert and export <strong>GridControl </strong>by using <strong>XtraReports</strong>.</p>

<br/>


