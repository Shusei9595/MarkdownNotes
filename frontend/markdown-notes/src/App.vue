<script setup>
import { ref, computed } from 'vue'; // computed を追加
import { marked } from 'marked';   // marked をインポート

// ダミーデータ（変更なし）
const notes = ref([
  { id: 1, title: '最初のノート', content: '# これは最初のノートです\n\nようこそ！' },
  { id: 2, title: 'Vueについて', content: '## Vue 3\n\n- Composition API\n- Vite\n\n```javascript\nconsole.log("Hello Vue!");\n```' },
  { id: 3, title: 'Markdown記法', content: '*太字*\n_イタリック_\n`コード`\n\n1. 番号付きリスト1\n2. 番号付きリスト2\n\n> 引用文です。' },
]);

const currentNoteContent = ref('');
// currentPreviewHtml は computed プロパティに変更します

// 選択されているノートのIDを保持（アクティブなノートを示すためなど）
const selectedNoteId = ref(null);

// プレビュー用のHTMLを計算プロパティで生成
const currentPreviewHtml = computed(() => {
  if (currentNoteContent.value) {
    return marked(currentNoteContent.value); // markedを使ってMarkdownをHTMLに変換
  }
  return ''; // コンテンツがなければ空文字
});

function selectNote(note) {
  currentNoteContent.value = note.content;
  selectedNoteId.value = note.id;
  // updatePreview は不要になります (currentPreviewHtmlがリアクティブなため)
}

// updatePreview 関数は不要になるので削除します
// function updatePreview() { ... } // この関数を削除

// 初期状態で最初のノートを選択（もしあれば）
if (notes.value.length > 0) {
  selectNote(notes.value[0]);
}
</script>

<template>
  <div id="app-container">
    <header class="app-header">
      <h1>Markdown ノートアプリ</h1>
    </header>

    <main class="main-content">
      <aside class="sidebar-notes">
        <h2>ノート一覧</h2>
        <ul>
          <li
            v-for="note in notes"
            :key="note.id"
            @click="selectNote(note)"
            class="note-item"
            :class="{ 'active': note.id === selectedNoteId }"
          >
            {{ note.title }}
          </li>
        </ul>
        <button class="add-note-button">新しいノートを追加</button>
      </aside>

      <section class="editor-area">
        <h2>エディタ</h2>
        <textarea
          v-model="currentNoteContent"
          placeholder="Markdownで入力してください..."
        ></textarea>
      </section>

      <section class="preview-area">
        <h2>プレビュー</h2>
        <div v-html="currentPreviewHtml" class="markdown-preview"></div>
      </section>
    </main>

    <footer class="app-footer">
      <p>© 2024 あなたのMarkdownノートアプリ</p>
    </footer>
  </div>
</template>

<style scoped>
html, body { /* ルート要素の高さを100%に */
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden; /* アプリ全体でのスクロールバーを基本的には出さない */
}

#app-container {
  display: flex;
  flex-direction: column;
  height: 100vh; /* ビューポートの高さ全体を使用 */
  min-height: 100vh; /* 最小高さも指定 */
  font-family: sans-serif;
  background-color: #f8f9fa; /* アプリ全体の背景色（任意） */
}

.app-header {
  background-color: #e9ecef; /* 少し濃いめのヘッダー背景 */
  padding: 0.75rem 1rem; /* 少しスリムに */
  text-align: center;
  flex-shrink: 0; /* 高さが縮まないように */
  border-bottom: 1px solid #dee2e6;
}
.app-header h1 {
  margin: 0;
  font-size: 1.5rem; /* タイトル文字サイズ調整 */
}

.main-content {
  display: flex;
  flex-direction: row;
  flex-grow: 1; /* ヘッダーとフッター以外の残りの高さを全て使用 */
  overflow: hidden; /* 各カラムがはみ出さないように */
  background-color: #fff; /* メインコンテンツエリアの背景 */
}

.sidebar-notes {
  width: 280px; /* 少し幅を広げる */
  flex-shrink: 0;
  border-right: 1px solid #dee2e6;
  padding: 1rem;
  overflow-y: auto; /* ノートが多い場合にスクロール */
  background-color: #f8f9fa; /* サイドバー背景 */
  display: flex; /* サイドバー内で要素を縦に並べる */
  flex-direction: column;
}
.sidebar-notes h2 {
  margin-top: 0;
  margin-bottom: 0.75rem; /* h2の下マージン調整 */
  font-size: 1.1rem; /* h2文字サイズ調整 */
  flex-shrink: 0;
}
.sidebar-notes ul {
  list-style: none;
  padding: 0;
  margin: 0;
  flex-grow: 1; /* リストが利用可能なスペースを埋める */
  overflow-y: auto; /* リスト項目が多い場合にスクロール */
}
.note-item {
  padding: 0.6rem 0.8rem; /* 少しパディング調整 */
  cursor: pointer;
  border-bottom: 1px solid #e9ecef;
  transition: background-color 0.2s ease-in-out;
  font-size: 0.9rem; /* 文字サイズ調整 */
}
.note-item:last-child {
  border-bottom: none;
}
.note-item:hover {
  background-color: #e0e0e0;
}
.note-item.active {
  background-color: #007bff; /* アクティブな色をはっきりと */
  color: white;
  font-weight: 500; /* 少し太く */
}
.add-note-button {
  margin-top: 1rem;
  padding: 0.6rem 1rem;
  background-color: #28a745; /* ボタンの色変更（緑系） */
  color: white;
  border: none;
  border-radius: 5px; /* 少し丸みをつける */
  cursor: pointer;
  font-size: 0.9rem;
  text-align: center;
  flex-shrink: 0;
}
.add-note-button:hover {
  background-color: #218838;
}

.editor-preview-wrapper { /* エディタとプレビューを囲むラッパー */
  flex-grow: 1;
  display: flex;
  flex-direction: row;
  overflow: hidden; /* この中で overflow: auto を使うため */
}

.editor-area,
.preview-area {
  flex-grow: 1; /* スペースを分け合う基本設定 */
  flex-shrink: 1; /* スペースが足りない場合に縮むことを許可 */
  flex-basis: 50%; /* 親要素の利用可能幅の50%を初期サイズとする */
  /* width: 50%; */ /* flex-basis と併用、または単独で使用することも検討 */
  min-width: 0; /* これが重要！ flexアイテムがコンテンツ幅以下に縮むことを許可する */
                 /* または、ある程度の最小幅は維持したい場合: min-width: 300px; など */
  padding: 1rem;
  display: flex;
  flex-direction: column;
  height: 100%;
  box-sizing: border-box;
}
.editor-area {
  border-right: 1px solid #dee2e6;
}
.editor-area {
  border-right: 1px solid #dee2e6; /* プレビューとの境界線 */
}

.editor-area h2, .preview-area h2 {
  margin-top: 0;
  margin-bottom: 0.75rem;
  font-size: 1.1rem;
  flex-shrink: 0; /* h2が縮まないように */
}

.editor-area textarea {
  flex-grow: 1; /* エディタエリア内で利用可能な高さを全て使用 */
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ced4da;
  border-radius: 4px;
  box-sizing: border-box;
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, Courier, monospace;
  font-size: 0.95rem;
  line-height: 1.6; /* 行間を少し広げる */
  resize: none;
  overflow-y: auto; /* テキストエリア自体にスクロールバー */
}

.preview-area .markdown-preview {
  flex-grow: 1; /* プレビューエリア内で利用可能な高さを全て使用 */
  border: 1px solid #ced4da;
  padding: 1rem;
  background-color: #fff;
  border-radius: 4px;
  overflow-y: auto; /* プレビュー自体にスクロールバー */
  line-height: 1.6;
  font-size: 0.95rem; /* プレビューのフォントサイズ調整 */
}

.app-footer {
  background-color: #e9ecef;
  padding: 0.5rem 1rem; /* 少しスリムに */
  text-align: center;
  flex-shrink: 0;
  border-top: 1px solid #dee2e6;
  font-size: 0.85rem; /* フッター文字サイズ調整 */
}
.app-footer p {
  margin: 0;
}

/* Markdownプレビューの基本的なスタイリング (好みで調整してください) */
.markdown-preview :first-child {
  margin-top: 0;
}
.markdown-preview h1,
.markdown-preview h2,
.markdown-preview h3,
.markdown-preview h4,
.markdown-preview h5,
.markdown-preview h6 {
  margin-top: 1.2em;
  margin-bottom: 0.6em;
  font-weight: 600;
}
.markdown-preview h1 { font-size: 1.8em; }
.markdown-preview h2 { font-size: 1.5em; }
.markdown-preview h3 { font-size: 1.3em; }
.markdown-preview p {
  margin-bottom: 1em;
}
.markdown-preview ul,
.markdown-preview ol {
  margin-bottom: 1em;
  padding-left: 2em;
}
.markdown-preview li > p { /* リスト内のpタグの余白調整 */
  margin-bottom: 0.2em;
}
.markdown-preview pre {
  background-color: #f5f5f5;
  padding: 1em;
  border-radius: 4px;
  overflow-x: auto; /* 横スクロール */
  margin-bottom: 1em;
}
.markdown-preview code {
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, Courier, monospace;
  background-color: #f0f0f0; /* インラインコードの背景 */
  padding: 0.2em 0.4em;
  border-radius: 3px;
}
.markdown-preview pre > code { /* pre内のcodeは背景なし */
  background-color: transparent;
  padding: 0;
}
.markdown-preview blockquote {
  margin-left: 0;
  padding-left: 1em;
  border-left: 0.25em solid #dfe2e5;
  color: #6a737d;
  margin-bottom: 1em;
}
.markdown-preview table {
  border-collapse: collapse;
  margin-bottom: 1em;
  width: auto; /* テーブル幅をコンテンツに合わせる */
}
.markdown-preview th,
.markdown-preview td {
  border: 1px solid #ddd;
  padding: 0.5em 0.75em;
}
.markdown-preview th {
  background-color: #f9f9f9;
  font-weight: bold;
}
.markdown-preview hr {
  border: none;
  border-top: 1px solid #ddd;
  margin: 1.5em 0;
}

</style>