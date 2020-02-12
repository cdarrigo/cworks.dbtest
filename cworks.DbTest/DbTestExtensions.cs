using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace cworks.DbTest
{
    /// <summary>
    /// Extension methods
    /// </summary>
    /// // ReSharper disable  UnusedMember.Global
    public static class DbTestExtensions
    {
        
        /// <summary>
        /// Configures the SqlRequest to commit data context changes after running the sql commands
        /// </summary>
        public static SqlRequest CommitDataContextBeforeRunningSql(this SqlRequest request)
        {
            request.DataContextCommitPoint = DataContextCommitPoint.BeforeRunningSql;
            return request;
        }

        /// <summary>
        /// Configures the SqlRequest to commit data context changes after running the sql commands
        /// </summary>

        public static SqlRequest CommitDataContextAfterRunningSql(this SqlRequest request)
        {
            request.DataContextCommitPoint = DataContextCommitPoint.AfterRunningSql;
            return request;
        }

        /// <summary>
        /// Configures the SqlRequest to not commit any data context changes
        /// </summary>
        public static SqlRequest DontCommitDataContext(this SqlRequest request)
        {
            request.DataContextCommitPoint = DataContextCommitPoint.None;
            return request;
        }

        /// <summary>
        /// returns the first column of the first row in the first datatable
        /// as T
        /// </summary>
        public static T ToScalar<T>(this DataTable[] data)
        {
            // ReSharper disable once RedundantTypeSpecificationInDefaultExpression
            return data == null || !data.Any() ? default(T) : data[0].ToScalar<T>();
        }

        /// <summary>
        /// returns the first column of the first row of the datatable
        /// as T
        /// Same as ToScalar, for backward compatibility
        /// </summary>
        public static T ToScalar<T>(this DataTable data)
        {
            var value = data.Rows[0][0];
            if (value == DBNull.Value)
                return default(T);
            
            return (T) data.Rows[0][0];
        }


        
    
        #region SqlRequest Generation Methods
        public static SqlRequest ReturnAllRows<TEntity>(this IDbTest test, params SqlParameter[] parameters)
        {
            
            return SqlRequest.ReturnAllRows(test.GetQualifiedTableOrViewName<TEntity>(),parameters);
        }

        public static SqlRequest InvokeScalarFunction(this IDbTest test, string functionName, string schema = null, params SqlParameter[] parameters)
        {
            var sql = new StringBuilder( $"Select {(!string.IsNullOrEmpty(schema) ? $"{schema}." : "")}{functionName}(");
            var firstParam = true;
            foreach (var p in parameters)
            {
                if (firstParam)
                    firstParam = false;
                else
                    sql.Append(", ");

                sql.Append(p.ParameterName);
            }

            sql.Append(") as RetVal");

            return SqlRequest.RunSqlText(sql.ToString(), parameters);
        }
        public static SqlRequest InvokeTableFunction(this IDbTest test, string functionName, string schema = null, params SqlParameter[] parameters)
        {
            var sql = new StringBuilder( $"Select * From {(!string.IsNullOrEmpty(schema) ? $"{schema}." : "")}{functionName}(");
            var firstParam = true;
            foreach (var p in parameters)
            {
                if (firstParam)
                    firstParam = false;
                else
                    sql.Append(", ");

                sql.Append(p.ParameterName);
            }

            sql.Append(")");

            return SqlRequest.RunSqlText(sql.ToString(), parameters);
        }
        public static SqlRequest ReturnAllRows(this IDbTest test, string tableOrViewName = null, string schema=null, params SqlParameter[] parameters)
        {
            return SqlRequest.ReturnAllRows(test.GetQualifiedTableOrViewName(tableOrViewName, schema),parameters);
        }

        public static SqlRequest ReturnNoRows<TEntity>(this IDbTest test, params SqlParameter[] parameters)
        {
            return SqlRequest.ReturnNoRows(test.GetQualifiedTableOrViewName<TEntity>(), parameters);
        }
        public static SqlRequest ReturnNoRows(this IDbTest test, string tableOrViewName, string schema = null, params SqlParameter[] parameters)
        {
            return SqlRequest.ReturnNoRows(test.GetQualifiedTableOrViewName(tableOrViewName,schema),parameters);
        }
        
    
        public static SqlRequest CommitDataContextNow(this IDbTest test)
        {
            return SqlRequest.CommitDataContextNow();
        }

        public static SqlRequest ReturnRowCount(this IDbTest test, string tableOrViewName, string schema = null, params SqlParameter[] parameters)
        {
            return SqlRequest.ReturnRowCount(test.GetQualifiedTableOrViewName(tableOrViewName,schema), parameters);
        }
        
        public static SqlRequest ReturnRowCount<TEntity>(this IDbTest test,  params SqlParameter[] parameters)
        {
            return SqlRequest.ReturnRowCount(test.GetQualifiedTableOrViewName<TEntity>(), parameters);
        }

        public static SqlRequest DoNothing(this IDbTest test)
        {
            return null;
        }

        public static SqlRequest RunSqlText( this IDbTest test, string sql = null, params SqlParameter[] parameters)
        {
            return SqlRequest.RunSqlText(sql,parameters);
        }

        public static SqlRequest ExecuteStoredProcedure(this IDbTest test, string sprocName = null, string schema=null, params SqlParameter[] parameters)
        {
            return SqlRequest.RunStoredProcedure(sprocName, schema, parameters);
        }


        public static SqlRequest RunTableDirect(this IDbTest test, string tableName = null, string schema = null, params SqlParameter[] parameters)
        {
            return SqlRequest.RunTableDirect(test.GetQualifiedTableOrViewName(tableName,schema),parameters);
        }
        public static SqlRequest RunTableDirect<TEntity>(this IDbTest test, params SqlParameter[] parameters)
        {
            return SqlRequest.RunTableDirect(test.GetQualifiedTableOrViewName<TEntity>(),parameters);
        }
       #endregion



     
       public static string GetQualifiedTableOrViewName(this IDbTest test, string tableOrViewName = null, string schema = null, bool isRequired = true)
       {
           return test.GetQualifiedTableOrViewName<object>(tableOrViewName, schema, isRequired);
       }

       public static string GetQualifiedTableOrViewName<TEntity>(this IDbTest test, string tableOrViewName = null, string schema = null, bool isRequired = true)
       {
           if (string.IsNullOrEmpty(tableOrViewName))
           {
               var info = test.GetTableOrViewInfoFromTest();
               tableOrViewName = info.Name;
               schema = info.Schema;
               if (String.IsNullOrEmpty(info.Name))
               {
                   info = GetTableOrViewInfoFromType(typeof(TEntity));
                   tableOrViewName = info.Name;
                   schema = info.Schema;
               }
           }


           if (string.IsNullOrEmpty(tableOrViewName))
           {
               if (isRequired)
                   throw new DbTestException("Failed to run test. Missing required tableOrViewName value.");
               return null;
           }

           tableOrViewName = tableOrViewName.Trim();
           schema = (schema ?? "dbo").Trim();

           if (!tableOrViewName.StartsWith("["))
               tableOrViewName = $"[{tableOrViewName}]";

           if (!schema.StartsWith("["))
               schema = $"[{schema}]";

           return $"{schema}.{tableOrViewName}";
       }

       
       private static (string Name, string Schema) GetTableOrViewInfoFromTest(this IDbTest test)
       {
           return GetTableOrViewInfoFromType(test.GetType());
       }
        
       private static (string Name, string Schema) GetTableOrViewInfoFromType( Type type)
       {
           var tableAttribute = type.GetCustomAttribute<TableAttribute>();
           if (tableAttribute != null)
               return (tableAttribute.Name, tableAttribute.Schema);
           return (null, null);
       }
        
    }
}