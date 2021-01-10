﻿using System;
using System.ComponentModel;
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
        public List<PhotoMetadataModel> Result { get; private set; }
        public string DirectoryPath { get; private set; }
        public int ImageFileCount { get; private set; }
        public int ImageFilesProcessed { get; private set; }

        public readonly BackgroundWorker Worker = new BackgroundWorker();

        private IEnumerable<string> _imageFiles;

        public PhotoAnalyzerService()
        {
            Worker.DoWork += AnalyzeFiles;
            Worker.WorkerReportsProgress = true;
        }

        private void AnalyzeFiles(object sender, DoWorkEventArgs e)
        {
            foreach (var imageFile in _imageFiles)
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
                ImageFilesProcessed++;
                var percentCompleted = (int)(ImageFilesProcessed / ImageFileCount)*100;
                Worker.ReportProgress(percentCompleted, newMetadata);
            }
        }

        public void StartAnalysis()
        {
            Worker.RunWorkerAsync();
        }

        public void ScanDirectory(string path)
        {
            DirectoryPath = path;
            Result = new List<PhotoMetadataModel>();
            _imageFiles = System.IO.Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories); // todo: file formats
            ImageFileCount = _imageFiles.Count();
            ImageFilesProcessed = 0;
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
