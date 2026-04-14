using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System.Reflection;
using System.Security.Claims;
using Siffrum.Ecom.BAL.Foundation.Odata;
using Siffrum.Ecom.BAL.Foundation.Config.Dependency;
using Siffrum.Ecom.BAL.Foundation.CommonUtils;


namespace Siffrum.Ecom.BAL.Foundation.Web
{
    public static class Extentions
    {
        public static long GetUserRecordIdFromCurrentUserClaims(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal != null && claimsPrincipal.Identity != null && claimsPrincipal.Identity.IsAuthenticated && claimsPrincipal.Identity is ClaimsIdentity)
            {
                Claim claim = (claimsPrincipal.Identity as ClaimsIdentity).Claims.FirstOrDefault((x) => x.Type == "dbRId");
                /*if (claim != null)
                {
                    return Convert.ToInt32(claim.Value);
                }*/
                if (claim != null && long.TryParse(claim.Value, out var id))
                {
                    return id;
                }
            }

            return 0L;
        }

        public static string GetUserRoleTypeFromCurrentUserClaims(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal != null && claimsPrincipal.Identity != null && claimsPrincipal.Identity.IsAuthenticated && claimsPrincipal.Identity is ClaimsIdentity)
            {
                Claim claim = (claimsPrincipal.Identity as ClaimsIdentity).Claims.FirstOrDefault((x) => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                if (claim != null)
                {
                    return claim.Value;
                }

            }

            return "";
        }

        public static long GetUserAdminIdFromCurrentUserClaims(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal?.Identity is ClaimsIdentity identity &&
                identity.IsAuthenticated)
            {
                var claim = identity.Claims.FirstOrDefault(x => x.Type == "adId");

                if (claim != null && long.TryParse(claim.Value, out var adminId))
                {
                    return adminId;
                }
            }

            return 0L;
        }

        public static string GetUserEmailFromUserClaims(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal != null && claimsPrincipal.Identity != null && claimsPrincipal.Identity.IsAuthenticated && claimsPrincipal.Identity is ClaimsIdentity)
            {
                Claim claim = (claimsPrincipal.Identity as ClaimsIdentity)?.Claims.FirstOrDefault((x) => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                if (claim != null)
                {
                    return claim.Value;
                }
            }

            return "";
        }

        public static string GetCompanyCodeFromCurrentUserClaims(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal != null && claimsPrincipal.Identity != null && claimsPrincipal.Identity.IsAuthenticated && claimsPrincipal.Identity is ClaimsIdentity)
            {
                Claim claim = (claimsPrincipal.Identity as ClaimsIdentity).Claims.FirstOrDefault((x) => x.Type == "clCd");
                if (claim != null)
                {
                    return claim.Value;
                }
            }

            return "";
        }

        public static int GetCompanyRecordIdFromCurrentUserClaims(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal != null && claimsPrincipal.Identity != null && claimsPrincipal.Identity.IsAuthenticated && claimsPrincipal.Identity is ClaimsIdentity)
            {
                Claim claim = (claimsPrincipal.Identity as ClaimsIdentity).Claims.FirstOrDefault((x) => x.Type == "clId");
                if (claim != null)
                {
                    return Convert.ToInt32(claim.Value);
                }
            }

            return 0;
        }

        public static void AutoRegisterAllBALAsSelfFromBaseTypes<TargetType>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            services.AutoRegisterAsSelfFromBaseTypes<BalRoot, TargetType>(serviceLifetime);
        }

        public static void AutoRegisterAsSelfFromBaseTypes<BaseT, TargetType>(this IServiceCollection services, ServiceLifetime serviceLifetime)
        {
            IServiceCollection services2 = services;
            Assembly assembly = typeof(TargetType).Assembly;
            IEnumerable<Type> source = from x in assembly.GetExportedTypes()
                                       where x.IsClass && !x.IsAbstract && typeof(BalRoot).IsAssignableFrom(x) && x.GetCustomAttributesData().FirstOrDefault((y) => y.AttributeType == typeof(IgnoreAutoInjectAttribute)) == null
                                       select x;
            source.ToList().ForEach(delegate (Type x)
            {
                services2.Add(new ServiceDescriptor(x, x, serviceLifetime));
            });
        }

        public static Guid GetTracingIdIfPresent(this HttpRequest request)
        {
            Guid result = Guid.Empty;
            if (request.Headers.ContainsKey("TracingID"))
            {
                Guid.TryParse(request.Headers["TracingID"].ToString(), out result);
            }

            return result;
        }

        public static Guid GetOrAddTracingId(this HttpRequest request)
        {
            Guid guid = request.GetTracingIdIfPresent();
            if (guid == Guid.Empty)
            {
                guid = Guid.NewGuid();
                if (request.Headers.ContainsKey("TracingID"))
                {
                    request.Headers["TracingID"] = guid.ToString();
                }
                else
                {
                    request.Headers.Add("TracingID", guid.ToString());
                }
            }

            return guid;
        }

        public static string GetValueFromHeaderOrQueryByKey(this HttpRequest request, string key)
        {
            string key2 = key;
            if (request != null)
            {
                if (request.Headers != null && request.Headers.ContainsKey(key2))
                {
                    return request.Headers.GetCommaSeparatedValues(key2).FirstOrDefault() ?? "";
                }

                if (request.Query != null)
                {
                    KeyValuePair<string, StringValues> keyValuePair = request.Query.FirstOrDefault((x) => x.Key == key2);
                    if (!keyValuePair.Equals(default(KeyValuePair<string, string>)))
                    {
                        return keyValuePair.Value.FirstOrDefault() ?? "";
                    }
                }
            }

            return "";
        }

        public static async Task<object> ToCustomizedObjectToLog(this HttpResponse response, bool addBody, bool addCookies, bool addSesitiveInfo = false, bool digestException = true, List<string> skipHeaderKeys = null)
        {
            List<string> skipHeaderKeys2 = skipHeaderKeys;
            Dictionary<string, string> headersToLog = null;
            string strReqBody = string.Empty;
            string serializationMessage = response == null ? "Failed ** HttpResponse is NULL ** " : null;
            skipHeaderKeys2 = skipHeaderKeys2 ?? new List<string>();
            try
            {
                if (response != null)
                {
                    List<KeyValuePair<string, StringValues>> headers = response.Headers?.ToList() ?? new List<KeyValuePair<string, StringValues>>();
                    if (!addSesitiveInfo)
                    {
                        skipHeaderKeys2.Add("authorization");
                        skipHeaderKeys2.Add("originauthorization");
                        skipHeaderKeys2.Add("usertoken");
                    }

                    if (!addCookies)
                    {
                        skipHeaderKeys2.Add("cookies");
                    }

                    if (skipHeaderKeys2.Count > 0)
                    {
                        headers.RemoveAll((x) => skipHeaderKeys2.Contains(x.Key.ToLower()));
                    }

                    headersToLog = headers?.ToDictionary((x) => x.Key, (x) => x.Value.ToString());
                }
            }
            catch (Exception e)
            {
                serializationMessage = serializationMessage + "  Failed ** HttpRequest.HEADERS ** Msg - '" + e.Message + "'. ||| ";
                if (!digestException)
                {
                    throw e;
                }
            }

            try
            {
                if (addBody && response != null)
                {
                    if (response.Body != null && response.Body.CanSeek)
                    {
                        strReqBody = await response.Body.ReadStreamAsStringIfPossible();
                    }
                    else
                    {
                        serializationMessage = serializationMessage + "  Failed ** HttpResponseMessage.BODY ** Cannot Seek Stream of type '" + response?.Body?.GetType().ToString() + "'  |||  ";
                    }
                }
            }
            catch (Exception e2)
            {
                serializationMessage = serializationMessage + "  Failed ** HttpResponseMessage.BODY ** Msg - '" + e2.Message + "'. ||| ";
                if (!digestException)
                {
                    throw e2;
                }
            }

            return new
            {
                response?.StatusCode,
                response?.ContentLength,
                response?.ContentType,
                Headers = headersToLog,
                Body = strReqBody,
                NetFramework = "netstandard2.0",
                SerializationMessage = serializationMessage ?? "  Success ** (body " + (addBody ? "NOT-" : "") + "SKIPPED)."
            };
        }

        public static async Task<object> ToCustomizedObjectToLog(this HttpRequest request, bool addBody, bool addCookies, bool addSesitiveInfo = false, bool digestException = true, List<string> skipHeaderKeys = null)
        {
            List<string> skipHeaderKeys2 = skipHeaderKeys;
            HttpRequest request2 = request;
            Dictionary<string, string> headersToLog = null;
            IEnumerable<KeyValuePair<string, StringValues>> formItems = null;
            string strReqBody = string.Empty;
            string serializationMessage = request2 == null ? "Failed ** HttpRequest is NULL ** " : null;
            skipHeaderKeys2 = skipHeaderKeys2 ?? new List<string>();
            try
            {
                if (request2 != null)
                {
                    List<KeyValuePair<string, StringValues>> headers = request2.Headers?.ToList() ?? new List<KeyValuePair<string, StringValues>>();
                    if (!addSesitiveInfo)
                    {
                        skipHeaderKeys2.Add("authorization");
                        skipHeaderKeys2.Add("originauthorization");
                        skipHeaderKeys2.Add("usertoken");
                    }

                    if (!addCookies)
                    {
                        skipHeaderKeys2.Add("cookies");
                    }

                    if (skipHeaderKeys2.Count > 0)
                    {
                        headers.RemoveAll((x) => skipHeaderKeys2.Contains(x.Key.ToLower()));
                    }

                    headersToLog = headers?.ToDictionary((x) => x.Key, (x) => x.Value.ToString());
                }
            }
            catch (Exception e)
            {
                serializationMessage = serializationMessage + "  Failed ** HttpRequest.HEADERS ** Msg - '" + e.Message + "'. ||| ";
                if (!digestException)
                {
                    throw e;
                }
            }

            try
            {
                if (request2 != null)
                {
                    if (request2.HasFormContentType && request2.Form != null)
                    {
                        formItems = request2.Form.Keys.Where((a) => !a.ToLower().Contains("password") || !a.ToLower().Contains("usertoken") || !a.ToLower().Contains("card") || !a.ToLower().Contains("cvv") || !a.ToLower().Contains("__VIEWSTATE")).ToDictionary((x) => x, (x) => request2.Form[x]);
                    }

                    if (addBody && request2 != null)
                    {
                        if (request2.Body != null && request2.Body.CanRead)
                        {
                            strReqBody = await request2.Body.ReadStreamAsStringIfPossible();
                        }
                        else
                        {
                            serializationMessage += "  Failed ** HttpRequest.BODY ** Cannot Read Stream  |||  ";
                        }
                    }
                }
            }
            catch (Exception e2)
            {
                serializationMessage = serializationMessage + "  Failed ** HttpRequest.BODY ** Msg - '" + e2.Message + "'. ||| ";
                if (!digestException)
                {
                    throw e2;
                }
            }

            string url = request2 == null ? "null" : request2.GetDisplayUrl();
            Dictionary<string, string> headers2 = headersToLog;
            string body = strReqBody;
            HttpRequest httpRequest = request2;
            int num;
            if (httpRequest == null)
            {
                num = 1;
            }
            else
            {
                _ = httpRequest.QueryString;
                num = 0;
            }

            string queryString = num != 0 ? "" : request2?.QueryString.Value;
            HttpRequest httpRequest2 = request2;
            int num2;
            if (httpRequest2 == null)
            {
                num2 = 1;
            }
            else
            {
                _ = httpRequest2.Host;
                num2 = 0;
            }

            string host = num2 != 0 ? "" : request2?.Host.Value;
            string remoteIpAddress = request2?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            int? remotePort = request2?.HttpContext?.Connection?.RemotePort;
            string localIpAddress = request2?.HttpContext?.Connection?.LocalIpAddress?.ToString();
            int? localPort = request2?.HttpContext?.Connection?.LocalPort;
            string method = request2?.Method?.ToString();
            long? contentLength = request2?.ContentLength;
            string? contentType = request2?.ContentType;
            string browser = request2?.Headers == null || !request2.Headers.ContainsKey("User-Agent") ? "Unknown" : request2?.Headers["User-Agent"].ToString();
            IEnumerable<KeyValuePair<string, StringValues>> forms = formItems;
            bool? isLocal = request2?.Host.Host?.Contains("localhost");
            bool? isSecureConnection = request2?.IsHttps;
            HttpRequest httpRequest3 = request2;
            return new
            {
                Url = url,
                Headers = headers2,
                Body = body,
                QueryString = queryString,
                Host = host,
                RemoteIpAddress = remoteIpAddress,
                RemotePort = remotePort,
                LocalIpAddress = localIpAddress,
                LocalPort = localPort,
                Method = method,
                ContentLength = contentLength,
                ContentType = contentType,
                Browser = browser,
                Forms = forms,
                IsLocal = isLocal,
                IsSecureConnection = isSecureConnection,
                RawUrl = httpRequest3 != null ? httpRequest3.GetEncodedPathAndQuery() : null,
                request2?.Protocol,
                NetFramework = "netstandard2.0",
                SerializationMessage = serializationMessage ?? "  Success ** (body " + (addBody ? "NOT-" : "") + "SKIPPED)."
            };
        }
    }
}
