using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// 获取数据库链接串
    /// </summary>
    public class SqlHelper
    {
        private static readonly Lazy<SqlHelper> lazy = new Lazy<SqlHelper>(() => new SqlHelper());

        public static SqlHelper Instance { get { return lazy.Value; } }

        private static Dictionary<string,string> dicDataBase = new Dictionary<string,string>();
        /// <summary>
        /// 数据库类型
        /// </summary>
        private string dataBase;
        /// <summary>
        /// 是否外网环境
        /// </summary>
        private bool isExtranet;

        private SqlHelper()
        {
            Console.WriteLine("构造函数");
            var DataBase = ConfigurationManager.AppSettings["DataBase"];
            if (string.IsNullOrEmpty(DataBase.Trim())) {
                throw new Exception("请检查配置，是否缺少DataBase,且DataBase的值只能为LocalHost,AliYun或AliYunTest");
            } else {
                if (DataBase != "LocalHost" && DataBase != "AliYun" && DataBase != "AliYunTest") {
                    throw new Exception("DataBase的值只能为LocalHost,AliYun或AliYunTest");
                }
            }
            var IsExtranet = ConfigurationManager.AppSettings["IsExtranet"];
            if (string.IsNullOrEmpty(IsExtranet)) {
                throw new Exception("请检查配置，是否缺少IsExtranet");
            }
            this.isExtranet = IsExtranet == "true";
            this.dataBase = DataBase.Trim();
            var DBConnString = "";
            var release = 0;
            if (dataBase == "AliYunTest" || dataBase == "LocalHost") {
                if (!isExtranet) {
                    DBConnString = "Server=rm-bp1d45xkyl13o47j52o.mysql.rds.aliyuncs.com;port=3306;User Id=root;password=ltl@aliyun1209;Database=aliyuntextforltl;persist security info=True;character set=utf8;SslMode=none;Allow User Variables=True";
                } else {
                    DBConnString = "Server=rm-bp1d45xkyl13o47j52o.mysql.rds.aliyuncs.com;port=3306;User Id=root;password=ltl@aliyun1209;Database=aliyuntextforltl;persist security info=True;character set=utf8;SslMode=none;Allow User Variables=True";
                }
                release = 0;
            } else if (dataBase == "AliYun") {
                if (ConfigurationManager.ConnectionStrings["ConnString"] == null) {
                    throw new Exception("连接正式库请配置ConnString");
                }
                DBConnString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                release = 1;
            }
            if (dicDataBase == null || dicDataBase.Count == 0) {
                using (var connApp = new MySqlConnection(DBConnString)) {
                    SimpleCRUD.SetDialect(SimpleCRUD.Dialect.MySQL);
                    var dataBaseList = connApp.Query<DataBaseConnection>($"select `DataBase`,`ConnStringType`,`ConnString`,`IsExtranet` from DataBaseConnection where IsRelease={release}");
                    if (dataBaseList != null && dataBaseList.Count() > 0) {
                        foreach (var item in dataBaseList) {
                            var key = item.DataBase + "_" + (int)item.ConnStringType + "_" + item.IsExtranet;//"数据库名称_连接类型_是否外网"
                            if (!dicDataBase.ContainsKey(key)) {
                                dicDataBase.Add(key,item.ConnString);
                            }
                        }
                    } else {
                        throw new Exception("请检查配置，无法初始化数据库配置");
                    }
                }
            }
        }

        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <param name="dataBaseEnum"></param>
        /// <param name="dBEnum"></param>
        /// <param name="dBNetEnumType"></param>
        /// <returns></returns>
        public MySqlConnection DataBaseConn(DBNameEnumType dataBaseEnum,DBEnumType? dBEnum = null,DBNetEnumType? dBNetEnumType = null)
        {
            var key = string.Empty;
            var database = 0;
            var extranet = isExtranet ? 1 : 0;
            if (dBEnum.HasValue) {
                database = (int)dBEnum.Value;
            } else {
                database = (int)DBEnumType.主库;
            }
            if (dataBase == "AliYunTest") {//数据库类型，优先使用配置文件的
                database = (int)DBEnumType.测试库;
            } else if (dataBase == "LocalHost") {
                database = (int)DBEnumType.本地测试库;
            }
            if (dBNetEnumType.HasValue) {//内外网，优先使用程序中的
                extranet = (int)dBNetEnumType.Value;
            }
            key = dataBaseEnum.ToString() + "_" + database + "_" + extranet;
            var DBConnString = dicDataBase[key];
            if (string.IsNullOrWhiteSpace(DBConnString)) {
                throw new Exception("数据库连接错误");
            }
            var connection = new MySqlConnection(DBConnString);
            SimpleCRUD.SetDialect(SimpleCRUD.Dialect.MySQL);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 连接测试库
        /// </summary>
        /// <param name="dataBaseEnum"></param>
        /// <param name="dBEnum"></param>
        /// <param name="dBNetEnumType"></param>
        /// <returns></returns>
        public MySqlConnection DataBaseConnTest(DBNameEnumType dataBaseEnum,DBEnumType? dBEnum = null,DBNetEnumType? dBNetEnumType = null)
        {
            var key = string.Empty;
            var extranet = isExtranet ? 1 : 0;
            if (dBNetEnumType.HasValue) {//内外网，优先使用程序中的
                extranet = (int)dBNetEnumType.Value;
            }
            key = dataBaseEnum.ToString() + "_" + (int)DBEnumType.测试库 + "_" + extranet;
            var DBConnString = dicDataBase[key];
            if (string.IsNullOrWhiteSpace(DBConnString)) {
                throw new Exception("数据库连接错误");
            }
            var connection = new MySqlConnection(DBConnString);
            SimpleCRUD.SetDialect(SimpleCRUD.Dialect.MySQL);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 
        /// </summary>
        public enum DBNameEnumType
        {
            /// <summary>
            /// App
            /// </summary>
            App,
            /// <summary>
            /// Web
            /// </summary>
            Web
        }

        /// <summary>
        /// 
        /// </summary>
        public enum DBNetEnumType
        {
            /// <summary>
            /// 内网
            /// </summary>
            内网 = 0,
            /// <summary>
            /// 外网
            /// </summary>
            外网 = 1
        }

        /// <summary>
        /// 
        /// </summary>
        public enum DBEnumType
        {
            /// <summary>
            /// 主库
            /// </summary>
            主库 = 1,
            /// <summary>
            /// 读库
            /// </summary>
            读库 = 2,
            /// <summary>
            /// 读写库
            /// </summary>
            读写库 = 3,
            /// <summary>
            /// 测试库
            /// </summary>
            测试库 = 4,
            /// <summary>
            /// 本地测试库
            /// </summary>
            本地测试库 = 5
        }

        /// <summary>
        /// 代码中可以覆盖默认值
        /// </summary>
        public class DataBaseConnection
        {
            /// <summary>
            /// 数据库 App,Trade,Web,Share
            /// </summary>
            public string DataBase { get; set; }
            /// <summary>
            /// 数据连接类型 正式连接 = 1,只读连接 = 2,读写连接 = 3
            /// </summary>
            public int ConnStringType { get; set; }
            /// <summary>
            ///  连接串
            /// </summary>
            public string ConnString { get; set; }
            /// <summary>
            /// 是否外网 是否外网 0：内网 ，1： 外网
            /// </summary>
            public int IsExtranet { get; set; }
        }

        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <param name="action"></param>
        public static void DbExecute(Action<MySqlConnection> action)
        {
            using (var db = Instance.DataBaseConn(DBNameEnumType.Web)) {
                action(db);
            }
        }

        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <param name="action"></param>
        public static void DbQuery(Action<MySqlConnection> action)
        {
            using (var db = Instance.DataBaseConn(DBNameEnumType.Web)) {
                action(db);
            }
        }
    }
}
