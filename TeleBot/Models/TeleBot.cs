namespace TeleBot.Models
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Telegram.Bot;

    public class TelegaBot
    {
        private const string tempImageName = "temp.jpg";
        private string path;
        private TelegramBotClient client;
        private TaskCompletionSource<bool> isBusy;

        private static TelegaBot instance;

        public static TelegaBot Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TelegaBot();
                }

                return instance;
            }
        }

        public async Task<TelegramBotClient> Get()
        {
            if (this.client != null)
            {
                return this.client;
            }

            this.client = new TelegramBotClient(AppSettings.Key);

#if DEBUG
            await this.client.SetWebhookAsync();
            this.client.StartReceiving();
            this.client.OnMessage += this.OnMessageReceived;
#else          
            var hook = string.Format(AppSettings.Url, "api/message/update");
            await this.client.SetWebhookAsync(hook);
#endif

            return this.client;
        }

#if DEBUG
        private async void OnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            await this.OnMessageReceived(e.Message);
        }
#endif

        public async Task OnMessageReceived(Telegram.Bot.Types.Message message)
        {
            if (!string.IsNullOrEmpty(message?.Text))
            {
                if (this.isBusy != null)
                {
                    await this.isBusy.Task;
                }

                this.isBusy = new TaskCompletionSource<bool>();

                var analyzedText = new AnalyzedText(message.Text);

                if (!string.IsNullOrEmpty(analyzedText?.Answer))
                {
                    try
                    {
                        if (analyzedText.IsHello)
                        {
                            await this.client.SendTextMessageAsync(message.Chat.Id, analyzedText.Answer, replyToMessageId: message.MessageId);
                        }
                        else if (analyzedText.IsSwearWord)
                        {
                            var url = Google.Instance.Search(analyzedText.Answer);

                            if (!string.IsNullOrEmpty(url))
                            {
                                var imageSource = await GetImage(url);

                                if (!string.IsNullOrEmpty(imageSource))
                                {
                                    using (var fileStream = File.Open(this.path, FileMode.Open))
                                    {
                                        await this.client.SendTextMessageAsync(message.Chat.Id, "сам ты " + analyzedText.Answer, replyToMessageId: message.MessageId);
                                        var file = new Telegram.Bot.Types.FileToSend(tempImageName, fileStream);
                                        await this.client.SendPhotoAsync(message.Chat.Id, file, "вот такой вот");
                                    }
                                }
                            }

                        }
                    }
                    catch (Exception)
                    {
                        //TODO: log
                    }
                }
            }


            this.isBusy.TrySetResult(false);
        }

        private async Task<string> GetImage(string url)
        {
            var result = string.Empty;

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    this.path = Path.Combine(Path.GetTempPath(), tempImageName);

                    using (Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                           stream = new FileStream(path, FileMode.Create))
                    {
                        result = this.path;
                        await contentStream.CopyToAsync(stream);

                    }
                }
            }

            return result;
        }
    }
}