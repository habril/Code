//----------------------------------------------------------------------------------------------
//    Copyright 2013 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software.Cryptography.X509Certificates
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Adal.TelemetryServiceWebApi
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Metadata;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using System.Xml;


    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configuration.MessageHandlers.Add(new TokenValidationHandler());
        }
    }

    internal class TokenValidationHandler : DelegatingHandler
    {
        const string Audience = "[Service App Uri, ex: http://localhost/service]";
        // Domain name or Tenant name
        const string DomainName = "[tenant or domain name, ex: aalsample.onmicrosoft.com]";

        static string _issuer = string.Empty;
        static List<X509SecurityToken> _signingTokens = null;
        static DateTime _stsMetadataRetrievalTime = DateTime.MinValue;

        // SendAsync is used to validate incoming requests contain a valid access token, and sets the current user identity 
        // SendAsync is used to validate incoming requests contain a valid access token, and sets the current user identity 
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string jwtToken;
            string issuer;
            string stsMetadataAddress = string.Format(CultureInfo.InvariantCulture, "https://login.windows.net/{0}/federationmetadata/2007-06/federationmetadata.xml", DomainName);

            List<X509SecurityToken> signingTokens;
            using (HttpResponseMessage responseMessage = new HttpResponseMessage())
            {

                if (!TryRetrieveToken(request, out jwtToken))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
                }

                try
                {
                    // Get tenant information that's used to validate incoming jwt tokens
                    GetTenantInformation(stsMetadataAddress, out issuer, out signingTokens);
                }
                catch (WebException)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                }
                catch (InvalidOperationException)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                }

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler()
                {
                    // for demo purposes certificate validation is turned off. Please note that this shouldn't be done in production code.
                    CertificateValidator = X509CertificateValidator.None
                };

                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    AllowedAudience = Audience,
                    ValidIssuer = issuer,
                    SigningTokens = signingTokens
                };


                try
                {
                    // Validate token
                    ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(jwtToken,
                                                                                 validationParameters);

                    //set the ClaimsPrincipal on the current thread.
                    Thread.CurrentPrincipal = claimsPrincipal;

                    // set the ClaimsPrincipal on HttpContext.Current if the app is running in web hosted environment.
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.User = claimsPrincipal;
                    }

                    return base.SendAsync(request, cancellationToken);
                }
                catch (SecurityTokenValidationException)
                {
                    responseMessage.StatusCode = HttpStatusCode.Unauthorized;
                    return Task.FromResult(responseMessage);
                }
                catch (SecurityTokenException)
                {
                    responseMessage.StatusCode = HttpStatusCode.Unauthorized;
                    return Task.FromResult(responseMessage);
                }
                catch (ArgumentException)
                {
                    responseMessage.StatusCode = HttpStatusCode.Unauthorized;
                    return Task.FromResult(responseMessage);
                }
                catch (FormatException)
                {
                    responseMessage.StatusCode = HttpStatusCode.Unauthorized;
                    return Task.FromResult(responseMessage);
                }
            }
        }

        // Reads the token from the authorization header on the incoming request
        static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        {
            token = null;

            if (!request.Headers.Contains("Authorization"))
            {
                return false;
            }

            string authzHeader = request.Headers.GetValues("Authorization").First<string>();

            // Verify Authorization header contains 'Bearer' scheme
            token = authzHeader.StartsWith("Bearer ", StringComparison.Ordinal) ? authzHeader.Split(' ')[1] : null;

            if (null == token)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses the federation metadata document and gets issuer Name and Signing Certificates
        /// </summary>
        /// <param name="metadataAddress">URL of the Federation Metadata document</param>
        /// <param name="issuer">Issuer Name</param>
        /// <param name="signingTokens">Signing Certificates in the form of X509SecurityToken</param>
        static void GetTenantInformation(string metadataAddress, out string issuer, out List<X509SecurityToken> signingTokens)
        {
            signingTokens = new List<X509SecurityToken>();

            // The issuer and signingTokens are cached for 24 hours. They are updated if any of the conditions in the if condition is true.            
            if (DateTime.UtcNow.Subtract(_stsMetadataRetrievalTime).TotalHours > 24
                || string.IsNullOrEmpty(_issuer)
                || _signingTokens == null)
            {
                MetadataSerializer serializer = new MetadataSerializer()
                {
                    // turning off certificate validation for demo. Don't use this in production code.
                    CertificateValidationMode = X509CertificateValidationMode.None
                };
                MetadataBase metadata = serializer.ReadMetadata(XmlReader.Create(metadataAddress));

                EntityDescriptor entityDescriptor = (EntityDescriptor)metadata;

                // get the issuer name
                if (!string.IsNullOrWhiteSpace(entityDescriptor.EntityId.Id))
                {
                    _issuer = entityDescriptor.EntityId.Id;
                }

                // get the signing certs
                _signingTokens = ReadSigningCertsFromMetadata(entityDescriptor);

                _stsMetadataRetrievalTime = DateTime.UtcNow;
            }

            issuer = _issuer;
            signingTokens = _signingTokens;
        }

        static List<X509SecurityToken> ReadSigningCertsFromMetadata(EntityDescriptor entityDescriptor)
        {
            List<X509SecurityToken> stsSigningTokens = new List<X509SecurityToken>();

            SecurityTokenServiceDescriptor stsd = entityDescriptor.RoleDescriptors.OfType<SecurityTokenServiceDescriptor>().First();

            if (stsd != null && stsd.Keys != null)
            {
                IEnumerable<X509RawDataKeyIdentifierClause> x509DataClauses = stsd.Keys.Where(key => key.KeyInfo != null && (key.Use == KeyType.Signing || key.Use == KeyType.Unspecified)).
                                                             Select(key => key.KeyInfo.OfType<X509RawDataKeyIdentifierClause>().First());

                stsSigningTokens.AddRange(x509DataClauses.Select(clause => new X509SecurityToken(new X509Certificate2(clause.GetX509RawData()))));
            }
            else
            {
                throw new InvalidOperationException("There is no RoleDescriptor of type SecurityTokenServiceType in the metadata");
            }

            return stsSigningTokens;
        }
    }
}