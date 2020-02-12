using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace cworks.DbTest
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
    public class SqlRequest
    {
        #region region static helper methods for producing configured SqlRequest instances 
        public static SqlRequest CommitDataContextNow()
        {
            return new SqlRequest{DataContextCommitPoint = DataContextCommitPoint.BeforeRunningSql};
        }

        public static SqlRequest DoNothing()
        {
            return null;
        }

        public static SqlRequest RunSqlText(string sql = null, params SqlParameter[] parameters)
        {
            return new SqlRequest(sql,parameters)
            {
                DataContextCommitPoint = DataContextCommitPoint.None,
                CommandType =  CommandType.Text
            };
        }

        public static SqlRequest RunStoredProcedure(string sprocName = null, string schema = null, params SqlParameter[] parameters)
        {
            var qualifiedName = sprocName;
            if (!string.IsNullOrEmpty(sprocName) && !string.IsNullOrEmpty(schema))
            {
                qualifiedName = $"[{schema.Trim()}].[{sprocName.Trim()}]";
            }
            return new SqlRequest(qualifiedName,parameters)
            {
                DataContextCommitPoint = DataContextCommitPoint.None,
                CommandType =  CommandType.StoredProcedure
            };
        }

        public static SqlRequest ReturnAllRows(string tableOrViewName, params SqlParameter[] parameters)
        {
            return RunTableDirect(tableOrViewName, parameters);
        }

        public static SqlRequest ReturnNoRows(string tableOrViewName, params SqlParameter[] parameters)
        {
            return RunSqlText($"Select * from {tableOrViewName} where 1=0");
            
        }
        public static SqlRequest RunTableDirect(string tableName = null, params SqlParameter[] parameters)
        {
            return new SqlRequest(tableName,parameters)
            {
                DataContextCommitPoint = DataContextCommitPoint.None,
                CommandType =  CommandType.TableDirect
            };
        }

        public static SqlRequest ReturnRowCount(string tableOrViewName, params SqlParameter[] parameters)
        {
            return RunSqlText($"Select Count(*) from {tableOrViewName}", parameters);
        }
        #endregion
    
        
        /// <summary>
        /// ctor
        /// </summary>
        public SqlRequest(string sqlToExecute, params SqlParameter[] parameters)
        {
            this.SqlToExecute = sqlToExecute;
            foreach (var p in parameters)
                this.Parameters[p.ParameterName] = p;
        }

        /// <summary>
        /// ctor
        /// </summary>
        private SqlRequest()
        {
            
        }
        
        public string SqlToExecute { get; set; }
        public Dictionary<string,SqlParameter> Parameters { get; set; } = new Dictionary<string, SqlParameter>();
        
        public bool IsEmpty => string.IsNullOrEmpty(this.SqlToExecute);

        public DataContextCommitPoint DataContextCommitPoint { get; set; } = DataContextCommitPoint.None;
        public CommandType CommandType { get; set; } = CommandType.Text;

     
    }
}