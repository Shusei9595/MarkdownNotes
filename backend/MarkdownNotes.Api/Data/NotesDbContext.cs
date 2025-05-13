using Microsoft.EntityFrameworkCore;
using MarkdownNotes.Api.Models; // Noteモデルを使うために追加

namespace MarkdownNotes.Api.Data
{
    public class NotesDbContext : DbContext
    {
        public NotesDbContext(DbContextOptions<NotesDbContext> options)
            : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; } = null!; // "Notes" という名前のテーブルに対応

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ここでモデルの詳細な設定（例：初期データ投入など）を行うことができますが、
            // 今回はシンプルに、Noteエンティティの基本的な設定はEF Coreの規約に任せます。

            // 例として、もし初期データを入れたい場合は以下のように書けます。
            /*
            modelBuilder.Entity<Note>().HasData(
                new Note { Id = 1, Title = "最初のノート", MarkdownContent = "# こんにちは", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            */

            base.OnModelCreating(modelBuilder);
        }
    }
}