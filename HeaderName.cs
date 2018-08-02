﻿using System;

namespace DataManagement
{
    public class HeaderName : Attribute
    {
        public string Name { get; set; }
        public bool Important { get; set; }

        public HeaderName(string name, bool important = true)
        {
            Name = name;
            Important = important;
        }
    }
}