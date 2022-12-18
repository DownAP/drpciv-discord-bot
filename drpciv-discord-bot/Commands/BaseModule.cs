using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace drpciv_discord_bot.Commands
{
    public class BaseModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Author id 
        public static ulong uid;

        //Previous message id
        public static ulong userMessage;

        public static ulong threadId;
        public static questionsClass[] questionsJson = JsonConvert.DeserializeObject<questionsClass[]>(System.IO.File.ReadAllText("questions.json"));

        public static int iQuestion;
        public static Dictionary<ulong, int> mActive = new Dictionary<ulong, int>();
        public static Dictionary<ulong, int> mQuestionsLeft = new Dictionary<ulong, int>();
        public static Dictionary<ulong, int> mCurrentQuestion = new Dictionary<ulong, int>();
        public static Dictionary<ulong, int> mCorrectAnswers = new Dictionary<ulong, int>();
        public static Dictionary<ulong, int> mWrongAnswers = new Dictionary<ulong, int>();
        public static Dictionary<ulong, (string, string)> mWrongAnswersList = new Dictionary<ulong, (string, string)>();


        [MessageCommand("Start Exam")]
        public async Task HandleDRPCIVCommand(IMessage message)
        {
            var channel = Context.Channel as ITextChannel;
            var examThread = await channel.CreateThreadAsync(
                name: $"{Context.User.Username}-{new Random().Next(0, 9999)}",
                autoArchiveDuration: ThreadArchiveDuration.OneHour,
                invitable: false,
                type: ThreadType.PublicThread);
            await examThread.SendMessageAsync("Examen inceput!");
            threadId = examThread.Id;

            await aDrpciv(Context);

        }


        [SlashCommand("drpciv", "Incepe un chestionar.")]
        public async Task HandleDRPCIV()
        {
            var channel = Context.Channel as ITextChannel;
            var examThread = await channel.CreateThreadAsync(
                name: $"{Context.User.Username}-{new Random().Next(0, 9999)}",
                autoArchiveDuration: ThreadArchiveDuration.OneHour,
                invitable: false,
                type: ThreadType.PublicThread);
            await examThread.SendMessageAsync("Examen inceput!");
            threadId = examThread.Id;

            await aDrpciv(Context);

        }

        public static async Task aDrpciv(Discord.Interactions.SocketInteractionContext Context)
        {
            if (!mActive.ContainsKey(Context.User.Id))
                mActive.Add(Context.User.Id, 1);

            if (!mQuestionsLeft.ContainsKey(Context.User.Id))
                mQuestionsLeft.Add(Context.User.Id, 26);

            if (!mCurrentQuestion.ContainsKey(Context.User.Id))
                mCurrentQuestion.Add(Context.User.Id, 1);

            if (!mCorrectAnswers.ContainsKey(Context.User.Id))
                mCorrectAnswers.Add(Context.User.Id, 0);

            if (!mWrongAnswers.ContainsKey(Context.User.Id))
                mWrongAnswers.Add(Context.User.Id, 0);


            // Get a random question from the array
            var random = new Random((int)DateTimeOffset.Now.ToUnixTimeSeconds());
            iQuestion = random.Next(0, 1100);

            // Remove artifacts from the scrapped data
            string strPattern = @"([^\/]+(jpg|png))";
            Regex regex = new Regex(strPattern);
            MatchCollection matchCollection = regex.Matches((questionsJson[iQuestion].QuestionName));


            string[] strAnswers = questionsJson[iQuestion].Answer.Split(",");

            string kkk = "";
            foreach (var k in strAnswers)
                kkk += k + ", ";

            var embedd = new EmbedBuilder()
            .WithAuthor("Intrebare #" + mActive[Context.User.Id])
            .WithTitle(questionsJson[iQuestion].QuestionName.Split("<", 2)[0])
            .AddField("Optiuni", Emoji.Parse(":regional_indicator_a:").ToString() + (questionsJson[iQuestion].Answer1.Split("A.", 2))[1] + "\n" +
                                                Emoji.Parse(":regional_indicator_b:").ToString() + (questionsJson[iQuestion].Answer2.Split("B.", 2))[1] + "\n" +
                                                Emoji.Parse(":regional_indicator_c:").ToString() + (questionsJson[iQuestion].Answer3.Split("C.", 2))[1] + "\n  ")
            .WithFooter(footer => footer.Text = $"Corecte: {mCorrectAnswers[Context.User.Id]}  |  Gresite: {mWrongAnswers[Context.User.Id]}  |  {26 - mQuestionsLeft[Context.User.Id]}/26");

            if (matchCollection.Count > 0)
                embedd.ImageUrl = "https://down.monster/drpciv/" + matchCollection[0].ToString();


            var emoji = new Emoji("\uD83D\uDC4C");
            var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Alege raspunsul")
            .WithCustomId("question-menu")
            .WithMinValues(1)
            .WithMaxValues(3)
            .AddOption(" ", "A", "Alege raspunsul A", Emoji.Parse(":regional_indicator_a:"))
            .AddOption(" ", "B", "Alege raspunsul B", Emoji.Parse(":regional_indicator_b:"))
            .AddOption(" ", "C", "Alege raspunsul C", Emoji.Parse(":regional_indicator_c:"))
            .AddOption("Treci peste intrebare", "SKIP", " ", Emoji.Parse(":arrow_right:"));

            var builder = new ComponentBuilder()
                .WithSelectMenu(menuBuilder);

            await Context.Interaction.DeferAsync();
            await Context.Guild.GetThreadChannel(threadId).SendMessageAsync(null, embed: embedd.Build(), components: builder.Build());
        }

        [SlashCommand("stats", "Statisticile tale")]
        public async Task HandleStats()
        {
            int completed = 0, failed = 0;

            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand($"SELECT * FROM `users` WHERE `uid` = {Context.User.Id};");
                MySqlDataReader dataReader;
                await Program.mysqlConnection.OpenAsync();
                mySqlCommand.Connection = Program.mysqlConnection;
                dataReader = await mySqlCommand.ExecuteReaderAsync();
                while (dataReader.Read())
                {
                    completed = (int)dataReader["completed"];
                    failed = (int)dataReader["failed"];
                }
                await dataReader.CloseAsync();
                await Program.mysqlConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                await RespondAsync("Oopsie! A plecat baza de date :(");
            }
            var embedd = new EmbedBuilder()
                .AddField("Statistici", $"{Context.User.Username}, ai la activ {completed} chestionare completate si {failed} chestionare picate.")
                .WithThumbnailUrl(Context.User.GetAvatarUrl());


            await RespondAsync(null, embed: embedd.Build());
        }

        [ComponentInteraction("question-menu")]
        public async Task HandleDRPCIVSelection(string[] inputs)
        {
            //Prevent others from interacting with the select menu
            if (Context.User.Id != uid)
            {
                await Context.User.SendMessageAsync($"Nu poti interactiona cu chestionarul lui {Context.Client.GetUser(uid).Username}.\nPoti incepe un chestionar folosind ```/drpciv```");
                return;
            }

            string[] strAnswers = questionsJson[iQuestion].Answer.Split(",");

            if (Array.Exists(inputs, element => element == "A"))                                  //A
            {
                if (Array.Exists(inputs, element => element == "B"))    //B
                {
                    if (Array.Exists(inputs, element => element == "C"))                          //ABC
                    {
                        if (int.Parse(strAnswers[0]) == 0 && strAnswers.Length == 3 && int.Parse(strAnswers[1]) == 1 && int.Parse(strAnswers[2]) == 2)
                            await SubmitAnswer(questionsJson[iQuestion].Answer, 1);
                        else
                            await SubmitAnswer(questionsJson[iQuestion].Answer, 0);
                    }
                    else if (int.Parse(strAnswers[0]) == 0 && strAnswers.Length == 2 && int.Parse(strAnswers[1]) == 1)           //AB
                        await SubmitAnswer(questionsJson[iQuestion].Answer, 1);
                    else
                        await SubmitAnswer(questionsJson[iQuestion].Answer, 0);
                }
                else if (Array.Exists(inputs, element => element == "C"))                     //AC
                {
                    if (int.Parse(strAnswers[0]) == 0 && strAnswers.Length == 2 && int.Parse(strAnswers[1]) == 2)
                        await SubmitAnswer(questionsJson[iQuestion].Answer, 1);
                    else
                        await SubmitAnswer(questionsJson[iQuestion].Answer, 0);
                }
                else if (int.Parse(strAnswers[0]) == 0 && strAnswers.Length == 1)
                    await SubmitAnswer(questionsJson[iQuestion].Answer, 1);
                else
                    await SubmitAnswer(questionsJson[iQuestion].Answer, 0);
            }
            else if (Array.Exists(inputs, element => element == "B"))                 //B
            {
                if (Array.Exists(inputs, element => element == "C"))                  //BC
                {
                    if (int.Parse(strAnswers[0]) == 1 && strAnswers.Length == 2)
                        await SubmitAnswer(questionsJson[iQuestion].Answer, 1);
                    else
                        await SubmitAnswer(questionsJson[iQuestion].Answer, 0);
                }
                else if (int.Parse(strAnswers[0]) == 1 && strAnswers.Length == 1)
                    await SubmitAnswer(questionsJson[iQuestion].Answer, 1);
                else
                    await SubmitAnswer(questionsJson[iQuestion].Answer, 0);
            }
            else if (Array.Exists(inputs, element => element == "C"))
            {
                if (int.Parse(strAnswers[0]) == 2 && strAnswers.Length == 1)
                    await SubmitAnswer(questionsJson[iQuestion].Answer, 1);
                else
                    await SubmitAnswer(questionsJson[iQuestion].Answer, 0);
            }
            else if (Array.Exists(inputs, element => element == "SKIP") && strAnswers.Length == 1)
            {
                await Context.Channel.DeleteMessageAsync(userMessage);
            }



            //Check whether the user failed the exam
            var congratsOrNot = new EmbedBuilder();
            if (mWrongAnswers[Context.User.Id] == 5)
            {
                congratsOrNot.AddField("Respins!", $"{Context.User.Username}, din pacate ai picat :(\nThread-ul va fi sters in 5 secunde!");
                Task.Delay(5000).ContinueWith(t => Context.Guild.GetThreadChannel(threadId).DeleteAsync());
                SubmitSql("failed");

                mQuestionsLeft.Remove(Context.User.Id);
                mCorrectAnswers.Remove(Context.User.Id);
                mWrongAnswers.Remove(Context.User.Id);
                mActive.Remove(Context.User.Id);
                await RespondAsync(null, embed: congratsOrNot.Build());
                await Context.Channel.DeleteMessageAsync(userMessage);
                return;
            }
            else if (mQuestionsLeft[Context.User.Id] == 0 && mCorrectAnswers[Context.User.Id] >= 22)
            {
                congratsOrNot.AddField("Admis!", $"Felicitari, {Context.User.Username}, ai obtinut {mCorrectAnswers[Context.User.Id]++} puncte!\nThread-ul va fi sters in 5 secunde!");
                Task.Delay(5000).ContinueWith(t => Context.Guild.GetThreadChannel(threadId).DeleteAsync());
                SubmitSql("completed");


                mQuestionsLeft.Remove(Context.User.Id);
                mCorrectAnswers.Remove(Context.User.Id);
                mWrongAnswers.Remove(Context.User.Id);
                mActive.Remove(Context.User.Id);
                await RespondAsync(null, embed: congratsOrNot.Build());
                await Context.Channel.DeleteMessageAsync(userMessage);
                return;
            }

            mActive[Context.User.Id]++;
            await aDrpciv(Context);
        }


        private async Task SubmitAnswer(string answer, int iAnswer)
        {
            if (mWrongAnswers[Context.User.Id] != 5)
                await Context.Channel.DeleteMessageAsync(userMessage);

            await ReplyAsync("", embed: AnswerEmbed(answer).Build());
            mQuestionsLeft[Context.User.Id]--;

            iAnswer = iAnswer == 1 ? mCorrectAnswers[Context.User.Id]++ : mWrongAnswers[Context.User.Id]++;
        }

        private EmbedBuilder AnswerEmbed(string answer)
        {
            string strFooter = "";
            strFooter += $"{(answer.Contains("0") ? Emoji.Parse(":white_check_mark:") : Emoji.Parse(":negative_squared_cross_mark:"))} {Emoji.Parse(":regional_indicator_a:")} {questionsJson[iQuestion].Answer1.Split("A.", 2)[1]} \n";
            strFooter += $"{(answer.Contains("1") ? Emoji.Parse(":white_check_mark:") : Emoji.Parse(":negative_squared_cross_mark:"))} {Emoji.Parse(":regional_indicator_b:")} {questionsJson[iQuestion].Answer2.Split("B.", 2)[1]} \n";
            strFooter += $"{(answer.Contains("2") ? Emoji.Parse(":white_check_mark:") : Emoji.Parse(":negative_squared_cross_mark:"))} {Emoji.Parse(":regional_indicator_c:")} {questionsJson[iQuestion].Answer3.Split("C.", 2)[1]} \n";

            return new EmbedBuilder().WithTitle((questionsJson[iQuestion].QuestionName.Split("<", 2))[0])
            .WithFooter(footer => footer.Text = strFooter);
        }

        private async void SubmitSql(string side)
        {
            try
            {
                MySqlCommand updateCommand = new MySqlCommand($"INSERT INTO users(uid, {side}) VALUES({Context.User.Id}, 1) ON DUPLICATE KEY UPDATE {side} = {side} + 1;", Program.mysqlConnection);
                MySqlDataReader dataReader;
                await Program.mysqlConnection.OpenAsync();
                dataReader = await updateCommand.ExecuteReaderAsync();
                await Program.mysqlConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                await RespondAsync("Oopsie! A plecat baza de date :(");
            }
        }
    }

}
