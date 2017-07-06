using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
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

namespace RelatedTableDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServiceFeatureTable serviceRequestTable;
        ServiceFeatureTable relatedTable;
        FeatureLayer serviceRequestLayer;

        public MainWindow()
        {
            InitializeComponent();
            CreateFeatureTable();
        }

        private async void CreateFeatureTable()
        {
            var featureTableUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/ServiceRequest/FeatureServer/0");
            serviceRequestTable = new ServiceFeatureTable(featureTableUri);

            var relatedTableUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/ServiceRequest/FeatureServer/1");
            relatedTable = new ServiceFeatureTable(relatedTableUri);

            // load the feature table
            await serviceRequestTable.LoadAsync();
            await relatedTable.LoadAsync();


            // if the table was loaded successfully, create a new feature layer for the table and add it to the map
            if (serviceRequestTable.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                serviceRequestLayer = new FeatureLayer(serviceRequestTable);
                MyMapView.Map.OperationalLayers.Add(serviceRequestLayer);
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs mapClickPoint)
        {

            IdentifyLayerResult idResult = await MyMapView.IdentifyLayerAsync(serviceRequestLayer, mapClickPoint.Position, 5, false);
            ArcGISFeature serviceRequestFeature = idResult.GeoElements.FirstOrDefault() as ArcGISFeature;
            if (serviceRequestFeature == null) { return; }
            
            // Create a new feature in the comments table
            ArcGISFeature newComment = relatedTable.CreateFeature() as ArcGISFeature;


            // Add comment text to the 'comments' attribute
            newComment.Attributes["comments"] = "Please show up on time!";


            // Relate the selected service request to the new comment
            serviceRequestFeature.RelateFeature(newComment);
            await relatedTable.AddFeatureAsync(newComment);
            var results = await relatedTable.ApplyEditsAsync();
        }

        // Map initialization logic is contained in MapViewModel.cs
    }
}
