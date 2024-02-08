using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ESAPI_UITemplate.Models
{
    public class ControlPointModel
    {
        public float[,] MLCPositions { get; set; }
        public double MetersetWeight { get; set; }
        public VRect<double> JawPositions { get; set; }
        public double GantryAngle { get; set; }
        public ControlPointModel(ControlPoint controlPoint) 
        {
            MLCPositions = controlPoint.LeafPositions;
            MetersetWeight = controlPoint.MetersetWeight;
            JawPositions = new VRect<double>(controlPoint.JawPositions.X1,controlPoint.JawPositions.Y1,
                controlPoint.JawPositions.X2,controlPoint.JawPositions.Y2);

        }
    }
}
