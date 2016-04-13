using Microsoft.Tri.Common.Data.Configurations;
using Microsoft.Tri.Common.Data.Serialization;
using Microsoft.Tri.Infrastructure;
using Microsoft.Tri.Infrastructure.Utils;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tri.Common.Data.Common
{
    public static class ManagementClient
    {
        #region Methods

        public static RegisterGatewayResult RegisterGateway(
            string managementServerUrl,
            string managementCertificateThumbprint,
            string managementAccountName,
            string managementAccountPassword,
            NetbiosName gatewayNetbiosName,
            X509Certificate2 gatewayCertificate)
        {
            Contract.Requires(!managementServerUrl.IsNullOrWhiteSpace());
            Contract.Requires(!managementCertificateThumbprint.IsNullOrWhiteSpace());
            Contract.Requires(!managementAccountName.IsNullOrWhiteSpace());
            Contract.Requires(!managementAccountPassword.IsNullOrWhiteSpace());
            Contract.Requires(gatewayNetbiosName != null);
            Contract.Requires(gatewayCertificate != null);

            using (var webRequestHandler = new WebRequestHandler())
            {
                webRequestHandler.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                {
                    var customCertificateValidator = new CustomCertificateValidator(managementCertificateThumbprint);
                    customCertificateValidator.Validate(new X509Certificate2(certificate));
                    return true;
                };
                using (var httpClient = new HttpClient(webRequestHandler))
                {
                    var managementUrl = $"{managementServerUrl}/Api";
                    try
                    {
                        var loginResponse = PostAsync<ManagementLoginResponse>(
                            httpClient,
                            $"{managementUrl}/Authentication/Login",
                            new ManagementLoginRequest
                            {
                                AccountName = managementAccountName,
                                Password = managementAccountPassword,
                            }).Result;
                        if (loginResponse.Status != ManagementLoginResponseStatus.Success)
                        {
                            return new RegisterGatewayResult(RegisterGatewayResultStatus.FailedAuthentication);
                        }

                        var gatewayConfiguration = PostAsync<GatewayConfiguration>(
                        httpClient,
                        $"{managementUrl}/SystemProfiles/Gateways/{gatewayNetbiosName}",
                        new RegisterGatewayRequest
                        {
                            Certificate = gatewayCertificate,
                            Version = GeneralInformation.FileVersion
                        }).Result;
                        return new RegisterGatewayResult(gatewayConfiguration);
                    }
                    catch (Exception exception)
                    {
                        if (exception.InnerException is HttpRequestException)
                        {
                            if (exception.InnerException.InnerException is WebException)
                            {
                                return new RegisterGatewayResult(
                                    RegisterGatewayResultStatus.FailedConnectivity,
                                    exception.ToString());
                            }
                            return new RegisterGatewayResult(
                                RegisterGatewayResultStatus.FailedInternal,
                                exception.ToString());
                        }
                        return new RegisterGatewayResult(
                            RegisterGatewayResultStatus.Failed,
                            exception.ToString());
                    }
                }
            }
        }

        private static async Task<TResponse> PostAsync<TResponse>(
            HttpClient httpClient,
            string url,
            object content)
        {
            var httpResponseMessage = await httpClient.PostAsync(
                url,
                new StringContent(
                    JsonConvert.SerializeObject(content),
                    Encoding.UTF8,
                    "application/json")).
                ConfigureAwait(false);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.Content.ReadAsAsync<TResponse>(
                new[]
                {
                    new JsonMediaTypeFormatter
                    {
                        SerializerSettings =
                        {
                            Converters = JsonConverters.DefaultConverters.ToArray(),
                        }
                    }
                });
        }

        #endregion
    }

    public sealed class ManagementLoginRequest
    {
        #region Properties

        public string AccountName { get; set; }
        public bool IsPersistent { get; set; }
        public string Password { get; set; }

        #endregion
    }

    public sealed class ManagementLoginResponse
    {
        #region Properties

        public string AccountName { get; private set; }
        public ManagementLoginResponseStatus Status { get; private set; }

        #endregion

        #region Constructors

        public ManagementLoginResponse(string accountName, ManagementLoginResponseStatus status)
        {
            if (accountName.IsNullOrWhiteSpace())
            {
                AccountName = accountName;
            }
            else
            {
                var domainAccountName = DomainSecurityPrincipalName.Parse(accountName);
                AccountName = domainAccountName.Name is UpnName
                    ? ((UpnName)domainAccountName.Name).UserName
                    : domainAccountName.Name.ToString();
            }
            Status = status;
        }

        #endregion
    }

    public enum ManagementLoginResponseStatus
    {
        Success,
        FailedAuthentication,
        FailedAuthorization,
    }

    public sealed class RegisterGatewayRequest
    {
        #region Properties

        public X509Certificate2 Certificate { get; set; }
        public string Version { get; set; }

        #endregion
    }

    public sealed class RegisterGatewayResult
    {
        #region Properties

        public GatewayConfiguration Configuration { get; private set; }
        public RegisterGatewayResultStatus Status { get; private set; }
        public string ErrorMessage { get; private set; }

        #endregion

        #region Constructors

        public RegisterGatewayResult(GatewayConfiguration configuration)
        {
            Contract.Requires(configuration != null);

            Configuration = configuration;
            Status = RegisterGatewayResultStatus.Success;
        }

        public RegisterGatewayResult(
            RegisterGatewayResultStatus status,
            string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }

        #endregion
    }

    public enum RegisterGatewayResultStatus
    {
        Success,
        Failed,
        FailedAuthentication,
        FailedConnectivity,
        FailedInternal
    }
}