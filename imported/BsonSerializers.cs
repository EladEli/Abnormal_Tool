using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;

namespace Abnormal_UI.Imported
{

    public class ObjectBsonSerializer<TSource, TDestination> : ClassSerializerBase<TSource>
        where TSource : class
    {
        #region Data Members

        private readonly Func<TSource, TDestination> _serializer;
        private readonly Func<TDestination, TSource> _deserializer;

        #endregion

        #region Constructors

        public ObjectBsonSerializer(
            Func<TSource, TDestination> serializer,
            Func<TDestination, TSource> deserializer)
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }

        #endregion

        #region ClassSerializerBase

        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs arguments, TSource value)
        {

            BsonSerializer.Serialize(context.Writer, _serializer(value));
        }

        protected override TSource DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs arguments)
        {

            return _deserializer(BsonSerializer.Deserialize<TDestination>(context.Reader));
        }

        #endregion
    }

    public sealed class ObjectStringBsonSerializer<TSource> : ObjectBsonSerializer<TSource, string>
        where TSource : class
    {
        #region Constructors

        public ObjectStringBsonSerializer(Func<string, TSource> deserializer)
            : base(_ => _.ToString(), deserializer)
        {
        }

        #endregion
    }

    public sealed class DomainObjectNameBsonSerializer<TType> : SerializerBase<TType>
        where TType : DomainObjectName
    {
        #region Data Members

        private readonly Func<string, string, TType> _parser;

        #endregion

        #region Constructors

        public DomainObjectNameBsonSerializer(Func<string, string, TType> parser)
        {

            _parser = parser;
        }

        #endregion

        #region ClassSerializerBase

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs arguments, TType value)
        {

            if (value == null)
            {
                context.Writer.WriteNull();
            }
            else
            {
                context.Writer.WriteStartDocument();
                if (value.DomainName == null)
                {
                    context.Writer.WriteNull(nameof(DomainObjectName.DomainName));
                }
                else
                {
                    context.Writer.WriteString(nameof(DomainObjectName.DomainName), value.DomainName.ToString());
                }
                context.Writer.WriteString(nameof(DomainObjectName.Name), value.Name.ToString());
                context.Writer.WriteEndDocument();
            }
        }

        public override TType Deserialize(BsonDeserializationContext context, BsonDeserializationArgs arguments)
        {

            if (context.Reader.GetCurrentBsonType() == BsonType.Null)
            {
                context.Reader.ReadNull();
                return null;
            }
            else
            {
                context.Reader.ReadStartDocument();
                string domainName = null;
                context.Reader.ReadName(nameof(DomainObjectName.DomainName));
                if (context.Reader.GetCurrentBsonType() == BsonType.Null)
                {
                    context.Reader.ReadNull();
                }
                else
                {
                    domainName = context.Reader.ReadString();
                }

                var domainObjectName =
                    _parser(
                        domainName,
                        context.Reader.ReadString(nameof(DomainObjectName.Name)));
                context.Reader.ReadEndDocument();
                return domainObjectName;
            }
        }

        #endregion
    }
}