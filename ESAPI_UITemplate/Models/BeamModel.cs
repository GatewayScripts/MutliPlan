using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ESAPI_UITemplate.Models
{
    public class BeamModel
    {
        public string Id { get; set; }
        public string Energy { get; set; }
        public string Machine { get; set; }
        public VVector Isocenter { get; set; }
        public int DoseRate { get; set; }
        public string Technique { get; set; }
        public string PrimaryFluenceMode { get; set; }
        public double CollimatorAngle { get; set; }
        public double CouchAngle { get; set; }
        public double GantryAngle { get; set; }
        public double GantryStop{ get; set; }
        public GantryDirection GantryDirection { get; set; }
        public List<ControlPointModel> ControlPoints { get; set; }
        public MLCPlanType MLCTechnique { get; set; }
        public MetersetValue MU { get; set; }
        public double FieldWeight { get; set; }
        public BeamModel(Beam beam)
        {
            ControlPoints = new List<ControlPointModel>();
            Id = beam.Id;
            Energy = beam.EnergyModeDisplayName.Contains("-") ? beam.EnergyModeDisplayName.Split('-').First() :
                beam.EnergyModeDisplayName;
            Machine = beam.TreatmentUnit.Id;
            Isocenter = new VVector(beam.IsocenterPosition.x, beam.IsocenterPosition.y, beam.IsocenterPosition.z);
            DoseRate = beam.DoseRate;
            Technique = beam.Technique.Id;
            MU = beam.Meterset;
            MLCTechnique = beam.MLCPlanType;
            FieldWeight = beam.WeightFactor;
            PrimaryFluenceMode = beam.EnergyModeDisplayName.Contains("-") ? beam.EnergyModeDisplayName.Split('-').Last() :
                null;
            CollimatorAngle = beam.ControlPoints.First().CollimatorAngle;
            GantryAngle = beam.ControlPoints.First().GantryAngle;
            CouchAngle = beam.ControlPoints.First().PatientSupportAngle;
            
            if (Technique.Contains("ARC"))
            {
                GantryStop = beam.ControlPoints.Last().GantryAngle;
                GantryDirection = beam.GantryDirection;
            }
            foreach(var cp in beam.ControlPoints)
            {
                ControlPointModel controlPointTemp = new ControlPointModel(cp);
                if (Technique.Contains("ARC"))
                {
                    controlPointTemp.GantryAngle = cp.GantryAngle;
                }
                ControlPoints.Add(controlPointTemp);
            }
        }
    }
}
