using System;
using System.ComponentModel.DataAnnotations; // これを追加

namespace MarkdownNotes.Api.Models
{
    public class Note
    {
        public int Id { get; set; } // ノートの一意なID (主キー)

        [Required] // タイトルは必須
        [StringLength(100)] // タイトルの最大長
        public string Title { get; set; } = string.Empty;

        public string MarkdownContent { get; set; } = string.Empty; // Markdown形式の本文

        public DateTime CreatedAt { get; set; } // 作成日時

        public DateTime UpdatedAt { get; set; } // 更新日時

        public Note()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}