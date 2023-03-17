using Caro.Game.DBAccess.DAO;
using Caro.Game.DBAccess.Factory;
using Caro.Game.DBAccess.Model;
using Caro.Game.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Caro.Game.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IGameDAO gameDAO = AbstractFactoryDAO.Instance().CreateGameDAO(Startup.AppSettings);
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Authenticate(dynamic data)
        {
            string strData = data.ToString();
            var jobject = JObject.Parse(strData);
            string _username = jobject["username"].ToString();
            string _password = jobject["password"].ToString();


            DBResult<long> Dbres = gameDAO.Authenticated(_username, _password);

            BaseResponse response = new BaseResponse()
            {
                status = Dbres.ResponseStatus,
                data = new
                {
                    message = Dbres.ResponseText.ToString()
                }
            };

            if (response.status == 1)
            {
                try
                {
                   
                    // authentication successful so generate jwt token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(Startup.AppSettings.SecretKey);
                    var name = Dbres.Data.ToString();
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, name)
                        }),
                        Expires = DateTime.UtcNow.AddHours(5),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenRes = tokenHandler.WriteToken(token);



                    response.data = new
                    {
                        Token = tokenRes,
                        AccountId = Dbres.Data
                    };
                }
                catch (Exception ex)
                {
                    response.status = -99;
                    response.data = new
                    {
                        message = ex.Message.ToString()
                    };
                }
            }
            return Ok(response);
        }
    }
}
