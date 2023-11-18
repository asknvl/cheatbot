using Avalonia.Controls;
using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.ViewModels.events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class mainVM : LifeCycleViewModelBase
    {
        #region vars
        DataBaseContext db = new DataBaseContext();
        #endregion

        #region properties
        loggerVM logger;
        public loggerVM Logger
        {
            get => logger;
            set => this.RaiseAndSetIfChanged(ref logger, value);
        }

        dropListVM dropList;
        public dropListVM DropList
        {
            get => dropList;
            set => this.RaiseAndSetIfChanged(ref dropList, value);
        }

        channelListVM channelList;
        public channelListVM ChannelList
        {
            get => channelList;
            set => this.RaiseAndSetIfChanged(ref channelList, value);
        }
        #endregion

        public mainVM()
        {
            //db.Database.EnsureCreated();

            initUserApiKeys().Wait();
            Logger = new loggerVM();

            //WTelegram.Helpers.Log = (x, y) =>
            //{
            //    Logger.dbg("API", $"{y}");
            //};

            DropList = new dropListVM(Logger);


            DropList.ChannelAddedEvent += (link, id, name) => { 
                ChannelList.updateChannelInfo(link, id, name);
            };

            ChannelList = new channelListVM();
            ChannelList.SubscribeAllRequestEvent += async (link) => {
                await Task.Run(async () => {
                    await DropList.subscribeAll(link);
                });            
            };

        }

        #region private
        async Task initUserApiKeys()
        {
            ApiSettings settings = new ApiSettings()
            {
                id = 1,
                group = 0,
                phone_number = "66620910934",
                api_id = "27228967",
                api_hash = "a8396cb1e06febf3fb5dec8e9d16c0fd",
                password = "5555"
            };

            if (!db.ApiSettings.Any(s => s.id == settings.id))
            {
                db.ApiSettings.Add(settings);
                db.SaveChanges();
            }
        }
        #endregion
    }
}
