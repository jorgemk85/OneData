﻿using OneData.Models;

namespace OneData.Interfaces
{
    public interface IManageable
    {
        Configuration Configuration { get; }
        bool IsFullyValidated { get; set; }
    }
}
