using Newtonsoft.Json;
using SQLite;
using static Android.Graphics.ColorSpace;

namespace Todo
{
    public class TodoItemDatabase
    {
        SQLiteAsyncConnection Database;

        public TodoItemDatabase()
        {
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<TodoItem>();
        }
        public async Task<List<TodoItem>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<TodoItem>().OrderBy(d => d.Done).ToListAsync();
        }

        public async Task<TodoItem> ImportAsync(string filePath)
        {
            await Init();
            // Get the file path
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filePath);

            // Read the JSON string from the file
            var json = await File.ReadAllTextAsync(file);

            // Convert the JSON string back to a model
            var model = JsonSerializer.Deserialize<TodoItem>(json);

            return model;
        }
        public async Task ExportsAsync(List<TodoItem> todoItem, string filePath)
        {
            await Init();
            var json = JsonConvert.SerializeObject(todoItem);
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filePath);
            await File.WriteAllTextAsync(file, json);
        }

        public async Task<List<TodoItem>> GetItemsNotDoneAsync()
        {
            await Init();
            return await Database.Table<TodoItem>().Where(t => t.Done).ToListAsync();

            // SQL queries are also possible
            //return await Database.QueryAsync<TodoItem>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
        }

        public async Task<TodoItem> GetItemAsync(int id)
        {
            await Init();
            return await Database.Table<TodoItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveItemAsync(TodoItem item)
        {
            await Init();
            if (item.ID != 0)
                return await Database.UpdateAsync(item);
            else
                return await Database.InsertAsync(item);
        }

        public async Task<int> DeleteItemAsync(TodoItem item)
        {
            await Init();
            return await Database.DeleteAsync(item);
        }
    }
}
