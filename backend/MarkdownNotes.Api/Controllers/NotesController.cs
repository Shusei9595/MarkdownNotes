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

         // PUT: api/notes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNote(int id, NoteUpdateDto noteDto) // DTOを使用
        {
            // 受け取ったIDとDTOのIDがもしあれば一致しているか確認（必須ではない）
            // if (id != noteDto.Id) // もしNoteUpdateDtoにIdプロパティを含めるなら
            // {
            //     return BadRequest("ID mismatch");
            // }

            if (_context.Notes == null)
            {
                return Problem("Entity set 'NotesDbContext.Notes' is null.");
            }

            var noteToUpdate = await _context.Notes.FindAsync(id);

            if (noteToUpdate == null)
            {
                return NotFound($"Note with ID {id} not found.");
            }

            // DTOから受け取った値でノートのプロパティを更新
            // null合体演算子 (??) を使って、DTOの値がnullの場合は既存の値を維持する (部分更新を許容する場合)
            // もし常に全ての値を上書きする場合は、DTOのプロパティを必須にするか、nullチェックを厳密に行う
            noteToUpdate.Title = noteDto.Title ?? noteToUpdate.Title;
            noteToUpdate.MarkdownContent = noteDto.MarkdownContent ?? noteToUpdate.MarkdownContent;
            noteToUpdate.UpdatedAt = DateTime.UtcNow; // 更新日時を現在時刻に

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // 非常に稀だが、同時に同じノートが更新/削除された場合の競合ハンドリング
                if (!NoteExists(id)) // NoteExistsは後で定義するヘルパーメソッド
                {
                    return NotFound();
                }
                else
                {
                    throw; // その他の競合エラーはそのままスロー
                }
            }

            return NoContent(); // 成功時は 204 No Content を返すのが一般的 (レスポンスボディなし)
                                // または return Ok(noteToUpdate); で更新後のノートを返しても良い
        }

        // DELETE: api/notes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            if (_context.Notes == null)
            {
                return Problem("Entity set 'NotesDbContext.Notes' is null.");
            }

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound($"Note with ID {id} not found.");
            }

            // DbContext からノートを削除対象としてマーク
            _context.Notes.Remove(note);
            // データベースへの変更を保存
            await _context.SaveChangesAsync();

            // 成功時は 204 No Content を返す
            return NoContent(); 
        }

        // Lintリクエスト用のDTO
        public class LintRequestDto
        {
            public string? MarkdownText { get; set; }
        }

        // NoteUpdateDto をコントローラ内に一時的に定義 (後で別ファイルに移動推奨)
        public class NoteUpdateDto
        {
            // 更新時にはIDはURLから取得するので、DTOには含めなくても良いことが多い
            // public int Id { get; set; }
            public string? Title { get; set; } // 更新しない場合はnullを許容
            public string? MarkdownContent { get; set; } // 更新しない場合はnullを許容
        }

        // ヘルパーメソッド: 指定されたIDのノートが存在するか確認
        private bool NoteExists(int id)
        {
            return (_context.Notes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}