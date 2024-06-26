using System.IO;
using Windows.Storage;
using FluentIcons.WinUI;
using LiteDB;
using Microsoft.UI.Xaml;
using WinUI3Utilities;

namespace DatabaseSystem;

public partial class App : Application
{
    public App()
    {
        _ = this.UseSegoeMetrics();
    }

    public const string DatabaseName = "database.db";

    public static string DatabasePath => Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName);

    public static LiteDatabase Database { get; set; } = new(DatabasePath);

    public static ILiteCollection<T> GetCollection<T>() where T : BsonEntry =>
        Database.GetCollection<T>(typeof(T).Name);

    public static ILiteCollection<T> DropGetCollection<T>() where T : BsonEntry
    {
        _ = Database.DropCollection(typeof(T).Name);
        return Database.GetCollection<T>(typeof(T).Name);
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Initialize(new() { BackdropType = BackdropType.MicaAlt });
        _window.Activate();
    }

    private Window _window = null!;

    public static AppViewModel AppViewModel { get; } = new();
}
