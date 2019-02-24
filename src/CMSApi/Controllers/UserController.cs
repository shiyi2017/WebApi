using CMSApi.Models;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;
using Common;
namespace CMSApi.Controllers
{
    public class UserController : ApiController
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public LoginResult Get([FromBody]LoginDto login)
        {
            LoginResult rs = new LoginResult();
            //假设用户名为"admin"，密码为"123"
            if (login.UserName == "admin" && login.Password == "123")
            {
                try
                {
                    int Minute = Convert.ToInt32(ConfigurationManager.AppSettings["Minute"]);  //过期时间
                    //如果用户登录成功，则可以得到该用户的身份数据。当然实际开发中，这里需要在数据库中获得该用户的角色及权限
                    AuthInfo authInfo = new AuthInfo
                    {
                        IsAdmin = true,
                        Roles = new List<string> { "admin", "owner" },
                        UserName = "admin",
                        LastTime=DateTime.Now.AddMinutes(Minute).ToString()
                    };
                    //生成token,SecureKey是配置的web.config中，用于加密token的key，打死也不能告诉别人
                    byte[] key = Encoding.Default.GetBytes(ConfigurationManager.AppSettings["SecureKey"]);
                    //采用HS256加密算法
                    IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
                    IJsonSerializer serializer = new JsonNetSerializer();
                    IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                    IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
                    string token = encoder.Encode(authInfo, key);
                    rs.Token = token;
                    rs.Success = true;
                    //存储token做单点登录
                    RedisHelper.StringSet(authInfo.UserName, token, Minute);
                    //储存token不做单点登录
                    RedisHelper.StringSet(token, "token", Minute);
                }
                catch
                {
                    rs.Success = false;
                    rs.Message = "登陆失败";
                }
            }
            else
            {
                rs.Success = false;
                rs.Message = "用户名或密码不正确";
            }
            return rs;
        }

        /// <summary>
        /// 用户注销
        /// </summary>
        /// <returns></returns>
        [ApiAuthorize]
        public bool Delete()
        {
            AuthInfo info = RequestContext.RouteData.Values["auth"] as AuthInfo;
            string token =  RequestContext.RouteData.Values["token"] as string;
            RedisHelper.StringRemove(info.UserName);
            return RedisHelper.StringRemove(token);
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="value"></param>
        public void Post([FromBody]string value)
        {



        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}