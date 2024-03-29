using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ApiCoreDapperCrud.Entities;
using ApiCoreDapperCrud.Enums;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace ApiCoreDapperCrud.Infrastructure.Services
{
    public class TodoService : ITodoService
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public TodoService(IConfiguration config)
        {
            _config = config;
            // Either one would work
            _connectionString = config.GetValue<string>("DataSources:SqlServer:ConnectionString");
            _connectionString = config.GetSection("DataSources:SqlServer:ConnectionString").Value;
        }

        private IDbConnection Connection => new SqlConnection(_connectionString);

        public async Task<List<Todo>> FetchMany(TodoShow show = TodoShow.All)
        {
            using (var conn = Connection)
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string sql;
                if (show == TodoShow.Completed || show == TodoShow.Pending)
                    sql = "Select Id, Title, Completed, CreatedAt, UpdatedAt From Todo Where " +
                          $"Completed={(show == TodoShow.Completed ? 1 : 0)} Order By CreatedAt";
                else
                    sql = "Select Id, Title, Completed, CreatedAt, UpdatedAt From Todo Order By CreatedAt";

                var todos = (await conn.QueryAsync<Todo>(sql)).ToList();
                return todos;
            }
        }

        public async Task<Todo> GetById(int id)
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                return (await dbConnection.QueryAsync<Todo>("SELECT * FROM Todo WHERE Id = @Id", new {Id = id}))
                    .FirstOrDefault();
            }
        }

        public async Task<Todo> GetProxyById(int id)
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                return (await dbConnection.QueryAsync<Todo>("SELECT Id FROM Todo WHERE Id = @Id", new {Id = id}))
                    .FirstOrDefault();
            }
        }


        public async Task<Todo> CreateTodo(Todo todo)
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                todo.Id = await dbConnection.ExecuteScalarAsync<int>(
                    "INSERT INTO Todo (Title, Description, Completed) VALUES (@Title, @Description, @Completed); Select SCOPE_IDENTITY();",
                    todo);
                return todo;
            }
        }

        public async Task<Todo> Update(Todo currentTodo, Todo todoFromUser)
        {
            using (var dbConnection = Connection)
            {
                todoFromUser.Id = currentTodo.Id;
                var now = DateTime.UtcNow;
                todoFromUser.UpdatedAt = now;
                dbConnection.Open();
                await dbConnection.QueryAsync(
                    "UPDATE [dbo].[Todo] SET Title = @Title,  Description  = @Description, Completed= @Completed, UpdatedAt= @UpdatedAt WHERE id = @Id",
                    todoFromUser);
                return todoFromUser;
            }
        }

        /// <summary>
        ///     Deletes a To do
        /// </summary>
        /// <param name="todoId"></param>
        /// <returns></returns>
        public async Task Delete(int todoId)
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                await dbConnection.ExecuteAsync("DELETE FROM Todo WHERE Id=@Id", new {Id = todoId});
            }
        }

        public async Task DeleteAll()
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();
                await dbConnection.ExecuteAsync("DELETE FROM Todo");
            }
        }
    }
}