using asknvl.logger;
using Avalonia.Controls;
using Avalonia.Threading;
using cheatbot.Database;
using cheatbot.Database.models;
using cheatbot.Models.drop;
using cheatbot.ViewModels.events;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reactive;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels
{
    public class dropListVM : ViewModelBase, IEventSubscriber<BaseEventMessage>
    {
        #region vars
        ILogger logger;
        IEventAggregator eventAggregator;
        #endregion

        #region properties
        List<GroupModel> groups;
        public List<GroupModel> Groups
        {
            get => groups;
            set
            {
                this.RaiseAndSetIfChanged(ref groups, value);
                if (groups != null && groups.Count > 0)
                    SelectedGroup = groups[0];                    
            }
        }

        GroupModel selectedGroup;
        public GroupModel SelectedGroup
        {
            get => selectedGroup;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedGroup, value);
                updateList(logger, selectedGroup);
            }
        }
        public ObservableCollection<dropVM> DropList { get; } = new();

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

        string _old2FA;
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
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        public ReactiveCommand<Unit, Unit> deleteCmd { get; }
        public ReactiveCommand<Unit, Unit> set2FACmd { get; }
        public ReactiveCommand<Unit, Unit> startAllCmd { get; }
        #endregion
        public dropListVM(ILogger logger)
        {
            this.logger = logger;
            //dropVM d0 = new dropVM();
            //d0.phone_number = "+78889993322";

            //DropList.Add(d0);            
            updateGroups();
            //updateList(logger, SelectedGroup);

            #region commands
            addCmd = ReactiveCommand.Create(() =>
            {

                addDropVM addVM = new addDropVM(SelectedGroup.id);
                SubContent = addVM;

                //addVM.AddDropRequestEvent += (phone, _2fa_password) =>
                //{
                //    phone = phone.Replace(" ", "");
                //    if (!phone.Contains("+"))
                //        phone = "+" + phone;

                //    using (var db = new DataBaseContext())
                //    {
                //        var found = db.Drops.Any(d => d.phone_number.Equals(phone));
                //        if (!found)
                //        {
                //            var dropModel = new DropModel()
                //            {
                //                phone_number = phone,
                //                _2fa_password = _2fa_password
                //            };

                //            db.Add(dropModel);
                //            db.SaveChanges();

                //            //var dvm = new dropVM(phone, logger);
                //            //dvm.ChannelAddedEvent += (link, id, name) => {
                //            //    ChannelAddedEvent?.Invoke(link, id, name);
                //            //};

                //            //DropList.Add(dvm);

                //            addDrop(dropModel);
                //        }
                //    }
                //    SubContent = null;
                //};


            });

            deleteCmd = ReactiveCommand.Create(() =>
            {

                using (var db = new DataBaseContext())
                {
                    var found_db = db.Drops.FirstOrDefault(d => d.phone_number.Equals(SelectedDrop.phone_number));
                    db.Remove(found_db);
                    db.SaveChanges();
                }

                var found_list = DropList.FirstOrDefault(d => d.phone_number.Equals(SelectedDrop.phone_number));
                DropList.Remove(found_list);

            });

            set2FACmd = ReactiveCommand.Create(() => {
                if (SelectedDrop != null)
                    EventAggregator.getInstance().Publish((BaseEventMessage)new Change2FAPasswordOneEventMessage(SelectedDrop.phone_number, Old2FA, New2FA));
                else
                    EventAggregator.getInstance().Publish((BaseEventMessage)new Change2FAPasswordAllEventMessage(Old2FA, New2FA));
            });

            startAllCmd = ReactiveCommand.CreateFromTask(async () => { 
            
                foreach (var drop in DropList)
                {
                    drop.startCmd.Execute();
                }
            
            });

            #endregion
        }

        #region private
        async Task updateList(ILogger logger, GroupModel model)
        {
            await Task.Run(() =>
            {
                DropList.Clear();

                List<DropModel> drops = new();

                using (var db = new DataBaseContext())
                {
                    if (model.id == 1)
                        drops = db.Drops.ToList();
                    else
                        drops = db.Drops.Where(d => d.group_id == model.id).ToList();
                }

                foreach (var drop in drops)
                {
                    //Dispatcher.UIThread.InvokeAsync(() =>
                    //{
                    //    DropList.Add(new dropVM(drop.phone_number, logger));
                    //});
                    addDrop(drop);
                }
                
            });
        }

        async Task updateGroups()
        {            
            using (var db = new DataBaseContext()) { 
                Groups = db.Groups.ToList();
            }
        }
        #endregion

        #region helpers
        void addDrop(DropModel model)
        {
            var dvm = new dropVM(model.phone_number, model._2fa_password, logger);
            dvm.ChannelAddedEvent += (link, id, name) =>
            {
                ChannelAddedEvent?.Invoke(link, id, name);
            };
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                DropList.Add(dvm);
            });
        }        
    
    #endregion

        #region public
    public async Task subscribeAll(string link)
        {
            await Task.Run(async () => { 
            
                foreach (var drop in DropList)
                {
                    try
                    {
                        await drop.subscribe(link);
                    } catch (Exception ex)
                    {
                        logger.err("ERR", ex.Message);
                    }
                }

            });
        }

        public void OnEvent(BaseEventMessage message)
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

                            addDrop(dropModel);
                        }
                    }
                    SubContent = null;
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
