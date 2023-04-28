using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using XFSQLiteEncrypt.Models;

namespace XFSQLiteEncrypt.Data
{
	public class Db
	{
		public const string DatabaseFilename = "user.db";
		private static readonly Db instance = new Db();

		public string _connectionString { get; set; }

		public static Db Instance
		{
			get => instance;
		}

		public Db()
		{
			SQLitePCL.Batteries_V2.Init();

			var baseConnectionString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DatabaseFilename);

			baseConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder()
			{
				//conn.ConnectionString = baseConnectionString; //Xamarin에서는 에러 발생, DataSource 방식 사용
				Password = "!@Kor_9218_누구냐_池",
				DataSource = baseConnectionString,
				Mode = SqliteOpenMode.ReadWriteCreate,
				Pooling = true,
				Cache = SqliteCacheMode.Shared
			}.ToString();

			_connectionString = baseConnectionString;


			using (SqliteConnection connection = CreateConnection())
			{
				CreateTableIfNotExists(connection);
			}

		}

		//Dapper Style
		public async Task<int> UserInsert(User user)
		{
			int resultId = -1;

			using (SqliteConnection cnn = CreateConnection())
			{
				using (var transaction = cnn.BeginTransaction())
				{
					try
					{
						var param = new DynamicParameters();
						param.Add("@UserId", user.UserId);
						param.Add("@UserName", user.UserName);
						param.Add("@Password", user.Password);

						string sql =
						@"
							INSERT INTO User (UserId,UserName,Password,CreatedDate,ModifiedDate)
							VALUES (@UserId,@UserName,@Password, strftime('%Y-%m-%d %H:%M:%f','now','localtime'), strftime('%Y-%m-%d %H:%M:%f','now','localtime'))
							RETURNING Id
						";

						// Dapper
						resultId = await cnn.ExecuteScalarAsync<int>(sql, param, transaction);

						// Dapper Transaction
						//resultId= await transaction.ExecuteScalarAsync<int>(sql, param);

						transaction.Commit();
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						resultId = -1;
					}
				}
			}

			return resultId;
		}


		//CreateCommand Style
		public async Task<int> UserInsert2(User user)
		{
			int resultId = -1;

			using (SqliteConnection cnn = CreateConnection())
			{
				using (var transaction = cnn.BeginTransaction())
				{
					try
					{
						var command = cnn.CreateCommand();
						command.Connection = cnn;
						command.Transaction = transaction;
						command.CommandText =
						@"
							INSERT INTO User (UserId,UserName,Password,CreatedDate,ModifiedDate)
							VALUES (@UserId, @UserName, @Password, strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'))
							RETURNING Id
						";

						command.Parameters.AddWithValue("@UserId", user.UserId);
						command.Parameters.AddWithValue("@UserName", user.UserName);
						command.Parameters.AddWithValue("@Password", user.Password);

						var result = await command.ExecuteScalarAsync();
						resultId = Convert.ToInt32(result);


						transaction.Commit();
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						resultId = -1;
					}
				}
			}

			return resultId;
		}


		public SqliteConnection CreateConnection()
		{
			var connection = new SqliteConnection(_connectionString);
			//connection.ConnectionString = _connectionString;
			//connection.ConnectionTimeout = 120; //기본값 15초, 이건 변경 못함.
			connection.Open();

			return connection;
		}

		private void CreateTableIfNotExists(SqliteConnection conn)
		{
			string sql = string.Empty;

			//Item
			sql = @"CREATE TABLE if not exists User(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
						[UserId] TEXT NOT NULL DEFAULT(''),
						[UserName] TEXT NOT NULL DEFAULT(''),
						[Password] TEXT NOT NULL DEFAULT(''),
						[CreatedDate] TEXT NOT NULL DEFAULT(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime')), 
						[ModifiedDate] TEXT NOT NULL DEFAULT(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'))
					)";
			new SqliteCommand(sql, conn).ExecuteNonQuery();

			sql = @"CREATE Unique INDEX if not exists [User.idx1] ON User ([UserId])";
			new SqliteCommand(sql, conn).ExecuteNonQuery();

			sql = @"CREATE INDEX if not exists [User.idx1] ON User ([UserName])";
			new SqliteCommand(sql, conn).ExecuteNonQuery();

		}

		public string DBToExport()
		{
			var backupPath = Path.Combine(FileSystem.CacheDirectory, DatabaseFilename);

			var conn = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder()
			{
				//conn.ConnectionString = baseConnectionString; //Xamarin에서는 에러 발생, DataSource 방식 사용
				Password = "!@Kor_9218_누구냐_池",
				DataSource = backupPath,
				Mode = SqliteOpenMode.ReadWriteCreate,
				Pooling = true,
				Cache = SqliteCacheMode.Shared
			};

			var backupConnection = new SqliteConnection(conn.ToString());
			backupConnection.Open();

			var connection = new SqliteConnection(_connectionString);
			connection.Open();
			connection.BackupDatabase(backupConnection);
			connection.Close();

			backupConnection.Close();

			return backupPath;
		}
	}
}
