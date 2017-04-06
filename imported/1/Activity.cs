using Microsoft.Tri.Common.Data.Entities;
using Microsoft.Tri.Common.Data.EventActivities;
using Microsoft.Tri.Common.Data.NetworkActivities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;
using System;

namespace Microsoft.Tri.Common.Data.Common
{
    [BsonKnownTypes(typeof(EventActivity))]
    [BsonKnownTypes(typeof(NetworkActivity))]
    [ProtoInclude(101, typeof(EventActivity))]
    [ProtoInclude(102, typeof(NetworkActivity))]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class Activity : Entity
    {
        #region Properties

        [BsonId]
        public ObjectId Id { get; private set; }
        [BsonIgnore]
        [ProtoIgnore]
        public Guid FlowId { get; private set; }

        #endregion

        #region Constructors

        protected Activity()
        {
            Id = ObjectId.GenerateNewId();
            FlowId = Guid.NewGuid();
        }

        #endregion
    }
}