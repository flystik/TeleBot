namespace TeleBot.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Results;
    using Telegram.Bot.Types;
    using TeleBot.Models;

    public class MessageController : ApiController
    {
        [Route("api/message/update")] //webhook uri part
        public async Task<OkResult> Update([FromBody]Update update)
        {
            if (update?.Message != null)
            {
                await TelegaBot.Instance.OnMessageReceived(update.Message);
            }

            return Ok();
        }
    }
}

