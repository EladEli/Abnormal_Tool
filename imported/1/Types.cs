using JetBrains.Annotations;
using Microsoft.Tri.Infrastructure;
using MoreLinq;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace Microsoft.Tri.Common.Data.Common
{
    public interface IName
    {
    }

    [PublicAPI]
    [ProtoInclude(101, typeof(DistinguishedName))]
    [ProtoInclude(102, typeof(NetworkName))]
    [ProtoInclude(103, typeof(SamName))]
    [ProtoInclude(104, typeof(SpnName))]
    [ProtoInclude(105, typeof(UpnName))]
    [ProtoContract]
    public class Name
    {
        #region Properties

        [ProtoMember(1)]
        protected string Value { get; private set; }

        #endregion

        #region Constructors

        protected Name(string name)
        {
            Contract.Requires(!name.IsNullOrWhiteSpace());

            Value = name.Trim();
        }

        protected Name()
        {
        }

        #endregion

        #region Methods

        public override bool Equals(object other)
        {
            return
                other != null &&
                GetType() == other.GetType() &&
                Value.EqualsOrdinalIgnoreCase(((Name)other).Value);
        }

        public override int GetHashCode()
        {
            return Value.ToLower().GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }

        #endregion
    }

    [ProtoInclude(101, typeof(DnsName))]
    [ProtoInclude(102, typeof(NetbiosName))]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class NetworkName : Name
    {
        #region Consturctors

        protected NetworkName(string name)
            : base(name)
        {
        }

        protected NetworkName()
        {
        }

        #endregion

        #region Methods

        public static NetworkName TryParse(string name)
        {
            return name.IsNullOrWhiteSpace()
                ? null
                : (NetworkName)DnsName.TryParse(name) ?? NetbiosName.TryParse(name);
        }

        public bool IsSimilar(NetworkName other)
        {
            if (other == null)
            {
                return false;
            }

            return GetType() == other.GetType()
                ? Equals(other)
                : (other is NetbiosName
                    ? ToString().StartsWithOrdinalIgnoreCase(other.ToString())
                    : other.ToString().StartsWithOrdinalIgnoreCase(ToString()));
        }

        #endregion
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class NetbiosName : NetworkName
    {
        #region Constructors

        private NetbiosName(string name)
            : base(name)
        {
        }

        [JsonConstructor]
        private NetbiosName()
        {
        }

        #endregion

        #region Methods

        public static NetbiosName Parse(string name)
        {
            var netbiosName = TryParse(name);
            if (netbiosName == null)
            {
                throw new ExtendedException($"Failed to parse NetBIOS name [{nameof(name)}={name}]");
            }
            return netbiosName;
        }

        public new static NetbiosName TryParse(string name)
        {
            return name.IsNullOrWhiteSpace() ? null : new NetbiosName(name);
        }

        #endregion
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class DnsName : NetworkName
    {
        #region Properties

        [JsonIgnore]
        public string ComputerName { get; private set; }

        [JsonIgnore]
        public string DomainName { get; private set; }

        #endregion

        #region Constructors

        private DnsName(string name)
            : base(name)
        {
            var nameParts = Value.Split(".", 2);
            ComputerName = nameParts[0];
            DomainName = nameParts.Length == 1 ? null : nameParts[1];
        }

        [JsonConstructor]
        private DnsName()
        {
        }

        #endregion

        #region Methods

        public static DnsName Parse(string name)
        {
            var dnsName = TryParse(name);
            if (dnsName == null)
            {
                throw new ExtendedException($"Failed to parse DNS name [{nameof(name)}={name}]");
            }
            return dnsName;
        }

        public static DnsName TryParse(string name, bool isForceValidation = true)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return null;
            }

            var nameParts = name.Split(".", StringSplitOptions.None);
            return isForceValidation && (nameParts.Length == 1 || nameParts.Any(_ => _.IsNullOrWhiteSpace()))
                ? null
                : new DnsName(name);
        }

        #endregion
    }

    public interface ISecurityPrincipalName : IName
    {
        bool IsComputer { get; }
    }

    public static class SecurityPrincipalName
    {
        #region Methods

        public static ISecurityPrincipalName TryParse(string name)
        {
            return name.IsNullOrWhiteSpace()
                ? null
                : ((ISecurityPrincipalName)UpnName.TryParse(name) ?? SamName.TryParse(name));
        }

        #endregion
    }

    public interface IResourceName : IName
    {
    }

    public static class ResourceName
    {
        #region Methods

        public static IResourceName TryParse(string name)
        {
            return name.IsNullOrWhiteSpace()
                ? null
                : SpnName.TryParse(name) ?? ((IResourceName)UpnName.TryParse(name) ?? SamName.TryParse(name));
        }

        #endregion
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class SamName : Name, ISecurityPrincipalName, IResourceName
    {
        #region Properties

        [ProtoIgnore]
        public bool IsComputer => ToString().EndsWith("$");

        #endregion

        #region Constructors

        private SamName(string name)
            : base(name)
        {
        }

        [JsonConstructor]
        private SamName()
        {
        }

        #endregion

        #region Methods

        public static SamName Parse(string name)
        {
            var samName = TryParse(name);
            if (samName == null)
            {
                throw new ExtendedException($"Failed to parse SAM name [{nameof(name)}={name}]");
            }
            return samName;
        }

        public static SamName TryParse(string name)
        {
            return name.IsNullOrWhiteSpace() ? null : new SamName(name);
        }

        #endregion
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class UpnName : Name, ISecurityPrincipalName, IResourceName
    {
        #region Data Members

        public const string Separator = "@";

        #endregion

        #region Properties

        public NetworkName DomainName { get; private set; }
        public string UserName { get; private set; }
        [ProtoIgnore]
        public bool IsComputer => UserName.EndsWith("$");

        #endregion

        #region Constructors

        private UpnName(string name)
            : base(name)
        {
            var nameParts = Value.Split(Separator, 2);
            DomainName = NetworkName.TryParse(nameParts.Length == 1 ? null : nameParts[1]);
            UserName = nameParts[0];
        }

        [JsonConstructor]
        private UpnName()
        {
        }

        #endregion

        #region Methods
        
        public static UpnName TryParse(string name, bool isForceValidation = true)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return null;
            }

            var nameParts = name.Split(Separator, StringSplitOptions.None);
            return isForceValidation && (nameParts.Length != 2 || nameParts.Any(_ => _.IsNullOrWhiteSpace()))
                ? null
                : new UpnName(name);
        }

        #endregion
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class DistinguishedName : Name, ISecurityPrincipalName
    {
        #region Data Members

        [ProtoMember(101)]
        private readonly IEnumerable<DistinguishedNamePart> _nameParts;

        #endregion

        #region Properties

        [ProtoIgnore]
        public bool IsComputer => false;
        [JsonIgnore]
        [ProtoIgnore]
        public DistinguishedName Parent => new DistinguishedName(_nameParts.Skip(1).ToList());
        [JsonIgnore]
        [ProtoIgnore]
        public DistinguishedName Domain { get { return new DistinguishedName(_nameParts.Where(_ => _.Name.EqualsOrdinalIgnoreCase("DC")).ToList()); } }
        [JsonIgnore]
        [ProtoIgnore]
        public DnsName DomainDnsName { get { return DnsName.TryParse(Domain._nameParts.Select(_ => _.Value).ToDelimitedString("."), false); } }

        #endregion

        #region Constructors

        private DistinguishedName(string name)
            : base(name)
        {
            _nameParts = Enumerable.Empty<DistinguishedNamePart>();
        }

        private DistinguishedName(IReadOnlyCollection<DistinguishedNamePart> nameParts)
            : base(nameParts.Select(_ => $"{_.Name}={_.Value}").ToDelimitedString(","))
        {
            _nameParts = nameParts;
        }

        [JsonConstructor]
        private DistinguishedName()
        {
            _nameParts = new List<DistinguishedNamePart>();
        }

        #endregion

        #region Methods

        public static DistinguishedName Parse(string name)
        {
            var distinguishedName = TryParse(name);
            if (distinguishedName == null)
            {
                throw new ExtendedException($"Failed to parse distinguished name [{nameof(name)}={name}]");
            }
            return distinguishedName;
        }

        public static DistinguishedName TryParse(string name, bool isForceValidation = true)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return null;
            }

            var nameParts = DistinguishedNameParser.GetNameParts(name);
            return isForceValidation && nameParts.None()
                ? null
                : nameParts.None() ? new DistinguishedName(name) : new DistinguishedName(nameParts);
        }

        #endregion

        #region Types

        private static class DistinguishedNameParser
        {
            #region Data Members

            private const char _namePartDelimiter = ',';
            private const char _backslash = '\\';
            private const char _quotes = '"';
            private const char _valueDelimiter = '=';

            #endregion

            #region Methods

            public static IReadOnlyCollection<DistinguishedNamePart> GetNameParts(string distinguishedName)
            {
                Contract.Requires(!distinguishedName.IsNullOrWhiteSpace());

                var distinguishedNameParts = new List<DistinguishedNamePart>(4);
                var distinguishedNameLength = distinguishedName.Length;
                var distinguishedNameCurrentIndex = 0;
                var nameStartIndex = 0;
                var valueStartIndex = -1;
                var isInQuotes = false;

                while (distinguishedNameCurrentIndex < distinguishedNameLength)
                {
                    var distinguishedNameCurrentChar = distinguishedName[distinguishedNameCurrentIndex];
                    if (distinguishedNameCurrentChar == _backslash)
                    {
                        if (distinguishedNameCurrentIndex < distinguishedNameLength - 1)
                        {
                            distinguishedNameCurrentIndex++;
                        }
                    }
                    else if (distinguishedNameCurrentChar == _quotes)
                    {
                        isInQuotes = !isInQuotes;
                    }
                    else if (isInQuotes)
                    {
                        // No operation - need to wait for closing quotes
                    }
                    else if (distinguishedNameCurrentChar == _namePartDelimiter)
                    {
                        if (!AddNamePart(
                            distinguishedNameParts,
                            distinguishedName,
                            nameStartIndex,
                            valueStartIndex,
                            distinguishedNameCurrentIndex))
                        {
                            return Array.Empty<DistinguishedNamePart>();
                        }

                        nameStartIndex = distinguishedNameCurrentIndex + 1;
                        valueStartIndex = -1;
                    }
                    else if (distinguishedNameCurrentChar == _valueDelimiter)
                    {
                        if (valueStartIndex >= 0)
                        {
                            return Array.Empty<DistinguishedNamePart>();
                        }

                        valueStartIndex = distinguishedNameCurrentIndex + 1;
                    }

                    distinguishedNameCurrentIndex++;
                }

                if (isInQuotes ||
                    !AddNamePart(
                        distinguishedNameParts,
                        distinguishedName,
                        nameStartIndex,
                        valueStartIndex,
                        distinguishedNameCurrentIndex))
                {
                    return Array.Empty<DistinguishedNamePart>();
                }

                return distinguishedNameParts;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool AddNamePart(
                List<DistinguishedNamePart> distinguishedNameParts,
                string distinguishedName,
                int nameStartIndex,
                int valueStartIndex,
                int distinguishedNamePartEndIndex)
            {
                if (valueStartIndex < 0 || nameStartIndex >= valueStartIndex || valueStartIndex >= distinguishedNamePartEndIndex)
                {
                    return false;
                }

                var name = distinguishedName.Substring(nameStartIndex, valueStartIndex - nameStartIndex - 1);
                var value = distinguishedName.Substring(valueStartIndex, distinguishedNamePartEndIndex - valueStartIndex);
                if (name.Length == 0 || value.Length == 0)
                {
                    return false;
                }

                distinguishedNameParts.Add(new DistinguishedNamePart(name, value));
                return true;
            }

            #endregion
        }

        [PublicAPI]
        [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
        private sealed class DistinguishedNamePart
        {
            #region Properties

            public string Name { get; private set; }
            public string Value { get; private set; }

            #endregion

            #region Constructors

            public DistinguishedNamePart(string name, string value)
            {
                Contract.Requires(!name.IsNullOrWhiteSpace());
                Contract.Requires(!value.IsNullOrWhiteSpace());

                Name = name;
                Value = value;
            }

            public DistinguishedNamePart()
            {
            }

            #endregion
        }

        #endregion
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class SpnName : Name, IResourceName
    {
        #region Data Members

        public const string Separator = "/";

        #endregion

        #region Properties

        public string ClassName { get; private set; }
        public string SuffixName { get; private set; }

        #endregion

        #region Constructors

        private SpnName(IReadOnlyCollection<string> nameParts)
            : base(nameParts.ToDelimitedString(Separator))
        {
            Contract.Requires(nameParts != null && nameParts.Any());

            ClassName = nameParts.First();
            SuffixName = nameParts.Skip(1).ToDelimitedString(Separator);
        }

        [JsonConstructor]
        private SpnName()
        {
        }

        #endregion

        #region Methods

        public static SpnName TryParse(string name, bool isForceValidation = true)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return null;
            }

            return TryParse(name.Split(Separator, 2, StringSplitOptions.None), isForceValidation);
        }

        public static SpnName TryParse(IReadOnlyCollection<string> nameParts, bool isForceValidation = true)
        {
            return nameParts == null ||
                   nameParts.None() ||
                   isForceValidation && (nameParts.Count < 2 || nameParts.Any(_ => _.IsNullOrWhiteSpace()))
                ? null
                : new SpnName(nameParts);
        }

        #endregion
    }

    [PublicAPI]
    [ProtoInclude(101, typeof(DomainNetworkName))]
    [ProtoInclude(102, typeof(DomainResourceName))]
    [ProtoInclude(103, typeof(DomainSecurityPrincipalName))]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DomainObjectName
    {
        #region Properties

        public NetworkName DomainName { get; private set; }
        public Name Name { get; private set; }

        #endregion

        #region Constructors

        protected DomainObjectName()
        {
        }

        protected DomainObjectName(NetworkName domainName, Name name)
        {
            Contract.Requires(name != null);

            DomainName = domainName;
            Name = name;
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            var other = obj as DomainObjectName;
            return
               other != null &&
               GetType() == other.GetType() &&
               Equals(DomainName, other.DomainName) &&
               Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return new
            {
                DomainName,
                Name
            }.GetHashCode();
        }

        public override string ToString()
        {
            return new[]
            {
                $"{nameof(DomainName)}={DomainName}",
                $"{nameof(Name)}={Name}"
            }.ToDelimitedString(" ");
        }

        #endregion
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class DomainNetworkName : DomainObjectName
    {
        #region Constructors

        public DomainNetworkName(NetworkName domainName, NetworkName name)
            : base(domainName, name)
        {
        }

        [JsonConstructor]
        private DomainNetworkName()
        {
        }

        #endregion

        #region Methods

        public static DomainNetworkName TryParse(string name)
        {
            return TryParse(null, name);
        }

        public static DomainNetworkName TryParse(string domainName, string name)
        {
            return name.IsNullOrWhiteSpace() ? null : new DomainNetworkName(NetworkName.TryParse(domainName), NetworkName.TryParse(name));
        }

        public bool IsSimilar(DomainNetworkName other)
        {
            return
                DomainName == null && other.DomainName == null && Name.Equals(other.Name) ||
                ((NetworkName)Name).IsSimilar((NetworkName)other.Name) &&
                (DomainName != null && other.DomainName != null && DomainName.IsSimilar(other.DomainName) ||
                 DomainName != null && other.DomainName == null && Name is NetbiosName && other.Name is DnsName && DomainName.IsSimilar(NetworkName.TryParse(((DnsName)other.Name).DomainName)) ||
                 DomainName == null && other.DomainName != null && Name is DnsName && other.Name is NetbiosName && other.DomainName.IsSimilar(NetworkName.TryParse(((DnsName)Name).DomainName)));
        }

        #endregion
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class DomainSecurityPrincipalName : DomainObjectName
    {
        #region Properties

        [ProtoIgnore]
        public bool IsComputer => ((ISecurityPrincipalName)Name).IsComputer;

        #endregion

        #region Constructors

        public DomainSecurityPrincipalName(NetworkName domainName, ISecurityPrincipalName name)
            : base(domainName, (Name)name)
        {
        }

        [JsonConstructor]
        private DomainSecurityPrincipalName()
        {
        }

        #endregion

        #region Methods

        public static DomainSecurityPrincipalName Parse(string name)
        {
            var domainSecurityPrincipalName = TryParse(name);
            if (domainSecurityPrincipalName == null)
            {
                throw new ExtendedException($"Failed to parse domain security principal name [{nameof(name)}={name}]");
            }
            return domainSecurityPrincipalName;
        }

        public static DomainSecurityPrincipalName TryParse(string name)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return null;
            }

            var samNameParts = name.Split("\\");
            if (samNameParts.Length == 2)
            {
                var domainSecurityPrincipalName = TryParse(samNameParts[0], samNameParts[1]);
                if (domainSecurityPrincipalName != null)
                {
                    return domainSecurityPrincipalName;
                }
            }

            var distinguishedName = DistinguishedName.TryParse(name);
            if (distinguishedName != null)
            {
                return new DomainSecurityPrincipalName(distinguishedName.DomainDnsName, distinguishedName);
            }

            var securityPrincipalName = SecurityPrincipalName.TryParse(name);
            return securityPrincipalName == null
                ? null
                : new DomainSecurityPrincipalName(null, securityPrincipalName);
        }

        public static DomainSecurityPrincipalName Parse(string domainName, string name)
        {
            var domainSecurityPrincipalName = TryParse(domainName, name);
            if (domainSecurityPrincipalName == null)
            {
                throw new ExtendedException($"Failed to parse domain security principal name [{nameof(domainName)}={domainName} {nameof(name)}={name}]");
            }
            return domainSecurityPrincipalName;
        }

        public static DomainSecurityPrincipalName TryParse(string domainName, string name)
        {
            return name.IsNullOrWhiteSpace()
                ? null
                : domainName.IsNullOrWhiteSpace()
                    ? TryParse(name)
                    : new DomainSecurityPrincipalName(NetworkName.TryParse(domainName), SecurityPrincipalName.TryParse(name));
        }

        public static NetworkCredential CreateNetworkCredential(string accountName, string password, string accountDomainName = null)
        {
            Contract.Requires(!accountName.IsNullOrWhiteSpace());
            Contract.Requires(password != null);

            var domainSecurityPrincipalName = Parse(accountName);
            return new NetworkCredential(
                domainSecurityPrincipalName.Name.ToString(),
                password,
                accountDomainName ??
                domainSecurityPrincipalName.DomainName?.ToString());
        }

        public override string ToString()
        {
            return Name is SamName
                ? $"{(DomainName == null ? string.Empty : $@"{DomainName}\")}{Name}"
                : Name.ToString();
        }

        #endregion
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class DomainResourceName : DomainObjectName
    {
        #region Properties

        [ProtoIgnore]
        public string ClassName => (Name as SpnName)?.ClassName;
        [ProtoIgnore]
        public string TypeName => (ClassName ?? Name.ToString()).ToLower();

        #endregion

        #region Constructors

        public DomainResourceName(NetworkName domainName, IResourceName name)
            : base(domainName, (Name)name)
        {
        }

        [JsonConstructor]
        private DomainResourceName()
        {
        }

        #endregion

        #region Methods

        public static DomainResourceName Parse(string domainName, string name)
        {
            var domainResourceName = TryParse(domainName, name);
            if (domainResourceName == null)
            {
                throw new ExtendedException($"Failed to parse domain resource name [{nameof(domainName)}={domainName} {nameof(name)}={name}]");
            }
            return domainResourceName;
        }

        public static DomainResourceName TryParse(string domainName, string name)
        {
            return name.IsNullOrWhiteSpace()
                ? null
                : new DomainResourceName(NetworkName.TryParse(domainName), ResourceName.TryParse(name));
        }

        public static DomainResourceName Parse(string domainName, IReadOnlyCollection<string> nameParts)
        {
            var domainResourceName = TryParse(domainName, nameParts);
            if (domainResourceName == null)
            {
                throw new ExtendedException($"Failed to parse domain resource name [{nameof(domainName)}={domainName} {nameof(nameParts)}={nameParts.ToDelimitedString(SpnName.Separator)}]");
            }
            return domainResourceName;
        }

        private static DomainResourceName TryParse(string domainName, IReadOnlyCollection<string> nameParts)
        {
            return nameParts == null || nameParts.None()
                ? null
                : new DomainResourceName(NetworkName.TryParse(domainName), SpnName.TryParse(nameParts, false));
        }

        public override string ToString()
        {
            return Name is SamName
                ? $"{(DomainName == null ? string.Empty : $@"{DomainName}\")}{Name}"
                : Name.ToString();
        }

        #endregion
    }

    [PublicAPI]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class DomainResourceIdentifier
    {
        #region Properties

        public string AccountId { get; set; }
        public DomainResourceName ResourceName { get; private set; }

        #endregion

        #region Constructors

        public DomainResourceIdentifier(DomainResourceName resourceName)
        {
            Contract.Requires(resourceName != null);

            ResourceName = resourceName;
        }

        public DomainResourceIdentifier(DomainResourceName resourceName, string accountId)
        {
            Contract.Requires(resourceName != null);
            Contract.Requires(!accountId.IsNullOrWhiteSpace());

            ResourceName = resourceName;
            AccountId = accountId;
        }

        [JsonConstructor]
        private DomainResourceIdentifier()
        {
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            var other = obj as DomainResourceIdentifier;
            return
                other != null &&
                GetType() == other.GetType() &&
                AccountId == other.AccountId &&
                (AccountId == null
                    ? ResourceName.Equals(other.ResourceName)
                    : ResourceName.TypeName == other.ResourceName.TypeName);
        }

        public override int GetHashCode()
        {
            return
                AccountId == null
                    ? ResourceName.GetHashCode()
                    : AccountId.GetHashCode() ^
                      ResourceName.TypeName.GetHashCode();
        }

        public override string ToString()
        {
            return new[]
            {
                $"{nameof(AccountId)}={AccountId}",
                $"{nameof(ResourceName)}={ResourceName}"
            }.ToDelimitedString(" ");
        }

        #endregion
    }
}