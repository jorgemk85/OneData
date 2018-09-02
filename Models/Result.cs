using DataManagement.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace DataManagement.Models
{
    //public class Result
    //{
    //    public DataTable Data { get; set; }
    //    public ICollection Collection { get; set; }
    //    public bool IsFromCache { get; set; }
    //    public bool IsSuccessful { get; set; }

    //    public Result(DataTable data, bool isFromCache, bool isSuccessful)
    //    {
    //        Data = data ?? new DataTable();
    //        IsFromCache = isFromCache;
    //        IsSuccessful = isSuccessful;
    //    }

    //    public Result(DataTable data, bool isFromCache, bool isSuccessful, ICollection collection)
    //    {
    //        Data = data ?? new DataTable();
    //        IsFromCache = isFromCache;
    //        IsSuccessful = isSuccessful;
    //        Collection = collection;
    //    }
    //}

    public class Result
    {
        public Hashtable Hash { get; set; }
        public bool IsFromCache { get; set; }
        public bool IsSuccessful { get; set; }

        public Result(Hashtable collection, bool isFromCache, bool isSuccessful)
        {
            Hash = collection;
            IsFromCache = isFromCache;
            IsSuccessful = isSuccessful;
        }
    }
}
