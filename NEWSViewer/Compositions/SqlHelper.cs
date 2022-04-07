using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer
{
    public class SqlHelper : IDisposable
    {
        #region GLOVAL VARIABLES
        public string ConnectionString { get; set; }
        public int _ComTimeOut = 60;
        private SQLiteConnection _Con;
        private SQLiteCommand _Com;
        private SQLiteDataAdapter _Adp;
        private DataSet _Ds;
        #endregion

        /// <summary>
        /// SqlHelper
        /// </summary>
        /// <param name="iTime">Optional int Command TimeOut Value</param>
        public SqlHelper(int iTime = 60)
        {
            this._ComTimeOut = iTime;
        }

        /// <summary>
        /// SqlHelper
        /// </summary>
        /// <param name="cs">DB ConnectionString</param>
        /// <param name="iTime">Optional Command TimeOut Value</param>
        public SqlHelper(string cs, int iTime = 60)
        {
            this.ConnectionString = cs;
            this._ComTimeOut = iTime;
        }

        /// <summary>
        /// Create DB
        /// </summary>
        /// <param name="path"></param>
        public void CreateDB(string path)
        {
            SQLiteConnection.CreateFile(path);
        }

        /// <summary>
        /// ExecuteDataSet
        /// </summary>
        /// <param name="q"></param>
        /// <param name="p"></param>
        /// <param name="t"></param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataSet(string q, SqlParamCollection p = null, CommandType t = CommandType.Text)
        {
            using (_Con = new SQLiteConnection(ConnectionString))
            {
                _Con = new SQLiteConnection(ConnectionString);

                _Com = new SQLiteCommand(q, _Con);
                _Com.CommandType = t;

                _Com.CommandTimeout = _ComTimeOut;

                _Adp = new SQLiteDataAdapter(_Com);
                _Ds = new DataSet();

                StringBuilder sb = new StringBuilder();
                if (p != null && p._Count > 0)
                {
                    var i = p.GetEnumerator();
                    while (i.MoveNext())
                    {
                        SQLiteParameter parameter = (SQLiteParameter)i.Current;
                        _Com.Parameters.Add(parameter);
#if DEBUG
                        sb.Append(string.Format("{0}:{1} ", parameter.ParameterName, parameter.Value));
#endif
                    }
                }
#if DEBUG
                Log.Info("ExecuteDataSet {0} {1}", q.Replace("\n", ""), sb.ToString());
#endif

                _Con.Open();
                _Adp.Fill(_Ds);
                _Con.Close();
            }

            return _Ds;
        }
        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="q"></param>
        /// <param name="p"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string q, SqlParamCollection p = null, CommandType t = CommandType.Text)
        {
            int r = 0;

            using (_Con = new SQLiteConnection(ConnectionString))
            {
                _Con = new SQLiteConnection(ConnectionString);

                _Com = new SQLiteCommand(q, _Con);
                _Com.CommandType = t;

                _Com.CommandTimeout = 600;

                StringBuilder sb = new StringBuilder();
                if (p != null && p._Count > 0)
                {
                    var i = p.GetEnumerator();
                    while (i.MoveNext())
                    {
                        SQLiteParameter parameter = (SQLiteParameter)i.Current;
                        _Com.Parameters.Add(parameter);
#if DEBUG
                        sb.Append(string.Format("{0}:{1} ", parameter.ParameterName, parameter.Value));
#endif
                    }
                }
#if DEBUG
                Log.Info("ExecuteNonQuery {0} {1}", q.Replace("\n", ""), sb.ToString());
#endif

                _Con.Open();
                r = _Com.ExecuteNonQuery();
                _Con.Close();
            }

            return r;
        }

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="q"></param>
        /// <param name="p"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string q, List<SqlParamCollection> lp, CommandType t = CommandType.Text)
        {
            int r = 0;

            using (_Con = new SQLiteConnection(ConnectionString))
            {
                _Con = new SQLiteConnection(ConnectionString);

                _Com = new SQLiteCommand(q, _Con);
                _Com.CommandType = t;

                _Com.CommandTimeout = _ComTimeOut;

                _Con.Open();
                if (lp != null && lp.Count > 0)
                {
                    using (var transaction = _Con.BeginTransaction())
                    {
                        foreach (var p in lp)
                        {
                            StringBuilder sb = new StringBuilder();
                            if (p != null && p._Count > 0)
                            {
                                var i = p.GetEnumerator();
                                while (i.MoveNext())
                                {
                                    SQLiteParameter parameter = (SQLiteParameter)i.Current;
                                    _Com.Parameters.Add(parameter);
#if DEBUG
                                    sb.Append(string.Format("{0}:{1} ", parameter.ParameterName, parameter.Value));
#endif
                                }
                            }
#if DEBUG
                            Log.Info("ExecuteNonQuery {0} {1}", q.Replace("\n", ""), sb.ToString());
#endif
                            r = _Com.ExecuteNonQuery();
                            _Com.Parameters.Clear();
                        }
                        transaction.Commit();
                    }
                }
                _Con.Close();
            }

            return r;
        }

        public object ExecuteScalar(string q, SqlParamCollection p = null, CommandType t = CommandType.Text)
        {
            object r;

            using (_Con = new SQLiteConnection(ConnectionString))
            {
                _Con = new SQLiteConnection(ConnectionString);

                _Com = new SQLiteCommand(q, _Con);
                _Com.CommandType = t;

                _Com.CommandTimeout = 600;

                StringBuilder sb = new StringBuilder();
                if (p != null && p._Count > 0)
                {
                    var i = p.GetEnumerator();
                    while (i.MoveNext())
                    {
                        SQLiteParameter parameter = (SQLiteParameter)i.Current;
                        _Com.Parameters.Add(parameter);
#if DEBUG
                        sb.Append(string.Format("{0}:{1} ", parameter.ParameterName, parameter.Value));
#endif
                    }
                }
#if DEBUG
                Log.Info("ExecuteScalar {0} {1}", q.Replace("\n", ""), sb.ToString());
#endif

                _Con.Open();
                r = _Com.ExecuteScalar();
                _Con.Close();
            }

            return r;
        }


        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (_Con != null) _Con.Dispose();
            if (_Com != null) _Com.Dispose();
            if (_Adp != null) _Adp.Dispose();
            if (_Ds != null) _Ds.Dispose();
        }

        public T GetSingleData<T>(DataSet dataSet)// where T : APIModelbase
        {
            var ts = GetData<T>(dataSet);
            if (ts.Count == 0)
            {
                return default(T);
            }
            else
            {
                return ts[0];
            }
        }

        public List<T> GetData<T>(DataSet dataSet)// where T : APIModelbase
        {
            var list = new List<T>();
            if (dataSet.Tables.Count > 0)
            {
                var type = typeof(T);
                DataTable dt = dataSet.Tables[0];
                foreach (DataRow r in dt.Rows)
                {
                    T obj = (T)Activator.CreateInstance(typeof(T), null);
                    //foreach (var info in obj.GetProperties())
                    var pps = type.GetProperties();
                    if (pps.Length > 0)
                    {
                        foreach (var info in pps)
                        {
                            string name = info.Name;
                            if ((info.CanRead == true)
                                && (info.CanWrite == true)
                                && (dt.Columns.Contains(name) == true))
                            {
                                string value = r[name].ToString();
                                if (info.PropertyType == typeof(bool)
                                    || info.PropertyType == typeof(Nullable<bool>))
                                {
                                    bool val = false;
                                    if (bool.TryParse(value, out val) == true)
                                    {
                                        info.SetValue(obj, val);
                                    }
                                    else if (value == "1" || value == "0")
                                    {
                                        info.SetValue(obj, value == "1");
                                    }
                                    else
                                    {
                                        info.SetValue(obj, null);
                                    }
                                }
                                else if (info.PropertyType == typeof(int)
                                    || info.PropertyType == typeof(Nullable<int>))
                                {
                                    int val = 0;
                                    if (int.TryParse(value, out val) == true)
                                    {
                                        info.SetValue(obj, val);
                                    }
                                    else
                                    {
                                        info.SetValue(obj, null);
                                    }
                                }
                                else if (info.PropertyType == typeof(long)
                                    || info.PropertyType == typeof(Nullable<long>))
                                {
                                    long val = 0;
                                    if (long.TryParse(value, out val) == true)
                                    {
                                        info.SetValue(obj, val);
                                    }
                                    else
                                    {
                                        info.SetValue(obj, null);
                                    }
                                }
                                else if (info.PropertyType == typeof(double)
                                    || info.PropertyType == typeof(Nullable<double>))
                                {
                                    double val = 0;
                                    if (double.TryParse(value, out val) == true)
                                    {
                                        info.SetValue(obj, val);
                                    }
                                    else
                                    {
                                        info.SetValue(obj, null);
                                    }
                                }
                                else if (info.PropertyType == typeof(DateTime)
                                    || info.PropertyType == typeof(Nullable<DateTime>))
                                {
                                    if (r[name] is DateTime || r[name] is Nullable<DateTime>)
                                    {
                                        info.SetValue(obj, (DateTime)r[name]);
                                    }
                                    else if (r[name] is string)
                                    {
                                        info.SetValue(obj, DateTime.Parse(r[name].ToString()));
                                    }
                                    else
                                    {
                                        info.SetValue(obj, null);
                                    }
                                }
                                else if (info.PropertyType == typeof(TimeSpan)
                                    || info.PropertyType == typeof(Nullable<TimeSpan>))
                                {
                                    TimeSpan val = new TimeSpan();
                                    DateTime val2 = new DateTime();
                                    if (TimeSpan.TryParse(value, out val) == true)
                                    {
                                        info.SetValue(obj, val);
                                    }
                                    if (DateTime.TryParse(value, out val2) == true)
                                    {
                                        info.SetValue(obj, val2.Subtract(DateTime.Today));
                                    }
                                    else
                                    {
                                        info.SetValue(obj, null);
                                    }
                                }
                                else
                                {
                                    info.SetValue(obj, value);
                                }
                            }
                        }
                    }
                    else
                    {
                        obj = (T)Convert.ChangeType(r[0], typeof(T));
                    }
                    list.Add(obj);
                }
            }
            return list;
        }
    }
}
