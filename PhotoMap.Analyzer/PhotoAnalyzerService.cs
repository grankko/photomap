using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace PhotoMap.Analyzer
{
    public class PhotoAnalyzerService
    {
        public List<PhotoMetadataModel> Result { get; set; }

        public void ScanDirectory(string path)
        {
            // Todo: run on a background thread, populate as results come in

            Result = new List<PhotoMetadataModel>();
            var imageFiles = System.IO.Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories);
            foreach (var imageFile in imageFiles)
            {
                var newMetadata = new PhotoMetadataModel();
                newMetadata.FileName = imageFile;

                try
                {

                    var metadata = ImageMetadataReader.ReadMetadata(imageFile);

                    ReadGpsInformation(newMetadata, metadata);
                    ReadSubIfInformation(newMetadata, metadata);

                }
                catch (Exception ex)
                {
                    newMetadata.AnalysisErrors.Add($"Unable to read file: {ex.Message}");
                }

                Result.Add(newMetadata);
            }
        }

        private static void ReadSubIfInformation(PhotoMetadataModel newMetadata, IReadOnlyList<MetadataExtractor.Directory> metadata)
        {
            try
            {
                var subIf = metadata.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (subIf != null)
                {
                    DateTime photoTaken;
                    if (subIf.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out photoTaken))
                        newMetadata.PhotoTaken = photoTaken;
                }
            }
            catch
            {
                newMetadata.AnalysisErrors.Add("Unable to read SubIf information");
            }

        }

        private static void ReadGpsInformation(PhotoMetadataModel newMetadata, IReadOnlyList<MetadataExtractor.Directory> metadata)
        {
            try
            {
                var gps = metadata.OfType<GpsDirectory>().FirstOrDefault();
                if (gps != null)
                {
                    var location = gps.GetGeoLocation();
                    if (location != null)
                    {
                        newMetadata.Longitude = location.Longitude;
                        newMetadata.Latitude = location.Latitude;
                    }
                }
            }
            catch
            {
                newMetadata.AnalysisErrors.Add("Unable to read Gps information");
            }
        }
    }
}
