using ShellTemperature.Data;
using ShellTemperature.Models;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.ViewModels.DataManipulation
{
    public class ShellTemperatureRecordConvertion
    {
        #region Fields

        private readonly IShellTemperatureRepository<ShellTemp> _shellTemperatureRepository;
        private readonly IShellTemperatureRepository<SdCardShellTemp> _sdCardShellTemperatureRepository;
        private readonly IRepository<ShellTemperatureComment> _commentRepository;
        private readonly IRepository<SdCardShellTemperatureComment> _sdCardCommentRepository;
        private readonly IRepository<ShellTemperaturePosition> _shellTemperaturePositionRepository;
        #endregion

        #region Connstructors

        public ShellTemperatureRecordConvertion(
            IShellTemperatureRepository<ShellTemp> shellTemperatureRepository,
            IShellTemperatureRepository<SdCardShellTemp> sdCardShellTemperatureRepository,
            IRepository<ShellTemperatureComment> commentRepository,
            IRepository<SdCardShellTemperatureComment> sdCardCommentRepository,
            IRepository<ShellTemperaturePosition> shellTemperaturePositionRepository)
        {
            _shellTemperatureRepository = shellTemperatureRepository;
            _sdCardShellTemperatureRepository = sdCardShellTemperatureRepository;
            _commentRepository = commentRepository;
            _sdCardCommentRepository = sdCardCommentRepository;
            _shellTemperaturePositionRepository = shellTemperaturePositionRepository;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Get all the live and sd card shell temperatures along with
        /// the comments and positions
        /// </summary>
        /// <returns></returns>
        public ShellTemperatureRecord[] GetShellTemperatureRecords(DateTime start, DateTime end, DeviceInfo deviceInfo = null)
        {
            ShellTemp[] tempData;
            SdCardShellTemp[] sdCardShellTemps;

            // Has device information, user selected device
            if (deviceInfo != null)
            {
                // Get live data and live data comments and positions
                tempData = _shellTemperatureRepository.GetShellTemperatureData(start, end,
                    deviceInfo.DeviceName, deviceInfo.DeviceAddress).ToArray();

                // Get SD Card data and SD card data comments
                sdCardShellTemps = _sdCardShellTemperatureRepository.GetShellTemperatureData(start, end,
                    deviceInfo.DeviceName, deviceInfo.DeviceAddress).ToArray();
            }
            else // No device information, just use dates
            {
                // Get live data and live data comments and positions
                tempData = _shellTemperatureRepository.GetShellTemperatureData(start, end).ToArray();

                // Get SD Card data and SD card data comments
                sdCardShellTemps = _sdCardShellTemperatureRepository.GetShellTemperatureData(start, end).ToArray();
            }

            // Get the live comments
            ShellTemperatureComment[] liveDataComments = _commentRepository.GetAll()
                .Where(x => x.ShellTemp.RecordedDateTime >= start
                            && x.ShellTemp.RecordedDateTime <= end)
                .ToArray();

            // Get the live positions
            ShellTemperaturePosition[] positions = _shellTemperaturePositionRepository.GetAll().ToArray();

            // Sd card comments
            SdCardShellTemperatureComment[] sdCardComments = _sdCardCommentRepository.GetAll().ToArray();


            // Create new temp list of records
            List<ShellTemperatureRecord> records = new List<ShellTemperatureRecord>();
            // For ever item in ShellTemps, find and match the comment that may have been made
            foreach (ShellTemp shellTemp in tempData)
            {
                ShellTemperatureRecord shellTemperatureRecord =
                    new ShellTemperatureRecord(shellTemp.Id, shellTemp.Temperature, shellTemp.RecordedDateTime,
                        shellTemp.Latitude, shellTemp.Longitude, shellTemp.Device, false); // Not from SD

                // Find the comment for the shell temperature
                ShellTemperatureComment comment =
                    liveDataComments.FirstOrDefault(x => x.ShellTemp.Id == shellTemperatureRecord.Id);

                ShellTemperaturePosition position =
                    positions.FirstOrDefault(x => x.ShellTemp.Id == shellTemperatureRecord.Id);

                if (comment?.Comment != null)
                    shellTemperatureRecord.Comment = comment.Comment.Comment;
                if (position?.Position != null)
                    shellTemperatureRecord.Position = position.Position.Position;

                records.Add(shellTemperatureRecord);
            }

            // Find the sd card data shell temps
            foreach (SdCardShellTemp shellTemp in sdCardShellTemps)
            {
                if (!shellTemp.RecordedDateTime.HasValue) // Doesn't have DateTime, skip
                    continue;

                ShellTemperatureRecord shellTemperatureRecord =
                    new ShellTemperatureRecord(shellTemp.Id, shellTemp.Temperature, (DateTime)shellTemp.RecordedDateTime,
                        shellTemp.Latitude, shellTemp.Longitude, shellTemp.Device, true); // This is from SD

                // Find the comment for the sd card shell temperature
                SdCardShellTemperatureComment temp =
                    sdCardComments.FirstOrDefault(x => x.SdCardShellTemp.Id == shellTemperatureRecord.Id);

                if (temp?.Comment != null)
                    shellTemperatureRecord.Comment = temp.Comment.Comment;

                records.Add(shellTemperatureRecord);
            }

            return records.ToArray();
        }
        #endregion
    }
}