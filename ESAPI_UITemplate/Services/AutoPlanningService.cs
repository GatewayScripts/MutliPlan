using ESAPI_UITemplate.Events;
using ESAPI_UITemplate.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ESAPI_UITemplate.Services
{
    public static class AutoPlanningService
    {
        public static EsapiWorker EsapiWorker;
        public static bool SavedModifications;
        public static StructureSet StructureSet;
        public static Patient Patient;
        public static IEventAggregator EventAggregator;

        internal static List<StructureModel> GetStructuresFromStructureSet()
        {
            List<StructureModel> structures = new List<StructureModel>();
            EsapiWorker.Run(xapp =>
            {
                foreach (var structure in StructureSet.Structures)
                {
                    structures.Add(new StructureModel
                    {
                        StructureId = structure.Id,
                        StructureCode = structure.StructureCode?.Code,
                        DicomType = structure.DicomType,
                        bIsEmpty = structure.IsEmpty
                    });
                }
            });
            return structures;
        }
        internal static void OpenPatientContext(string patientId)
        {
            EsapiWorker.RunWithWait(xapp =>
            {
                xapp.ClosePatient();
                Patient = xapp.OpenPatientById(patientId);
            });
        }
        internal static void CopyBeamFromPlan(string courseId, string planId)
        {
            EsapiWorker.Run(xapp =>
            {
                Course course = AutoPlanningService.Patient.Courses.FirstOrDefault(c => c.Id == courseId);//one
                ExternalPlanSetup plan = course.ExternalPlanSetups.FirstOrDefault(ps => ps.Id == planId);//two
                List<BeamModel> localBeams = new List<BeamModel>();
                foreach (var beam in plan.Beams.Where(b=>!b.IsSetupField))
                {
                    localBeams.Add(new BeamModel(beam));
                }
                //changes to localBeams goes here.
                AutoPlanningService.Patient.BeginModifications();
                ExternalPlanSetup newPlan = course.AddExternalPlanSetup(plan.StructureSet);//three
                newPlan.SetPrescription((int)plan.NumberOfFractions, plan.DosePerFraction, 1.0);
                ExternalBeamMachineParameters parameters = new ExternalBeamMachineParameters("TrueBeam",
                    "10X", localBeams.First().DoseRate, localBeams.First().Technique, null);
                List<KeyValuePair<string, MetersetValue>> mus = new List<KeyValuePair<string, MetersetValue>>();
                EventAggregator.GetEvent<StatusUpdateEvent>().Publish($"Copying Beams to {newPlan.Id}");
                EventAggregator.GetEvent<ProgressUpdateEvent>().Publish(25);
                foreach (var lbeam in localBeams)
                {
                    Beam b = null;
                    if (lbeam.MLCTechnique == MLCPlanType.DoseDynamic)
                    {
                         b = newPlan.AddSlidingWindowBeam(parameters,
                            lbeam.ControlPoints.Select(cp => cp.MetersetWeight),
                            lbeam.CollimatorAngle,
                            lbeam.GantryAngle,
                            lbeam.CouchAngle,
                            lbeam.Isocenter);
                        
                    }
                    if(lbeam.MLCTechnique == MLCPlanType.VMAT)
                    {
                         b = newPlan.AddVMATBeam(parameters,
                            lbeam.ControlPoints.Select(cp => cp.MetersetWeight),
                            lbeam.CollimatorAngle,
                            lbeam.GantryAngle,
                            lbeam.GantryStop,
                            lbeam.GantryDirection,
                            lbeam.CouchAngle,
                            lbeam.Isocenter);

                    }
                    mus.Add(new KeyValuePair<string, MetersetValue>(b.Id, lbeam.MU));
                    var edits = b.GetEditableParameters();
                    int cpnum = 0;
                    foreach (var cp in edits.ControlPoints)
                    {
                        cp.JawPositions = lbeam.ControlPoints.ElementAt(cpnum).JawPositions;
                        cp.LeafPositions = lbeam.ControlPoints.ElementAt(cpnum).MLCPositions;
                        cpnum++;
                    }
                    edits.WeightFactor = lbeam.FieldWeight;
                    b.ApplyParameters(edits);
                }
                newPlan.SetCalculationModel(CalculationType.PhotonVolumeDose, "AAA_1610"); 
                EventAggregator.GetEvent<StatusUpdateEvent>().Publish($"Calculating Dose");
                
                newPlan.CalculateDoseWithPresetValues(mus);
                EventAggregator.GetEvent<ProgressUpdateEvent>().Publish(50);
                EventAggregator.GetEvent<StatusUpdateEvent>().Publish($"Saving Plan {newPlan.Id} on patient {Patient.Id}");
                xapp.SaveModifications();
                EventAggregator.GetEvent<ProgressUpdateEvent>().Publish(25);
                //return $"Plan Generated: {newPlan.Id} with {newPlan.Beams.Count()} beams";
            });

        }
    }
}
