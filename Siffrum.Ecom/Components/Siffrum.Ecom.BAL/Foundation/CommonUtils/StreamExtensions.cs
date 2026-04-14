namespace Siffrum.Ecom.BAL.Foundation.CommonUtils
{
    public static class StreamExtensions
    {
        public static async Task<object> ToCustomizedObjectToLog(this HttpRequestMessage reqMsg, bool addBody, bool addCookies, bool addSesitiveInfo = false, bool digestException = true, List<string> skipHeaderKeys = null)
        {
            Dictionary<string, string> headersToLog = null;
            string strReqBody = string.Empty;
            string serializationMessage = reqMsg == null ? "Failed ** HttpRequestMessage is NULL ** " : null;
            skipHeaderKeys = skipHeaderKeys ?? new List<string>();
            try
            {
                if (reqMsg != null)
                {
                    List<KeyValuePair<string, IEnumerable<string>>> headers = reqMsg.Headers?.ToList() ?? new List<KeyValuePair<string, IEnumerable<string>>>();
                    if (reqMsg.Content != null && reqMsg.Content.Headers.Count() > 0)
                    {
                        headers.AddRange(reqMsg.Content.Headers.ToList());
                    }

                    if (!addSesitiveInfo)
                    {
                        skipHeaderKeys.Add("authorization");
                        skipHeaderKeys.Add("originauthorization");
                        skipHeaderKeys.Add("usertoken");
                    }

                    if (!addCookies)
                    {
                        skipHeaderKeys.Add("cookies");
                    }

                    if (skipHeaderKeys.Count > 0)
                    {
                        headers.RemoveAll((x) => skipHeaderKeys.Contains(x.Key.ToLower()));
                    }

                    headersToLog = headers?.ToDictionary((x) => x.Key, (x) => string.Join(",", x.Value));
                }
            }
            catch (Exception e)
            {
                serializationMessage = serializationMessage + "  Failed ** HttpRequestMessage.HEADERS ** Msg - '" + e.Message + "'. ||| ";
                if (!digestException)
                {
                    throw e;
                }
            }

            try
            {
                if (addBody && reqMsg?.Content != null)
                {
                    strReqBody = await reqMsg.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e2)
            {
                serializationMessage = serializationMessage + "  Failed ** HttpRequestMessage.BODY ** Msg - '" + e2.Message + "'. ||| ";
                if (!digestException)
                {
                    throw e2;
                }
            }

            return new
            {
                Headers = headersToLog,
                HttpMethod = reqMsg?.Method?.ToString(),
                Url = reqMsg?.RequestUri?.ToString(),
                UrlReferrer = reqMsg?.Headers?.Referrer?.ToString(),
                Body = strReqBody,
                Version = reqMsg?.Version?.ToString(),
                reqMsg?.Properties,
                NetFramework = "netstandard2.0",
                SerializationMessage = serializationMessage ?? "  Success ** (body " + (addBody ? "NOT-" : "") + "SKIPPED)."
            };
        }

        public static async Task<object> ToCustomizedObjectToLog(this HttpResponseMessage respMsg, bool addBody, bool addCookies, bool addSesitiveInfo = false, bool digestException = true, List<string> skipHeaderKeys = null)
        {
            Dictionary<string, string> headersToLog = null;
            string strReqBody = string.Empty;
            string serializationMessage = respMsg == null ? "Failed ** HttpResponseMessage is NULL ** " : null;
            skipHeaderKeys = skipHeaderKeys ?? new List<string>();
            try
            {
                if (respMsg != null)
                {
                    List<KeyValuePair<string, IEnumerable<string>>> headers = respMsg.Headers?.ToList() ?? new List<KeyValuePair<string, IEnumerable<string>>>();
                    if (respMsg.Content != null && respMsg.Content.Headers.Count() > 0)
                    {
                        headers.AddRange(respMsg.Content.Headers.ToList());
                    }

                    if (!addSesitiveInfo)
                    {
                        skipHeaderKeys.Add("authorization");
                        skipHeaderKeys.Add("originauthorization");
                        skipHeaderKeys.Add("usertoken");
                    }

                    if (!addCookies)
                    {
                        skipHeaderKeys.Add("cookies");
                    }

                    if (skipHeaderKeys.Count > 0)
                    {
                        headers.RemoveAll((x) => skipHeaderKeys.Contains(x.Key.ToLower()));
                    }

                    headersToLog = headers?.ToDictionary((x) => x.Key, (x) => string.Join(",", x.Value));
                }
            }
            catch (Exception e)
            {
                serializationMessage = serializationMessage + "  Failed ** HttpResponseMessage.HEADERS ** Msg - '" + e.Message + "'. ||| ";
                if (!digestException)
                {
                    throw e;
                }
            }

            try
            {
                if (addBody && respMsg?.Content != null)
                {
                    strReqBody = await respMsg.Content.ReadAsStringAsync();
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
                StatusCode = respMsg?.StatusCode.ToString(),
                respMsg?.ReasonPhrase,
                Headers = headersToLog,
                Body = strReqBody,
                Version = respMsg?.Version?.ToString(),
                NetFramework = "netstandard2.0",
                SerializationMessage = serializationMessage ?? "  Success ** (body " + (addBody ? "NOT-" : "") + "SKIPPED)."
            };
        }

        public static async Task<string> ReadStreamAsStringIfPossible(this Stream stream)
        {
            string retBody = string.Empty;
            if (stream?.CanRead ?? false)
            {
                if (stream.CanSeek && stream.Position != 0)
                {
                    stream.Seek(0L, SeekOrigin.Begin);
                }

                retBody = await new StreamReader(stream).ReadToEndAsync();
            }

            return retBody;
        }
    }
}