using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace JETWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TokenController : ControllerBase
    {
        private readonly string _issuer;
        private readonly string _signKey;
        IHttpContextAccessor _httpContextAccessor;

        private TokenManager _tokenManager;

        public TokenController(IConfiguration configuration, 
                                IHttpContextAccessor httpContextAccessor)
        {
            var jwtConfig = configuration.GetSection("JWT");

            _issuer = jwtConfig["issuer"];
            _signKey = jwtConfig["signKey"];

            _httpContextAccessor = httpContextAccessor;

            _tokenManager = new TokenManager();
        }

        //紀錄 Refresh Token，需紀錄在資料庫
        private static Dictionary<string, User> refreshTokens =
            new Dictionary<string, User>();

        //登入
        //[HttpPost]
        //[Route("signIn")]
        [AllowAnonymous]
        [HttpPost("~/signin")]
        public Token SignIn(SignInViewModel model)
        {
            //模擬從資料庫取得資料
            if (!(model.UserId == "abc" && model.Password == "123"))
            {
                throw new Exception("登入失敗，帳號或密碼錯誤");
            }
            var user = new User
            {
                Id = 1,
                UserId = "abc",
                UserName = "小明",
                //Identity = Identity.User
            };
            //產生 Token
            var token = _tokenManager.Create(user);
            //需存入資料庫
            refreshTokens.Add(token.refresh_token, user);
            return token;
        }

        //換取新 Token
        //[HttpPost]
        //[Route("refresh")]
        [AllowAnonymous]
        [HttpPost("~/refresh")]
        public Token Refresh(RefreshTokenModel model)
        {
            //檢查 Refresh Token 
            if (model == null)
            {
                throw new Exception("Refresh Token Model is Null");
            }
            //檢查 Refresh Token 是否正確
            if (!refreshTokens.ContainsKey(model.refreshToken))
            {
                throw new Exception("查無此 Refresh Token");
            }
            //需查詢資料庫
            var user = refreshTokens[model.refreshToken];
            //產生一組新的 Token 和 Refresh Token
            var token = _tokenManager.Create(user);
            //刪除舊的
            refreshTokens.Remove(model.refreshToken);
            //存入新的
            refreshTokens.Add(token.refresh_token, user);
            return token;
        }

        //測試是否通過驗證
        //[HttpPost]
        //[Route("isAuthenticated")]
        [AllowAnonymous]
        [HttpPost("~/isAuthenticated")]
        public bool IsAuthenticated()
        {
            var user = _tokenManager.GetUser(_httpContextAccessor);
            if (user == null)
            {
                return false;
            }
            return true;
        }

        // GET: api/Token
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Token/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Token
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Token/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class SignInViewModel
    {
        public string UserId { get; set; }

        public string Password { get; set; }
    }

    public class RefreshTokenModel
    {
        public string refreshToken { get; set; }
    }
}
