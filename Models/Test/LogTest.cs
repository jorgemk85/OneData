using DataManagement.Attributes;
using DataManagement.DAO;
using DataManagement.Interfaces;
using System;
using System.Collections.Generic;

namespace DataManagement.Models.Test
{
    [DataTable("LogTestGuids"), CacheEnabled(60)]
    public class LogTestGuid : Cope<LogTestGuid>, IManageable
    {
        [PrimaryProperty]
        public Guid? Id { get; set; }
        [DateCreatedProperty]
        public DateTime? DateCreated { get; set; }
        [DateModifiedProperty]
        public DateTime? DateModified { get; set; }

        public string Ip { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }

        [ForeignCollection(typeof(UserTest))]
        public ICollection<UserTest> UserTests { get; set; }
    }

    [DataTable("LogTestInts"), CacheEnabled(60)]
    public class LogTestInt : Cope<LogTestInt>, IManageable
    {
        [PrimaryProperty]
        public int? Id { get; set; }
        [DateCreatedProperty]
        public DateTime? DateCreated { get; set; }
        [DateModifiedProperty]
        public DateTime? DateModified { get; set; }
        public string Ip { get; set; }
        public string Transaccion { get; set; }
        public string TablaAfectada { get; set; }
        public string Parametros { get; set; }
    }
}