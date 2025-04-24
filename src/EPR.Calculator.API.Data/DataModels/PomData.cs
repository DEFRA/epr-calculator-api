﻿namespace EPR.Calculator.API.Data.DataModels
{
    public class PomData
    {
        public int? OrganisationId { get; set; }

        public string? SubsidaryId { get; set; }

        public required string? SubmissionPeriod { get; set; }

        public string? PackagingActivity { get; set; }

        public string? PackagingType { get; set; }

        public string? PackagingClass { get; set; }

        public string? PackagingMaterial { get; set; }

        public double? PackagingMaterialWeight { get; set; }

        public required string? SubmissionPeriodDesc { get; set; }

        public required DateTime LoadTimeStamp { get; set; }
    }
}
