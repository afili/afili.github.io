using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.Geometry;
using System.Diagnostics;
using Esri.ArcGISRuntime.Security;

namespace OfflineMapTaskDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PortalItem webmapItem;
        public MainWindow()
        {
            InitializeComponent();
        }


        private async void OfflineMapTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string pathToOutputPackage = @"C:\My Documents\Readiness\Trainings\Runtime 100.1\Demos\OfflineMap";
            // Create a mobile map package from the offline map
            MobileMapPackage offlineMapPackage = await MobileMapPackage.OpenAsync(pathToOutputPackage);
            
            // Get the map from the package and set it to the MapView
            var map = offlineMapPackage.Maps.First();
            MyMapView.Map = map;
        }


    }
}
