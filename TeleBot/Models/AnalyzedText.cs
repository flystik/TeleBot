namespace TeleBot.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class AnalyzedText
    {
        private const string botString = "бот";

        //TODO: searching by google or db
        private List<string> helloBieWords = new List<string>()
        {
            "привет",
            "пока",
            "доброй ночи",
            "спокойной ночи",
            "сладких снов",
            "здарова",
            "hello",
            "hi",
            "доброе утро"
        };

        public AnalyzedText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                this.Text = text;
                this.CheckMessage();
            }
        }

        public string Text { get; set; }

        public string Answer { get; private set; } = string.Empty;

        public bool IsHello { get; set; }

        public bool IsSwearWord { get; set; }

        public bool IsGoTo { get; set; }

        public bool IsQuestion { get; set; }

        private void CheckMessage()
        {
            var messageLower = this.Text.ToLower();

            if (messageLower.Contains(botString))
            {
                messageLower = messageLower.Remove(messageLower.IndexOf(botString), botString.Length);

                this.SwearWordsCheck(messageLower, " ты ");

                this.HelloWordsCheck(messageLower);             
            }
        }

        private void HelloWordsCheck(string text)
        {
            foreach (string helloBieWord in this.helloBieWords)
            {
                if (text.Contains(helloBieWord))
                {
                    this.IsHello = true;

                    if (this.IsSwearWord)
                    {
                        this.Answer = "fuck you";
                    }
                    else
                    {
                        this.Answer = helloBieWord + " xD";
                    }

                    break;
                }
            }
        }

        private void SwearWordsCheck(string text, string pattern)
        {
            if (text.Contains(pattern))
            {
                var textPart = text.Remove(0, text.IndexOf(pattern) + pattern.Length);

                var strings =  textPart.Split(' ');

                if (strings != null && strings.Any() && !string.IsNullOrEmpty(strings[0]))
                {
                    this.Answer = strings[0];

                    this.IsSwearWord = true;
                }
            }
        }
    }
}