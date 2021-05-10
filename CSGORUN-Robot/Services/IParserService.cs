﻿using System;

namespace CSGORUN_Robot.Services
{
    public interface IParserService
    {
        public event EventHandler<object> MessageReceived;
        public bool IsActive { get; }
        public void Start();
        public void Stop();
    }
}
