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

            CreateOnlineMap();
        }

        private async void CreateOnlineMap()
        {
            // Load map from a portal item
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();
            webmapItem = await PortalItem.CreateAsync(portal, "acc027394bc84c2fb04d1ed317aac674");

            // Create map and add it to the view
            MyMapView.Map = new Map(webmapItem);
        }

        private void OfflineMapTaskButton_Click(object sender, RoutedEventArgs e)
        {
            CreateOfflineMapTask();
        }

        private async void CreateOfflineMapTask()
        {
            AuthenticationManager.Current.ChallengeHandler = new Esri.ArcGISRuntime.Security.ChallengeHandler(CreateKnownCredentials);

            // Create task and set parameters
            OfflineMapTask task = await OfflineMapTask.CreateAsync(MyMapView.Map);

            GenerateOfflineMapParameters parameters = await GenerateGeodatabaseParameters(task, webmapItem);

            CanBeTakenOffline(task, parameters);
            GenerateOfflineMap(task, parameters);
        }

        private async Task<Credential> CreateKnownCredentials(CredentialRequestInfo info)
        {
            // If this isn't the expected resource, the credential will stay null
            Credential knownCredential = null;

            knownCredential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri, "afili_ess", "@pantelis2111", info.GenerateTokenOptions);
             return knownCredential;
        }

        private async Task<GenerateOfflineMapParameters> GenerateGeodatabaseParameters(OfflineMapTask task, PortalItem webmapItem)
        {
            // Create default parameters
            //Envelope areaOfInterest = new Envelope(-12211308.778729, 4645116.003309, -12208257.879667, 4650542.535773, SpatialReferences.WebMercator);
            //GenerateOfflineMapParameters parameters = await task.CreateDefaultGenerateOfflineMapParametersAsync(areaOfInterest);

            GenerateOfflineMapParameters parameters = await task.CreateDefaultGenerateOfflineMapParametersAsync(MyMapView.Map.InitialViewpoint.TargetGeometry.Extent);

            // Update the parameters if needed
            // Limit maximum scale to 5000 but take all the scales above (use 0 as a MinScale)
            //parameters.MaxScale = 5000;


            // Set attachment options
            parameters.AttachmentSyncDirection = AttachmentSyncDirection.Upload;
            parameters.ReturnLayerAttachmentOption = ReturnLayerAttachmentOption.EditableLayers;
            parameters.IncludeBasemap = true;

            // Request the table schema only (existing features won’t be included)
            parameters.ReturnSchemaOnlyForEditableLayers = false;


            // Update the title to contain the region 
            parameters.ItemInfo.Title = parameters.ItemInfo.Title + " (Central)";


            // Override thumbnail with the new image based on the extent
            RuntimeImage thumbnail = await MyMapView.ExportImageAsync();

            // Create new item info
            OfflineMapItemInfo itemInfo = new OfflineMapItemInfo();


            // Create new thumbnail from the map
            RuntimeImage thumbnailImage = await MyMapView.ExportImageAsync();


            // Set values to the itemInfo
            itemInfo.Thumbnail = thumbnailImage;
            itemInfo.Title = "Water network (Central)";
            itemInfo.Snippet = webmapItem.Snippet; // Copy from the source map
            itemInfo.Description = webmapItem.Description; // Copy from the source map
            itemInfo.AccessInformation = webmapItem.AccessInformation; // Copy from the source map
            itemInfo.Tags.Add("Water network");
            itemInfo.Tags.Add("Data validation");


            // Set metadata to parameters
            parameters.ItemInfo = itemInfo;

            return parameters;
        }

        private async void CanBeTakenOffline(OfflineMapTask task, GenerateOfflineMapParameters parameters)
        {
            OfflineMapCapabilities results = await task.GetOfflineMapCapabilitiesAsync(parameters);
            if (results.HasErrors)
            {
                // Handle possible errors with layers
                foreach (var layerCapability in results.LayerCapabilities)
                {
                    if (!layerCapability.Value.SupportsOffline)
                    {
                        Debug.WriteLine(layerCapability.Key.Name + " cannot be taken offline. Error : " + layerCapability.Value.Error.Message);
                    }
                }


                // Handle possible errors with tables
                foreach (var tableCapability in results.TableCapabilities)
                {
                    if (!tableCapability.Value.SupportsOffline)
                    {
                        Debug.WriteLine(tableCapability.Key.TableName + " cannot be taken offline. Error : " + tableCapability.Value.Error.Message);
                    }
                }
            }
            else
            {
                // All layers and tables can be taken offline!
                MessageBox.Show("All layers are good to go!");
                Debug.WriteLine("All layers are good to go!");
            }
        }

        private async void GenerateOfflineMap(OfflineMapTask task, GenerateOfflineMapParameters parameters)
        {
            string pathToOutputPackage = @"C:\My Documents\Readiness\Trainings\Runtime 100.1\Demos\OfflineMap";
            // Create a job and provide needed parameters
            GenerateOfflineMapJob job = task.GenerateOfflineMap(parameters, pathToOutputPackage);


            // Generate the offline map and download it 
            GenerateOfflineMapResult results = await job.GetResultAsync();


            if (!results.HasErrors)
            {
                // Job is finished and all content was generated
                Debug.WriteLine("Map " + results.MobileMapPackage.Item.Title + " saved to " + results.MobileMapPackage.Path);


                // Show offline map in a MapView
                MyMapView.Map = results.OfflineMap;
            }
            else
            {
                // Job is finished but some of the layers/tables had errors
                if (results.LayerErrors.Count > 0)
                {
                    foreach (var layerError in results.LayerErrors)
                    {
                        Debug.WriteLine("Error occurred when taking " + layerError.Key.Name + " offline. Error : " + layerError.Value.Message);
                    }
                }
                if (results.TableErrors.Count > 0)
                {
                    foreach (var tableError in results.TableErrors)
                    {
                        Debug.WriteLine("Error occurred when taking " + tableError.Key.TableName + " offline. Error : " + tableError.Value.Message);
                    }
                }
            }
        }

        
    }
}
