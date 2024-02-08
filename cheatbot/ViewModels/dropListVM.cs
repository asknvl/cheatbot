using asknvl;
using asknvl.logger;
using Avalonia.Controls;
using Avalonia.Threading;
using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.Models.cleaner;
using cheatbot.Models.drop;
using cheatbot.Models.files;
using cheatbot.ViewModels.events;
using cheatbot.WS;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TL;

namespace cheatbot.ViewModels
{
    public class dropListVM : ViewModelBase, IEventSubscriber<BaseEventMessage>
    {
        #region vars
        ILogger logger;
        DropCleaner cleaner;
        #endregion

        #region properties
        List<GroupModel> groups;
        public List<GroupModel> Groups
        {
            get => groups;
            set
            {
                this.RaiseAndSetIfChanged(ref groups, value);
            }
        }

        GroupModel selectedGroup;
        public GroupModel SelectedGroup
        {
            get => selectedGroup;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedGroup, value);
                updateViewedDrops();
            }
        }

        int onlineCount;
        public int OnlineCount
        {
            get => onlineCount;
            set => this.RaiseAndSetIfChanged(ref onlineCount, value);
        }

        public ObservableCollection<dropVM> DropList { get; } = new();
        public ObservableCollection<dropVM> ViewedDropList { get; } = new();

        dropVM selectedDrop;
        public dropVM SelectedDrop
        {
            get => selectedDrop;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedDrop, value);
                SubContent = selectedDrop;
            }
        }

        object subcontent;
        public object SubContent
        {
            get => subcontent;
            set => this.RaiseAndSetIfChanged(ref subcontent, value);
        }

        string _old2FA = "Wf52";
        public string Old2FA
        {
            get => _old2FA;
            set => this.RaiseAndSetIfChanged(ref _old2FA, value);
        }

        string _new2FA;
        public string New2FA
        {
            get => _new2FA;
            set => this.RaiseAndSetIfChanged(ref _new2FA, value);
        }

        string rootPath;
        public string RootPath
        {
            get => rootPath;
            set => this.RaiseAndSetIfChanged(ref rootPath, value);
        }

        string tgpath;
        public string TGPath
        {
            get => tgpath;
            set => this.RaiseAndSetIfChanged(ref tgpath, value);
        }

        string? proxyString;
        public string? ProxyString
        {
            get => proxyString;
            set => this.RaiseAndSetIfChanged(ref proxyString, value); 
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        public ReactiveCommand<Unit, Unit> deleteCmd { get; }
        public ReactiveCommand<Unit, Unit> set2FACmd { get; }
        public ReactiveCommand<Unit, Unit> startGroupCmd { get; }
        public ReactiveCommand<Unit, Unit> stopGroupCmd { get; }
        public ReactiveCommand<Unit, Unit> loadNewCmd { get; }
        public ReactiveCommand<Unit, Unit> setRootPathCmd { get; }
        public ReactiveCommand<Unit, Unit> setTGPathCmd { get; }
        public ReactiveCommand<Unit, Unit> setProxyCmd { get; }
        public ReactiveCommand<Unit, Unit> closeTGsCmd { get; }
        public ReactiveCommand<Unit, Unit> startAllCmd { get; }
        public ReactiveCommand<Unit, Unit> stopAllCmd { get; }        
        #endregion
        public dropListVM(ILogger logger)
        {

            InitAppSettings();

            this.logger = logger;

            cleaner = new DropCleaner(logger);

            EventAggregator.getInstance().Subscribe(this);

            Task.Run(async () =>
            {
                await loadDropList(logger);
                await loadGroups();


                EventAggregator.getInstance().Publish((BaseEventMessage)new DropListUpdatedEventMessage(0, DropList));

                //foreach (var group in Groups)
                //{
                //    EventAggregator.getInstance().Publish((BaseEventMessage)new DropListUpdatedEventMessage(group.id, DropList));
                //}

            });

            #region commands
            loadNewCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                var gdir = Path.Combine(RootPath, "" + SelectedGroup.id);
                var subdirs = Directory.GetDirectories(gdir);

                using (var db = new DataBaseContext())
                {
                    foreach (var subdir in subdirs)
                    {
                        var drops = db.Drops.ToList();
                        string fn = Path.GetFileName(subdir);

                        var phone_number = $"+{fn}";
                        var found = drops.Any(d => d.phone_number.Equals(phone_number));
                        if (!found)
                        {
                            var istg = File.Exists(Path.Combine(RootPath, $"{SelectedGroup.id}", fn, "Telegram.exe"));
                            
                            if (!istg)
                                Files.CopyDir(TGPath, subdir);
                            
                            var dropModel = new DropModel()
                            {
                                phone_number = phone_number,
                                _2fa_password = Old2FA,
                                group_id = SelectedGroup.id
                            };

                            db.Drops.Add(dropModel);
                            db.SaveChanges();
                            await addDrop(dropModel);

                            try
                            {

                                Process.Start(Path.Combine(subdir, "Telegram.exe"));

                            } catch (Exception ex)
                            {

                            }

                        }
                    }
                }
                await updateViewedDrops();
            });

            closeTGsCmd = ReactiveCommand.Create(() => {

                Process[] GetPArry = Process.GetProcesses();
                foreach (Process testProcess in GetPArry)
                {
                    string ProcessName = testProcess.ProcessName;

                    ProcessName = ProcessName.ToLower();
                    if (ProcessName.CompareTo("telegram") == 0)
                        testProcess.Kill();
                }

            });

            setRootPathCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                var result = await WindowService.getInstance().ShowFolderDialog();
                if (result != null)
                {
                    using (var db = new DataBaseContext())
                    {
                        AppSettings? found = db.AppSettings.FirstOrDefault();
                        if (found == null)
                        {
                            found = new AppSettings();
                            db.AppSettings.Add(found);
                        }
                        found.RootPath = result;
                        db.SaveChanges();
                    }
                    RootPath = result;
                }
            });

            setTGPathCmd = ReactiveCommand.CreateFromTask(async () =>
            {

                var result = await WindowService.getInstance().ShowFolderDialog();
                if (result != null)
                {
                    using (var db = new DataBaseContext())
                    {
                        AppSettings? found = db.AppSettings.FirstOrDefault();
                        if (found == null)
                        {
                            found = new AppSettings();
                            db.AppSettings.Add(found);
                        }
                        found.TGPath = result;
                        db.SaveChanges();
                    }
                    TGPath = result;
                }
            });

            setProxyCmd = ReactiveCommand.Create(() => {

                using (var db = new DataBaseContext())
                {
                    AppSettings? found = db.AppSettings.FirstOrDefault();
                    if (found == null)
                    {
                        found = new AppSettings();
                        db.AppSettings.Add(found);  
                    }
                    found.ProxyString = ProxyString;
                    db.SaveChanges();
                }
            });

            addCmd = ReactiveCommand.Create(() =>
            {
                addDropVM addVM = new addDropVM(SelectedGroup.id);
                SubContent = addVM;
            });

            deleteCmd = ReactiveCommand.CreateFromTask(async () =>
            {

                if (SelectedDrop == null)
                    return;

                using (var db = new DataBaseContext())
                {
                    var found_db = db.Drops.FirstOrDefault(d => d.phone_number.Equals(SelectedDrop.phone_number));
                    db.Remove(found_db);

                    var found_subs = db.DropSubscribes.Where(ds => ds.drop_id == SelectedDrop.id);
                    db.RemoveRange(found_subs);

                    db.SaveChanges();
                }

                var found_list = DropList.FirstOrDefault(d => d.phone_number.Equals(SelectedDrop.phone_number));
                DropList.Remove(found_list);

                await updateViewedDrops();

            });

            set2FACmd = ReactiveCommand.Create(() =>
            {

                //throw new NotImplementedException();

                if (SelectedDrop != null)
                    EventAggregator.getInstance().Publish((BaseEventMessage)new Change2FAPasswordOneEventMessage(SelectedDrop.phone_number, Old2FA, New2FA));
                else
                    EventAggregator.getInstance().Publish((BaseEventMessage)new Change2FAPasswordAllEventMessage(Old2FA, New2FA));
            });

            startGroupCmd = ReactiveCommand.CreateFromTask(async () =>
            {

                foreach (var drop in ViewedDropList)
                {
                    drop.startCmd.Execute();
                }

                EventAggregator.getInstance().Publish((BaseEventMessage)new GroupStartedEventMessage(SelectedGroup.id));

            });

            stopGroupCmd = ReactiveCommand.CreateFromTask(async () =>
            {

                foreach (var drop in ViewedDropList)
                {
                    drop.stopCmd.Execute();
                }

                EventAggregator.getInstance().Publish((BaseEventMessage)new GroupStoppedEventMessage(SelectedGroup.id));
            });

            startAllCmd = ReactiveCommand.CreateFromTask(async () => { 
            
                foreach (var drop in DropList)
                {
                    drop.startCmd.Execute();
                }            
            });

            stopAllCmd = ReactiveCommand.CreateFromTask(async () => { 
                foreach (var drop in DropList)
                {
                    drop.stopCmd.Execute();
                }
            });

            #endregion
        }

        #region private
        async Task loadDropList(ILogger logger)
        {
            await Task.Run(async () =>
            {
                DropList.Clear();

                List<DropModel> dropModels = new();

                using (var db = new DataBaseContext())
                {
                    //if (model.id == 1)
                    //    drops = db.Drops.ToList();
                    //else
                    //    drops = db.Drops.Where(d => d.group_id == model.id).ToList();

                    dropModels = db.Drops.ToList();

                }

                foreach (var dropModel in dropModels)
                {
                    await addDrop(dropModel);
                }
            });

        }

        async Task updateViewedDrops()
        {
            await Task.Run(() =>
            {
                ViewedDropList.Clear();

                if (SelectedGroup != null)
                {
                    var viewedDrops = DropList.Where(d => d.group_id == SelectedGroup.id);
                    foreach (var drop in viewedDrops)
                    {
                        ViewedDropList.Add(drop);
                    }
                }
            });
        }

        async Task loadGroups()
        {
            await Task.Run(() =>
            {
                using (var db = new DataBaseContext())
                {
                    Groups = db.Groups.ToList();
                    if (Groups.Count > 0)
                        SelectedGroup = Groups[0];
                }
            });
        }
        #endregion

        #region helpers
        void InitAppSettings()
        {
            using (var db = new DataBaseContext())
            {
                var found = db.AppSettings.FirstOrDefault();
                if (found != null)
                {
                    RootPath = found.RootPath;
                    TGPath = found.TGPath;
                    ProxyString = found.ProxyString;
                }
            }
        }
        async Task addDrop(DropModel model)
        {
            var dvm = new dropVM(model, logger);
            dvm.ChannelAddedEvent += (link, id, name) =>
            {
                ChannelAddedEvent?.Invoke(link, id, name);
            };
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                DropList.Add(dvm);
            });
        }

        #endregion

        #region public
        public async Task subscribeAll(string link)
        {
            await Task.Run(async () =>
            {

                foreach (var drop in DropList)
                {
                    try
                    {
                        await drop.subscribe(link);
                    }
                    catch (Exception ex)
                    {
                        logger.err("ERR", ex.Message);
                    }
                }

            });
        }

        Random subsRand = new Random();

        public async void OnEvent(BaseEventMessage message)
        {
            switch (message)
            {
                case AddDropEventMessage addDropMessage:

                    var phone = addDropMessage.phone_number;

                    phone = phone.Replace(" ", "");
                    if (!phone.Contains("+"))
                        phone = "+" + phone;

                    using (var db = new DataBaseContext())
                    {
                        var found = db.Drops.Any(d => d.phone_number.Equals(phone));
                        if (!found)
                        {
                            var dropModel = new DropModel()
                            {
                                phone_number = phone,
                                _2fa_password = addDropMessage._2fa_password,
                                group_id = addDropMessage.group_id
                            };

                            db.Add(dropModel);
                            db.SaveChanges();
                            try
                            {
                                await addDrop(dropModel);
                                await updateViewedDrops();
                            }
                            catch (Exception ex)
                            {
                                throw;
                            }
                        }
                    }
                    SubContent = null;
                    break;

                case DropStatusChangedEventMessage statusMessage:

                    switch (statusMessage.status)
                    {
                        case DropStatus.removed:
                        case DropStatus.banned:

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                var found = DropList.FirstOrDefault(d => d.id == statusMessage.id); 
                                if (found != null)
                                    DropList.Remove(found);
                            });

                            cleaner.Enqueue(statusMessage.group_id, statusMessage.id, statusMessage.phone_number);
                            break;
                    }

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        OnlineCount = (statusMessage.status == DropStatus.active) ? ++OnlineCount : --OnlineCount;
                    });
                    break;

                case ChannelSubscribeRequestEventMessage subscribeMessage:
                    try
                    {
                        //if (group_id == subscribeMessage.group_id)
                        //    await subscribe(subscribeMessage.link);

                        var groupedDrops = DropList.Where(d => d.group_id == subscribeMessage.group_id).ToList();
                        foreach (var drop in groupedDrops) {

                            try
                            {
                                await Task.Run(async () => { 
                                    var res = await drop.subscribe(subscribeMessage.link);
                                    if (res)
                                        //Thread.Sleep(subsRand.Next(3, 5) * 1000);
                                        await Task.Delay(subsRand.Next(3, 5) * 1000);
                                });

                            } catch (Exception ex)
                            {
                                logger.err($"GroupSubscribe {drop.phone_number}", $"OnEvent subscribeMessage: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.err($"GroupSubscribe {subscribeMessage.group_id}", $"OnEvent subscribeMessage: {ex.Message}");
                    }
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region events
        public event Action<string, long, string> ChannelAddedEvent;
        #endregion
    }
}
