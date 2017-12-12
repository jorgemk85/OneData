using DataAccess.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public interface IConnectable
    {
        Result Insert<T>(T obj) where T : new();

        Result Update<T>(T obj) where T : new();

        Result Delete<T>(T obj) where T : new();

        T Select<T>(params Parameter[] parameters) where T : new();

        List<T> SelectList<T>(params Parameter[] parameters) where T : new();

        Dictionary<Guid, T> SelectDictionary<T>(params Parameter[] parameters) where T : new();

        Result SelectOther(string tableName, string storedProcedure, params Parameter[] parameters) ;

        List<T> SelectAllList<T>() where T : new();

        Dictionary<Guid, T> SelectAllDictionary<T>() where T : new();
    }
}
