using asknvl.logger;
using cheatbot.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Models.cleaner
{
    public class DropCleaner
    {
        #region vars
        string root;
        Queue<CleanInstance> cleanQueue = new Queue<CleanInstance>();
        System.Timers.Timer cleanTimer;
        ILogger logger;
        #endregion

        public DropCleaner(ILogger logger)
        {
            this.logger = logger;   

            cleanTimer = new System.Timers.Timer();
            cleanTimer.Interval = 5000; 
            cleanTimer.AutoReset = true;
            cleanTimer.Elapsed += CleanTimer_Elapsed;
            cleanTimer.Start();

            using (var db = new DataBaseContext())
            {
                root = db.AppSettings.First().RootPath;
            }
        }

        private async void CleanTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (cleanQueue.Count > 0)
                {
                    var found = cleanQueue.Dequeue();
                    await found.Clean();

                    logger.inf("Clener:", $"{found.phone_number} removed");

                }
            } catch (Exception ex)
            {
                logger.err("Clean:", ex.Message);
            }
        }
        #region public
        public void Enqueue(int group_id, int id, string phone_number)
        {
            var instance = new CleanInstance(group_id, id, phone_number, root);
            cleanQueue.Enqueue(instance);
        }
        #endregion
    }

    public class CleanInstance
    {
        #region vars        
        int id;        
        string folder_path;
        #endregion

        #region properties
        public string phone_number { get; }
        #endregion

        public CleanInstance(int group_id, int id, string phone_number, string root)
        {            
            this.id = id;
            this.phone_number = phone_number;

            folder_path = Path.Combine(root, $"{group_id}", $"{phone_number.Replace("+", "")}");
        }

        public async Task Clean()
        {
            await Task.Run(() => { 
                using (var db = new DataBaseContext())
                {
                    var found_db = db.Drops.FirstOrDefault(d => d.phone_number.Equals(phone_number));
                    if (found_db != null)
                        db.Remove(found_db);

                    var found_subs = db.DropSubscribes.Where(ds => ds.drop_id == id);
                    db.RemoveRange(found_subs);

                    db.SaveChanges();
                }

                if (Directory.Exists(folder_path))
                    Directory.Delete(folder_path, true);

            });
        }
    }
}
