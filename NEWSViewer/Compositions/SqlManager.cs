using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer
{
    public class SqlManager : SingletonBase<SqlManager>
    {
        public string ConnectionString { get; set; }

        public SqlManager()
        {
            string fileName = "Database.db";
            string projectName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            string database = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), projectName, fileName);

            string dir = System.IO.Path.GetDirectoryName(database);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }
            string connectionstring = "Data Source=" + database;
            ConnectionString = connectionstring;
            if (File.Exists(database) == false)
            {
                using (SqlHelper sql = new SqlHelper(ConnectionString))
                {
                    sql.CreateDB(database);
                    sql.ExecuteNonQuery(@"
CREATE TABLE T_OPTION (
    Key TEXT, 
    Value TEXT,
    PRIMARY KEY(Key)
);
CREATE TABLE T_CATEGORY (
    CategorySeq INTEGER,
    Category TEXT, 
    IsSearchTitle INTEGER,
    Type TEXT,
    SearchText TEXT, 
    NoSearchText TEXT, 
    UpCategorySeq INTEGER, 
    Number INTEGER,
    RegDate TEXT, 
    ModDate TEXT,
    PRIMARY KEY(CategorySeq  AUTOINCREMENT)
);
CREATE TABLE T_ARTICLE (
    Type TEXT, 
    CategorySeq INTEGER,
    Press TEXT, 
    PressLink TEXT, 
    InfoTime TEXT, 
    Title TEXT, 
    Link TEXT, 
    Description TEXT, 
    ImageUrl TEXT,
    IsRead INTEGER,
    IsDelete INTEGER, 
    RegDate TEXT,
    ReadDate TEXT,
    ModDate TEXT,
    PRIMARY KEY(Link, CategorySeq)
);
");
                    //CREATE TABLE T_READ(
                    //    Link TEXT,
                    //    IsRead INTEGER,
                    //    IsDelete INTEGER,
                    //    RegDate TEXT,
                    //    ModDate TEXT,
                    //    PRIMARY KEY(Link)
                    //);
                }
            }
        }

        public class SQLITE_MASTER
        {
            public string type { get; set; }
            public string name { get; set; }
            public string tbl_name { get; set; }
            public string rootpage { get; set; }
            public string sql { get; set; }
        }

        public T_CATEGORY InsertT_CATEGORY(T_CATEGORY data)
        {
            SqlParamCollection p = new SqlParamCollection();
            p.Add("@Category", data.Category);
            p.Add("@IsSearchTitle", data.IsSearchTitle);
            p.Add("@Type", data.Type);
            p.Add("@SearchText", data.SearchText);
            p.Add("@NoSearchText", data.NoSearchText);
            p.Add("@UpCategorySeq", data.UpCategorySeq);
            p.Add("@Number", data.Number);
            p.Add("@RegDate", data.RegDate.ToString("yyyy-MM-dd HH:mm:ss"));
            p.Add("@ModDate", data.ModDate.ToString("yyyy-MM-dd HH:mm:ss"));
            return ExecuteDataSetSingle<T_CATEGORY>(@"
INSERT INTO T_CATEGORY (
Category,
IsSearchTitle,
Type,
SearchText,
NoSearchText,
UpCategorySeq,
Number,
RegDate,
ModDate
) VALUES (
@Category,
@IsSearchTitle,
@Type,
@SearchText,
@NoSearchText,
@UpCategorySeq,
@Number,
@RegDate,
@ModDate
);
SELECT * FROM T_CATEGORY ORDER BY CategorySeq DESC LIMIT 1
", p);
        }

        public int UpdateT_CATEGORY(T_CATEGORY data)
        {
            SqlParamCollection p = new SqlParamCollection();
            p.Add("@CategorySeq", data.CategorySeq);
            p.Add("@Category", data.Category);
            p.Add("@IsSearchTitle", data.IsSearchTitle);
            p.Add("@Type", data.Type);
            p.Add("@SearchText", data.SearchText);
            p.Add("@NoSearchText", data.NoSearchText);
            p.Add("@UpCategorySeq", data.UpCategorySeq);
            p.Add("@Number", data.Number);
            p.Add("@RegDate", data.RegDate.ToString("yyyy-MM-dd HH:mm:ss"));
            p.Add("@ModDate", data.ModDate.ToString("yyyy-MM-dd HH:mm:ss"));
            return ExecuteNonQuery(@"
UPDATE T_CATEGORY 
SET 
Category=@Category,
IsSearchTitle=@IsSearchTitle,
Type=@Type,
SearchText=@SearchText,
NoSearchText=@NoSearchText,
UpCategorySeq=@UpCategorySeq,
Number=@Number,
RegDate=@RegDate,
ModDate=@ModDate
WHERE CategorySeq=@CategorySeq
", p);
        }

        public int DeleteT_CATEGORY(T_CATEGORY data)
        {
            SqlParamCollection p = new SqlParamCollection();
            p.Add("@CategorySeq", data.CategorySeq);
            return ExecuteNonQuery(@"
DELETE FROM T_CATEGORY WHERE CategorySeq=@CategorySeq
", p);
        }

            public List<T_CATEGORY> SelectT_CATEGORY()
        {
            SqlParamCollection p = new SqlParamCollection();
            var result = ExecuteDataSet<T_CATEGORY>(@"
SELECT * FROM T_CATEGORY ORDER BY Number Desc
", p);
            return result;
        }

        public int InsertT_ARTICLE(IEnumerable<T_ARTICLE> datas)
        {
            var lp = new List<SqlParamCollection>();
            foreach (var data in datas)
            {
                var p = new SqlParamCollection();
                p.Add("@Type", data.Type);
                p.Add("@CategorySeq", data.CategorySeq);
                p.Add("@Press", data.Press);
                p.Add("@PressLink", data.PressLink);
                p.Add("@InfoTime", data.InfoTime.ToString("yyyy-MM-dd HH:mm:ss"));
                p.Add("@Title", data.Title);
                p.Add("@Link", data.Link);
                p.Add("@Description", data.Description);
                p.Add("@ImageUrl", data.ImageUrl);
                p.Add("@IsRead", data.IsRead);
                p.Add("@IsDelete", data.IsDelete);
                p.Add("@RegDate", data.RegDate);
                p.Add("@ReadDate", data.ReadDate);
                p.Add("@ModDate", data.ModDate);
                lp.Add(p);
            }
            return ExecuteNonQuery(@"
REPLACE INTO T_ARTICLE (
Type,
CategorySeq,
Press,
PressLink,
InfoTime,
Title,
Link,
Description,
ImageUrl,
IsRead,
IsDelete,
RegDate,
ReadDate,
ModDate
) VALUES (
@Type,
@CategorySeq,
@Press,
@PressLink,
@InfoTime,
@Title,
@Link,
@Description,
@ImageUrl,
@IsRead,
@IsDelete,
@RegDate,
@ReadDate,
@ModDate
)


", lp);
        }

        public int DeleteT_ARTICLE(T_ARTICLE data)
        {
            SqlParamCollection p = new SqlParamCollection();
            p.Add("@Link", data.Link);
            return ExecuteNonQuery(@"
DELETE FROM T_ARTICLE WHERE Link=@Link
", p);
        }

        public int DeleteT_ARTICLE(DateTime enddate)
        {
            SqlParamCollection p = new SqlParamCollection();
            p.Add("@enddate", enddate.ToString("yyyy-MM-dd HH:mm:ss"));
            return ExecuteNonQuery(@"
UPDATE T_ARTICLE 
SET 
IsDelete = 1,
ModDate = datetime('now')
WHERE InfoTime<@enddate AND IsRead=1
", p);
        }

        public int SelectCountT_ARTICLE(int CategorySeq)
        {
            SqlParamCollection p = new SqlParamCollection();
            p.Add("@CategorySeq", CategorySeq);
            string q = @"
SELECT COUNT(*) Value FROM T_ARTICLE WHERE CategorySeq=@CategorySeq AND IsRead = 0 AND IsDelete = 0
";
            var result = ExecuteDataSetSingle<int>(q, p);
            return result;
        }

        public List<T_ARTICLE> SelectT_ARTICLE(int CategorySeq)
        {
            SqlParamCollection p = new SqlParamCollection();
            p.Add("@CategorySeq", CategorySeq);
            string q = @"
SELECT * FROM T_ARTICLE A WHERE A.CategorySeq=@CategorySeq AND A.IsDelete = 0 ORDER BY A.InfoTime Desc
";
            var result = ExecuteDataSet<T_ARTICLE>(q, p);
            return result;
        }

        public List<T_ARTICLE> SelectT_ARTICLE(IEnumerable<int> CategorySeqs)
        {
            SqlParamCollection p = new SqlParamCollection();
            string q = string.Format(@"
SELECT * FROM T_ARTICLE WHERE CategorySeq IN ({0}) AND IsDelete = 0 ORDER BY InfoTime Desc
", string.Join(",", CategorySeqs.ToArray()));
            var result = ExecuteDataSet<T_ARTICLE>(q, p);
            return result;
        }

        public List<T_ARTICLE> SelectT_ARTICLE(string searchText = "", bool isTitle = true)
        {
            SqlParamCollection p = new SqlParamCollection();
            p.Add("@searchText", "%" + searchText + "%");
            string q = @"
SELECT * FROM T_ARTICLE {0} ORDER BY InfoTime Desc
";
            if (string.IsNullOrWhiteSpace(searchText) == false) 
            {
                if (isTitle == true)
                {
                    q = string.Format(q, "WHERE Title LIKE @searchText");
                }
                else
                {
                    q = string.Format(q, "WHERE Title LIKE @searchText OR Description LIKE @searchText");
                }
            }
            else
            {
                q = string.Format(q, "");
            }
            var result = ExecuteDataSet<T_ARTICLE>(q, p);
            return result;
        }

//        public int InsertT_READ(IEnumerable<T_READ> datas)
//        {
//            var lp = new List<SqlParamCollection>();
//            foreach (var data in datas)
//            {
//                var p = new SqlParamCollection();
//                p.Add("@Link", data.Link);
//                p.Add("@IsRead", data.IsRead);
//                p.Add("@IsDelete", data.IsDelete);
//                p.Add("@RegDate", data.RegDate.ToString("yyyy-MM-dd HH:mm:ss"));
//                p.Add("@ModDate", data.ModDate.ToString("yyyy-MM-dd HH:mm:ss"));
//                lp.Add(p);
//            }
//            return ExecuteNonQuery(@"
//REPLACE INTO T_READ (
//Link,
//IsRead,
//IsDelete,
//RegDate,
//ModDate
//) VALUES (
//@Link,
//@IsRead,
//@IsDelete,
//@RegDate,
//@ModDate
//)", lp);
//        }

//        public List<T_READ> SelectT_READ()
//        {
//            SqlParamCollection p = new SqlParamCollection();
//            var result = ExecuteDataSet<T_READ>(@"
//SELECT * FROM T_READ ORDER BY RegDate Desc
//", p);
//            return result;
//        }

        public int InsertT_OPTION(IEnumerable<T_OPTION> datas)
        {
            var lp = new List<SqlParamCollection>();
            foreach (var data in datas)
            {
                var p = new SqlParamCollection();
                p.Add("@Key", data.Key);
                p.Add("@Value", data.Value);
                lp.Add(p);
            }
            return ExecuteNonQuery(@"
REPLACE INTO T_OPTION (
Key,
Value
) VALUES (
@Key,
@Value
)", lp);
        }

        public List<T_OPTION> SelectT_OPTION()
        {
            SqlParamCollection p = new SqlParamCollection();
            var result = ExecuteDataSet<T_OPTION>(@"
SELECT * FROM T_OPTION
", p);
            return result;
        }

        public IEnumerable<SQLITE_MASTER> GetSQLITE_MASTER()
        {
            var tables = ExecuteDataSet<SQLITE_MASTER>(
                @"SELECT type, name, tbl_name, rootpage, sql FROM SQLITE_MASTER;",
                new SqlParamCollection()
                );
            return tables;
        }

        public int ExecuteNonQuery(string qurey, SqlParamCollection p)
        {
            using (SqlHelper sql = new SqlHelper(ConnectionString))
            {
                return sql.ExecuteNonQuery(qurey, p);
            }
        }

        public int ExecuteNonQuery(string qurey, List<SqlParamCollection> p)
        {
            using (SqlHelper sql = new SqlHelper(ConnectionString))
            {
                return sql.ExecuteNonQuery(qurey, p);
            }
        }

        public List<T> ExecuteDataSet<T>(string qurey, SqlParamCollection p)// where T : APIModelbase
        {
            using (SqlHelper sql = new SqlHelper(ConnectionString))
            {
                return sql.GetData<T>(sql.ExecuteDataSet(qurey, p));
            }
        }

        public T ExecuteDataSetSingle<T>(string qurey, SqlParamCollection p)// where T : APIModelbase
        {
            using (SqlHelper sql = new SqlHelper(ConnectionString))
            {
                return sql.GetSingleData<T>(sql.ExecuteDataSet(qurey, p));
            }
        }
    }
}
