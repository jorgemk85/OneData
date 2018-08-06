using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataManagement.Models
{
    public class Result
    {
        public DataTable Data { get; set; }

        public bool IsFromCache { get; set; }

        public Result(DataTable data = null, bool isFromCache = false)
        {
            Data = data == null ? new DataTable() : data;
            IsFromCache = isFromCache;
        }
    }
}
