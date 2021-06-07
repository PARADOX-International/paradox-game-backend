﻿using PARADOX_RP.Core.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace PARADOX_RP.Core.Models
{
    public class InstanceBase<T> : ISingleton
    {
        public static T Instance { get; set; }
    }

    public class Singleton<T> : InstanceBase<T> where T : Singleton<T>
    {
        public Singleton()
        {
            Instance = (T)this;
        }
    }
}
