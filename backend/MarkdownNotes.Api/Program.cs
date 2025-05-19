using Microsoft.EntityFrameworkCore;
using MarkdownNotes.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// --- CORS設定を追加 ---
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; // ポリシー名

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173") // フロントエンドのURL
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});
// --- CORS設定ここまで ---

// Add services to the container.

builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
        // マイグレーションが NotesDbContext と同じアセンブリにあることを明示
        b => b.MigrationsAssembly(typeof(NotesDbContext).Assembly.FullName)));

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<NotesDbContext>();
            context.Database.Migrate(); // マイグレーションを適用
            Console.WriteLine("Database migration applied successfully or no pending migrations.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}

// --- CORSミドルウェアを有効化 ---
app.UseCors(MyAllowSpecificOrigins); // UseRouting の後、UseAuthorization の前あたりが良い
// --- CORSミドルウェアここまで ---

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();