using System;
using MongoDB.Bson;

namespace Abnormal_UI.Imported
{
    class Kerberos
    {
        #region Properties
        public DateTime? ProcessingTime { get; set; }
        public string SourceComputerId { get; set; }
        public string SourceAccountId { get; set; }
        public ObjectId RequestTicketKerberosId { get; set; }
        public byte[] ResponseTicketHash { get; set; }
        public KerberosError Error { get; set; }
        public KerberosEncryptionType? RequestTicketEncryptionType { get; set; }
        public KerberosActionType KerberosActionType { get; set; }


        #endregion

    }

    public enum KerberosEncryptionType
    {
        Null = 0,
        DesCbcCrc = 1,
        DesCbcMd4 = 2,
        DesCbcMd5 = 3,
        DesCbcRaw = 4,
        Des3CbcSha = 5,
        Des3CbcRaw = 6,
        DesHmacSsh1 = 8,
        DsaSha1Cms = 9,
        RsaMd5Cms = 10,
        RsaSha1Cms = 11,
        Rc2CbcEnv = 12,
        RsaEnv = 13,
        RsaEsOeapEnv = 14,
        DesEde3CbcEnv = 15,
        Des3CbcSha1 = 16,
        Aes128CtsHmacSha196 = 17,
        Aes256CtsHmacSha196 = 18,
        DesCbcMd5Nt = 20,
        Rc4Hmac = 23,
        Rc4HmacExp = 24,
        Camellia128CtsCmac = 25,
        Camellia256CtsCmac = 26,
        Unknown = 511,
        LocalDes3HmacSha1 = 28679,
        Rc4PlainExp = -141,
        Rc4Plain = -140,
        Rc4PlainOldExp = -136,
        Rc4HmacOldExp = -135,
        Rc4PlainOld = -134,
        Rc4HmacOld = -133,
        DesPlain = -132,
        Rc4Sha = -131,
        Rc4Lm = -130,
        Rc4Plain2 = -129,
        Rc4Md4 = -128
    }
    public enum KerberosActionType
    {
        None,
        As,
        Tgs,
        Ap,
    }
    public enum KerberosError
    {
        Success = 0,
        NameExpired = 1,
        ServiceExpired = 2,
        BadProtocolVersionNumber = 3,
        ClientOldMasterKey = 4,
        ServerOldMasterKey = 5,
        ClientPrincipalUnknown = 6,
        ServerPrincipalUnknown = 7,
        PrincipalNotUnique = 8,
        NullKey = 9,
        CannotPostdate = 10,
        NeverValid = 11,
        Policy = 12,
        BadOption = 13,
        EncryptionTypeNotSupported = 14,
        ChecksumTypeNotSupported = 15,
        PadataTypeNotSupported = 16,
        TransitedTypeNotSupported = 17,
        ClientRevoked = 18,
        ServiceRevoked = 19,
        TgtRevoked = 20,
        ClientNotYetValid = 21,
        ServiceNotYetValid = 22,
        KeyExpired = 23,
        PreauthenticationFailed = 24,
        PreauthenticationRequired = 25,
        ServerNoMatch = 26,
        MustUseUser2User = 27,
        PathNotAccepted = 28,
        ServiceUnavailable = 29,
        BadIntegrity = 31,
        TicketExpired = 32,
        TicketNotYetValid = 33,
        Repeat = 34,
        NotUs = 35,
        BadMatch = 36,
        Skew = 37,
        BadAddress = 38,
        BadVersion = 39,
        MessageType = 40,
        Modified = 41,
        BadOrder = 42,
        BadKeyVersion = 44,
        NoKey = 45,
        MutualFailed = 46,
        BadDirection = 47,
        Method = 48,
        BadSequence = 49,
        InappropriateChecksum = 50,
        TransitedPathNotAccepted = 51,
        ResponseTooBig = 52,
        Generic = 60,
        FieldTooLong = 61,
        ClientNotTrusted = 62,
        KdcNotTrusted = 63,
        InvalidSig = 64,
        KeyTooWeak = 65,
        CertificateMismatch = 66,
        NoTgt = 67,
        WrongRealm = 68,
        UserToUserRequired = 69,
        CannotVerifyCertificate = 70,
        InvalidCertificate = 71,
        RevokedCertificate = 72,
        RevocationStatusUnknown = 73,
        RevocationStatusUnavailable = 74,
        ClientNameMismatch = 75,
        KdcNameMismatch = 76,
    }
    
}
