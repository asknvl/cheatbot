﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class GroupStartedEventMessage : BaseEventMessage
    {
        public int group_id { get; set; }
        public GroupStartedEventMessage(int group_id)
        {
            this.group_id = group_id;
        }   
    }
}