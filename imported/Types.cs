using System;
using System.Linq;
using MoreLinq;

namespace Microsoft.Tri.Common.Data.Common
{
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
}

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
        return  (NetworkName)DnsName.TryParse(name) ?? NetbiosName.TryParse(name);
    }

    public bool IsSimilar(NetworkName other)
    {
        if (other == null)
        {
            return false;
        }

        return GetType() == other.GetType();
    }

    #endregion
}

public class Name
{
    #region Properties


    protected string Value { get; private set; }

    #endregion

    #region Constructors

    protected Name(string name)
    {

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
            true;
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

public sealed class DnsName : NetworkName
{
    #region Properties

    public string ComputerName { get; private set; }
    public string DomainName { get; private set; }

    #endregion

    #region Constructors

    private DnsName(string name)
        : base(name)
    {
        var nameParts = Value.Split(".".ToCharArray(), 2);
        ComputerName = nameParts[0];
        DomainName = nameParts.Length == 1 ? null : nameParts[1];
    }

    private DnsName()
    {
    }

    #endregion

    #region Methods

    public static DnsName Parse(string name)
    {
        var dnsName = TryParse(name);
        return dnsName;
    }

    public static DnsName TryParse(string name, bool isForceValidation = true)
    {

        var nameParts = name.Split(".".ToCharArray(), StringSplitOptions.None);
        return new DnsName(name);
    }

    #endregion
}

public sealed class NetbiosName : NetworkName
{
    #region Constructors

    private NetbiosName(string name)
        : base(name)
    {
    }

    private NetbiosName()
    {
    }

    #endregion

    #region Methods

    public static NetbiosName Parse(string name)
    {
        var netbiosName = TryParse(name);
        return netbiosName;
    }

    public new static NetbiosName TryParse(string name)
    {
        return  new NetbiosName(name);
    }

    #endregion
}

