using System.Collections.ObjectModel;

namespace Todo;

public partial class TodoListPage : ContentPage
{
    TodoItemDatabase database;
    public ObservableCollection<TodoItem> Items { get; set; } = new();
    private double subTtl = 0;
    private double grdTtl = 0;
    public TodoListPage(TodoItemDatabase todoItemDatabase)
    {
        InitializeComponent();
        database = todoItemDatabase;
        BindingContext = this;
    }


    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        GetItemsAsync();
    }

    private async void GetItemsAsync()
    {
        var items = await database.GetItemsAsync();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
        });
        grdTtl = items.Sum(i => i.Total);
        LblTotal.Text = ($"Grand Total: {grdTtl}");
        subTtl = items.Where(x => x.Done == true).Sum(i => i.Total);
        LblSubTtl.Text = ($"Done Total: {subTtl}");
    }

    async void OnItemAdded(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(TodoItemPage), true, new Dictionary<string, object>
        {
            ["Item"] = new TodoItem()
        });
    }

    private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not TodoItem item)
            return;

        await Shell.Current.GoToAsync(nameof(TodoItemPage), true, new Dictionary<string, object>
        {
            ["Item"] = item
        });
    }

    private async void cbDone_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        var item = (TodoItem)((CheckBox)sender).BindingContext;
        if (item is not null)
        {
            await database.SaveItemAsync(item);
            Items.Clear();
            GetItemsAsync();
        }
        else
        {
            await DisplayAlert("Information","Something went wrong!", "OK");
        }
    }

    private void tiImport_Clicked(object sender, EventArgs e)
    {

    }

    private async void tiExport_Clicked(object sender, EventArgs e)
    {
        var items = await database.GetItemsAsync();
        await database.ExportsAsync(items.ToList(), "exports.json");
        await Task.Delay(1000);
        await DisplayAlert("Information", "Exported Successfully", "Ok");
    }
}
