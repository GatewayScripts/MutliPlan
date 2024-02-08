using ESAPI_UITemplate.Services;
using ESAPI_UITemplate.ViewModels;
using ESAPI_UITemplate.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VMS.TPS.Common.Model.API;
using Esapi = VMS.TPS.Common.Model.API;

[assembly: ESAPIScript(IsWriteable = true)]
namespace ESAPI_UITemplate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private string _patientId;
        private string _structureSetId;
        private string _imageId;
        private Esapi.Application esapiApplication;
        private Esapi.Patient _patient;
        private Esapi.StructureSet _structureSet;
        private EsapiWorker esapiWorker;
        private MainView mainView;
        private MainViewModel mainViewModel;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Logger.Debug("Test");
            try
            {
                if (e.Args.Any())
                {
                    _patientId = e.Args.First().Split(';').First();
                    _structureSetId = e.Args.First().Split(';').Count()>1?
                        e.Args.First().Split(';').ElementAt(1):String.Empty;
                    _imageId = e.Args.First().Split(';').Count() > 2 ?
                        e.Args.First().Split(';').Last() : String.Empty;
                    using(esapiApplication = Esapi.Application.CreateApplication())
                    {
                        _patient = esapiApplication.OpenPatientById(_patientId);
                        if(_patient == null)
                        {
                            MessageBox.Show($"Unable to open patient {_patientId}");
                            Shutdown();
                        }
                        Logger.Debug($"Patient {_patient.Name} accessed by {esapiApplication.CurrentUser.Id}");
                        _structureSet = String.IsNullOrEmpty(_structureSetId) || String.IsNullOrEmpty(_imageId) ?
                            null : _patient.StructureSets.FirstOrDefault(ss => ss.Id == _structureSetId && ss.Image.Id == _imageId);
                        esapiWorker = new EsapiWorker(esapiApplication);
                        AutoPlanningService.EsapiWorker = esapiWorker;
                        AutoPlanningService.StructureSet = _structureSet;
                        AutoPlanningService.Patient = _patient;
                        // This new queue of tasks will prevent the script
                        // for exiting until the new window is closed
                        DispatcherFrame frame = new DispatcherFrame();


                        RunOnNewStaThread(() =>
                        {
                            // This method won't return until the window is closed
                            InitializeAndStartMainWindow();

                            // End the queue so that the script can exit
                            frame.Continue = false;
                        });

                        // Start the new queue, waiting until the window is closed
                        Dispatcher.PushFrame(frame);
                        esapiApplication.ClosePatient();
                        Shutdown();
                    }
                }
                else
                {
                    MessageBox.Show("No patient input");
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
            }
        }
        private void RunOnNewStaThread(Action a)
        {
            Thread thread = new Thread(() => a());
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }
        private void InitializeAndStartMainWindow()
        {
            mainView = new MainView();
            mainViewModel = new MainViewModel(esapiWorker);
            mainView.DataContext = mainViewModel;
            mainView.Closing += MainView_Closing;
            mainView.ShowDialog();
        }
        private void MainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!AutoPlanningService.SavedModifications)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("You have not saved modification. Discard changes?", "Do you want to exit?", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

    }
}
