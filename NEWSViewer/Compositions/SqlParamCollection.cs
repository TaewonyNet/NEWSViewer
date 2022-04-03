using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEWSViewer
{
    public class SqlParamCollection : IEnumerable<SQLiteParameter>
    {
        #region GLOVAL VARIABLES
        private List<SQLiteParameter> _List;

        public int _Count { get { return this._List.Count; } }

        public SQLiteParameter this[int index] { get { return this._List[index]; } }

        public SQLiteParameter this[string name] { get { return this._List.Find(x => x.ParameterName == name); } }
        #endregion

        /// <summary>
        /// SqlParamCollection
        /// </summary>
        public SqlParamCollection()
        {
            _List = new List<SQLiteParameter>();
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="n"></param>
        public void Add(string n)
        {
            SQLiteParameter param = new SQLiteParameter();
            param.ParameterName = n;

            this._List.Add(param);
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="n"></param>
        /// <param name="v"></param>
        public void Add(string n, object v)
        {
            SQLiteParameter param = new SQLiteParameter();

            param.ParameterName = n;
            param.Value = v;

            this._List.Add(param);
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="n"></param>
        /// <param name="v"></param>
        /// <param name="i"></param>
        public void Add(string n, object v, int i)
        {
            SQLiteParameter param = new SQLiteParameter();

            param.ParameterName = n;
            param.Value = v;

            this._List.Insert(0, param);
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="n"></param>
        /// <param name="t"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        public void Add(string n, DbType t, int s, object v)
        {
            SQLiteParameter param = new SQLiteParameter();

            param.ParameterName = n;
            param.DbType = t;
            param.Size = s;
            param.Value = v;

            this._List.Add(param);
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="n"></param>
        /// <param name="t"></param>
        /// <param name="s"></param>
        /// <param name="d"></param>
        public void Add(string n, DbType t, int s, ParameterDirection d)
        {
            SQLiteParameter param = new SQLiteParameter();

            param.ParameterName = n;
            param.DbType = t;
            param.Size = s;
            param.Direction = d;

            this._List.Add(param);
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="n"></param>
        /// <param name="t"></param>
        /// <param name="s"></param>
        /// <param name="d"></param>
        /// <param name="v"></param>
        public void Add(string n, DbType t, int s, ParameterDirection d, object v)
        {
            SQLiteParameter param = new SQLiteParameter();

            param.ParameterName = n;
            param.DbType = t;
            param.Size = s;
            param.Direction = d;
            param.Value = v;

            this._List.Add(param);
        }

        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            this._List.Clear();
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns>IEnumerator</returns>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<SQLiteParameter>)this).GetEnumerator();
        }

        /// <summary>
        /// IEnumerable<SQLiteParameter>.GetEnumerator
        /// </summary>
        /// <returns>IEnumerator<SQLiteParameter></returns>
        IEnumerator<SQLiteParameter> IEnumerable<SQLiteParameter>.GetEnumerator()
        {
            for (int index = 0; index < this._Count; index++)
            {
                yield return _List[index];
            }
        }
    }
}