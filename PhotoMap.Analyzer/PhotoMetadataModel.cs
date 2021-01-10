using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PhotoMap.Analyzer
{
    public class PhotoMetadataModel
    {
        private static NumberFormatInfo _format = new NumberFormatInfo() { NumberDecimalSeparator = "." };

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string FileName { get; set; }
        public DateTime? PhotoTaken { get; set; }
        public List<string> AnalysisErrors { get; private set; }
        public Guid Id { get; private set; }

        public PhotoMetadataModel()
        {
            AnalysisErrors = new List<string>();
            Id = Guid.NewGuid();
        }

        public bool HasGpsData
        {
            get
            {
                return Latitude.HasValue && Longitude.HasValue;
            }
        }

        public string FormatedLatitude {  get
            {
                return Latitude.HasValue ? Latitude.Value.ToString(_format) : "";
            } 
        }

        public string FormatedLongitude
        {
            get
            {
                return Longitude.HasValue ? Longitude.Value.ToString(_format) : "";
            }
        }

    }
}
