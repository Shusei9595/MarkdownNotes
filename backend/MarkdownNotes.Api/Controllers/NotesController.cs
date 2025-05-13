using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // EF Core を使うために追加
using MarkdownNotes.Api.Data;       // NotesDbContext を使うために追加
using MarkdownNotes.Api.Models;     // Note モデルを使うために追加
using System.Collections.Generic;   // List を使うために追加
using System.Linq;                  // LINQ を使うために追加
using System.Threading.Tasks;       //非同期処理のために追加
using System;                       // DateTime のために追加
using Markdig;
using Markdig.Syntax;

namespace MarkdownNotes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // このコントローラへのルートは /api/notes となる
    public class NotesController : ControllerBase
    {
        private readonly NotesDbContext _context;

        public NotesController(NotesDbContext context)
        {
            _context = context;
        }

         // GET: api/notes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotes()
        {
            if (_context.Notes == null)
            {
                return NotFound("Notes table is not found."); // 通常はありえないが念のため
            }
            return await _context.Notes.OrderByDescending(n => n.UpdatedAt).ToListAsync();
        }

        // POST: api/notes
        [HttpPost]
        public async Task<ActionResult<Note>> PostNote(NoteCreateDto noteDto) // Noteの代わりにDTOを使用
        {
            if (_context.Notes == null)
            {
                return Problem("Entity set 'NotesDbContext.Notes' is null.");
            }

            var note = new Note
            {
                Title = noteDto.Title ?? "無題のノート", // DTOからタイトルを取得、nullならデフォルト値
                MarkdownContent = noteDto.MarkdownContent ?? "", // DTOからMarkdownコンテンツを取得
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // 作成されたノートの情報と、そのノートを取得するためのURLを返す (RESTfulなプラクティス)
            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
        }

        // DTO (Data Transfer Object) をコントローラ内に一時的に定義 (後で別ファイルに移動推奨)
        public class NoteCreateDto
        {
            public string? Title { get; set; }
            public string? MarkdownContent { get; set; }
        }

        // 個別ノート取得用のヘルパーエンドポイント (CreatedAtActionで使うため)
        // GET: api/notes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNoteById(int id)
        {
            if (_context.Notes == null)
            {
                return NotFound();
            }
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            return note;
        }

        // GET: api/notes/{id}/html
        [HttpGet("{id}/html")]
        public async Task<ActionResult<string>> GetNoteAsHtml(int id)
        {
            if (_context.Notes == null)
            {
                return NotFound("Notes table is not found.");
            }
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound($"Note with ID {id} not found.");
            }

            if (string.IsNullOrEmpty(note.MarkdownContent))
            {
                return Ok(string.Empty); // Markdownが空なら空のHTMLを返す
            }

            var html = Markdown.ToHtml(note.MarkdownContent);
            return Ok(html); // HTML文字列を直接返す
        }

        // POST: api/notes/lint
        [HttpPost("lint")]
        public ActionResult<object> LintMarkdown([FromBody] LintRequestDto request)
        {
            if (string.IsNullOrEmpty(request.MarkdownText))
            {
                return Ok(new { isValid = true, message = "Markdown is empty, considered valid." });
            }

            try
            {
                // Markdown.ToHtml を試行する。例外が発生しなければ基本的な構文はOKとみなす。
                // 実際にはパース結果を捨てるので無駄があるが、簡易的なチェックとして。
                var pipeline = new MarkdownPipelineBuilder().Build();
                MarkdownDocument document = Markdig.Markdown.Parse(request.MarkdownText, pipeline);

                // もしMarkdigにLint機能があればそれを使うのがベストだが、
                // 標準ではエラー報告よりは「どう解釈するか」に主眼がある。
                return Ok(new { isValid = true, message = "Markdown syntax appears valid." });
            }
            catch (Exception ex)
            {
                // 何らかのパースエラーが発生した場合
                // 本来はもっと詳細なエラー情報を返すべき
                return BadRequest(new { isValid = false, message = "Invalid Markdown syntax.", error = ex.Message });
            }
        }

        // Lintリクエスト用のDTO
        public class LintRequestDto
        {
            public string? MarkdownText { get; set; }
        }
    }
}