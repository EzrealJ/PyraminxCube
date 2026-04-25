namespace PyraminxCube.Rbac.EntityFrameworkCore
{
    /// <summary>
    /// RBAC 数据库配置选项
    /// </summary>
    public class RbacDbOptions
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public RbacDbType DbType { get; set; } = RbacDbType.Sqlite;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = "Data Source=rbac.db";

        /// <summary>
        /// MySQL 服务器版本（仅 MySQL 需要）
        /// </summary>
        public string? MySqlVersion { get; set; }
    }

    /// <summary>
    /// 支持的数据库类型
    /// </summary>
    public enum RbacDbType
    {
        /// <summary>
        /// SQLite（默认，适合开发和轻量部署）
        /// </summary>
        Sqlite,

        /// <summary>
        /// MySQL / MariaDB
        /// </summary>
        MySql,

        /// <summary>
        /// SQL Server
        /// </summary>
        SqlServer,

        /// <summary>
        /// PostgreSQL
        /// </summary>
        PostgreSql
    }
}
