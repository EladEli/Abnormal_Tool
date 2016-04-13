using Microsoft.Tri.Common.Data.Entities;
using Microsoft.Tri.Common.Data.MonitoringAlerts;
using Microsoft.Tri.Infrastructure;
using Microsoft.Tri.Infrastructure.Framework;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Microsoft.Tri.Common.Data.Common
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [ProtoInclude(101, typeof(MonitoringAlert))]
    public abstract class Alert : Entity
    {
        #region Properties

        [BsonId]
        public ObjectId Id { get; protected set; }
        public DateTime StartTime { get; protected set; }
        public DateTime EndTime { get; protected set; }
        [JsonIgnore]
        public bool IsVisible { get; protected set; }
        public AlertSeverity Severity { get; protected set; }
        public AlertStatus Status { get; set; }
        public string TitleKey { get; protected set; }
        [JsonIgnore]
        public IEnumerable<string> TitleDetailKeys { get; protected set; }
        public string DescriptionFormatKey { get; protected set; }
        public IEnumerable<string> DescriptionDetailFormatKeys { get; protected set; }

        #endregion

        #region Constructors

        protected Alert()
        {
            TitleDetailKeys = new List<string>();
            DescriptionDetailFormatKeys = new List<string>();
        }

        protected Alert(
            DateTime time,
            AlertSeverity severity,
            AlertStatus status)
            : this(
                time,
                time,
                severity,
                status)
        {
        }

        protected Alert(
            DateTime startTime,
            DateTime endTime,
            AlertSeverity severity,
            AlertStatus status)
        {
            Id = ObjectId.GenerateNewId();
            StartTime = startTime;
            EndTime = endTime;
            IsVisible = true;
            Severity = severity;
            Status = status;
        }

        #endregion

        #region Methods

        public virtual void UpdateProperties(IModuleLocator locator)
        {
            Contract.Requires(locator != null);

            TitleKey = $"{Type}Title";
            TitleDetailKeys = Array.Empty<string>();
            DescriptionFormatKey = $"{Type}Description";
            DescriptionDetailFormatKeys = Array.Empty<string>();
        }

        #endregion
    }

    [ProtoContract(EnumPassthru = true)]
    public enum AlertSeverity
    {
        Low = 3,
        Medium = 5,
        High = 10,
    }

    [ProtoContract(EnumPassthru = true)]
    public enum AlertStatus
    {
        Open,
        Resolved,
        Dismissed,
    }
}