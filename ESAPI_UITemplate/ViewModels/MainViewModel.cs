using ESAPI_UITemplate.Events;
using ESAPI_UITemplate.Models;
using ESAPI_UITemplate.Services;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ESAPI_UITemplate.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private EsapiWorker _esapiWorker;
        public List<StructureModel> Structures { get; private set; }
        public string PatientId { get; set; }
        public string StructureSetId { get; set; }
        private string _filePath;

        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }
        private double _progressValue;

        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }
        private string _status;

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status,value); }
        }
        private IEventAggregator _eventAggegrator;
        private double patientCounter;

        public DelegateCommand OpenPatientFileCommand { get; set; }
        public DelegateCommand LaunchPlanCommand { get; set; }
        public MainViewModel(EsapiWorker esapiWorker)
        {
            _esapiWorker = esapiWorker;
            _eventAggegrator = new EventAggregator();
            AutoPlanningService.EventAggregator = _eventAggegrator;
            Structures = new List<StructureModel>();
            GetHeaderInfo();
            GetStructuresFromStructureSet();

            OpenPatientFileCommand = new DelegateCommand(OnOpenPatientFile);
            LaunchPlanCommand = new DelegateCommand(OnLaunchPlan);
            _eventAggegrator.GetEvent<ProgressUpdateEvent>().Subscribe(OnUpdateProgress);
            _eventAggegrator.GetEvent<StatusUpdateEvent>().Subscribe(OnUpdateStatus);
        }

        private void OnUpdateStatus(string obj)
        {
            Status = obj;
        }

        private void OnUpdateProgress(double obj)
        {
            ProgressValue += obj / patientCounter;
        }

        private void OnLaunchPlan()
        {
            //read the patients. 
            //Change the patient context for that patient.
            //trigger the planning. 
            List<Tuple<string, string, string>> patientList = new List<Tuple<string, string, string>>();
            foreach (var line in File.ReadAllLines(FilePath))//file in System.IO
            {
                patientList.Add(new Tuple<string, string, string>
                    (line.Split(';').First().TrimStart('"'), line.Split(';').ElementAt(1), line.Split(';').Last().TrimEnd('"')));
            }
            patientCounter = (double)patientList.Count();
            int planCounter = 0;
            _esapiWorker.Run(xapp =>
            {
                foreach (var plan in patientList)
                {
                    AutoPlanningService.OpenPatientContext(plan.Item1);
                    AutoPlanningService.CopyBeamFromPlan(plan.Item2, plan.Item3);
                    //ProgressValue += 1.0 / patientCounter * 100;
                }
            });
        }

        private void OnOpenPatientFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open Patients to plan";
            ofd.Filter = "Text file (*.txt)|*.txt";
            if (ofd.ShowDialog() == true)
            {
                FilePath = ofd.FileName;
            }
        }

        /// <summary>
        /// You can either access ESAPI objects from inside the viewmodel
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void GetHeaderInfo()
        {
            _esapiWorker.Run(xapp =>
            {
                PatientId = AutoPlanningService.Patient.Id;
                StructureSetId = AutoPlanningService.StructureSet.Id;
            });
        }
        /// <summary>
        /// Or you can access ESAPI data through a separate service and keep all ESAPI calls in a designated area. 
        /// </summary>
        private void GetStructuresFromStructureSet()
        {
            Structures = AutoPlanningService.GetStructuresFromStructureSet();
        }
    }
}
