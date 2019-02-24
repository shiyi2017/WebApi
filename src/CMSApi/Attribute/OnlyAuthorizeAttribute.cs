using CMSApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using JWT;
using JWT.Serializers;
using Common;
namespace CMSApi
{
    public class OnlyAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            //前端请求api时会将token存放在名为"auth"的请求头中
            var authHeader = from h in actionContext.Request.Headers where h.Key == "auth" select h.Value.FirstOrDefault();
            if (authHeader == null)
            {
                return false;
            }

            string token = authHeader.FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
               return false;
            }

            if (string.IsNullOrEmpty(RedisHelper.StringGet(token)))
            {
                return false;
            }

            try
            {
                //对token进行解密
                string secureKey = System.Configuration.ConfigurationManager.AppSettings["SecureKey"];

                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

                AuthInfo authInfo = decoder.DecodeToObject<AuthInfo>(token, secureKey, verify: true);
                if (authInfo != null)
                {
                    if (Convert.ToDateTime(authInfo.LastTime) > DateTime.Now)  //是否过期
                    {
                        if (RedisHelper.StringGet(authInfo.UserName) == token)  //唯一登录验证
                        {
                            //将用户信息存放起来，供后续调用
                            actionContext.RequestContext.RouteData.Values.Add("auth", authInfo);
                            actionContext.RequestContext.RouteData.Values.Add("token", token);
                            return true;
                        }
                        else
                            return false;

                    }
                    return false;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }


        }
    }
}