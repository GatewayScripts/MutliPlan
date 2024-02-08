using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESAPI_UITemplate.Models
{
    public class StructureModel
    {
        public string StructureId { get; set; }
        public string DicomType { get; set; }
        public string StructureCode { get; set; }
        public bool bIsEmpty { get; set; }
    }
}
