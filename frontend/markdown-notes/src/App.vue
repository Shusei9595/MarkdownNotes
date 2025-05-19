<script setup>
import { ref, computed, onMounted, watch } from 'vue'; // computed を追加
import { marked } from 'marked';   // marked をインポート
import api from './services/api';

// notes の初期値を空の配列に変更
const notes = ref([]); // APIから取得したノートを格納

const currentNoteContent = ref('');
// 選択されているノートのIDを保持（アクティブなノートを示すためなど）
const selectedNoteId = ref(null);
// 選択されたノートの初期のMarkdownテキスト (変更検知用)
const originalNoteContent = ref('');
// API通信中のローディング状態
const isLoading = ref(false);
// エラーメッセージ
const errorMessage = ref('');

// currentSelectedNote は、選択中のノートオブジェクト全体を保持するのに便利
const currentSelectedNote = computed(() => {
  return notes.value.find(note => note.id === selectedNoteId.value) || null;
});

// プレビュー用のHTMLを計算プロパティで生成
const currentPreviewHtml = computed(() => {
  if (currentNoteContent.value) {
    try {
      return marked(currentNoteContent.value);
    } catch (e) {
      console.error("Error in marked(): ", e);
      return "<p>Error rendering Markdown.</p>";
    }
  }
  return '';
});

// エディタの内容が変更されたかどうかを判定する computed プロパティ
const isNoteDirty = computed(() => {
  if (!currentSelectedNote.value) return false; // ノートが選択されていなければ変更なし
  return currentNoteContent.value !== originalNoteContent.value;
});

// ノートをAPIから取得する関数
async function fetchNotes() {
  isLoading.value = true;
  errorMessage.value = '';
  try {
    const response = await api.getNotes();
    notes.value = response.data; // APIからのレスポンスデータを notes にセット
    // もしノートがあれば、最初のノートを自動的に選択する
    if (notes.value.length > 0 && !selectedNoteId.value) {
      // APIから返るデータ構造に合わせて content プロパティ名を調整する必要があるかもしれません
      // バックエンドのNoteモデルは MarkdownContent という名前でした
      // response.data の各要素が { id, title, markdownContent, createdAt, updatedAt } の形になっているか確認
      selectNote(notes.value[0]);
    } else {
      // ノートがない場合はエディタとプレビューをクリア
      currentNoteContent.value = '';
      originalNoteContent.value = '';
      selectedNoteId.value = null;
    }
  } catch (error) {
    console.error('ノートの取得に失敗しました:', error);
    errorMessage.value = 'ノートの取得に失敗しました。';
  } finally {
    isLoading.value = false;
  }
}

function selectNote(note) {
  selectedNoteId.value = note.id;
  currentNoteContent.value = note.markdownContent || '';
  // 選択時の内容をオリジナルとして保存
  originalNoteContent.value = note.markdownContent || '';
}

// 新しいノートを追加するダミー関数 (後でAPI連携を実装)
async function addNewNote() {
  const newTitle = prompt("新しいノートのタイトルを入力してください:", "無題のノート");
  if (newTitle) {
    isLoading.value = true;
    errorMessage.value = '';
    try {
      const newNoteData = { title: newTitle, markdownContent: `# ${newTitle}\n\nここに内容を記述...` };
      const response = await api.createNote(newNoteData);
      await fetchNotes(); // ノート作成後はリストを再取得して表示を更新
      // 新しく作成されたノートを選択状態にする (任意)
      const createdNote = notes.value.find(n => n.id === response.data.id);
      if (createdNote) {
          selectNote(createdNote);
      }
    } catch (error) {
      console.error('ノートの作成に失敗しました:', error);
      errorMessage.value = 'ノートの作成に失敗しました。';
    } finally {
      isLoading.value = false;
    }
  }
}

async function saveCurrentNote() {
  if (!currentSelectedNote.value || !isNoteDirty.value) {
    // 保存対象がない、または変更がない場合は何もしない
    return;
  }
  isLoading.value = true;
  errorMessage.value = '';
  try {
    const noteToUpdate = {
      // バックエンドのNoteUpdateDtoに合わせて送信するデータを構築
      title: currentSelectedNote.value.title, // タイトルは維持
      markdownContent: currentNoteContent.value
    };
    await api.updateNote(currentSelectedNote.value.id, noteToUpdate);
    originalNoteContent.value = currentNoteContent.value; // 保存したのでオリジナルを更新
    console.log('ノートが保存されました！');

    // リスト内のノートの content (markdownContent) と updatedAt も更新する
    const index = notes.value.findIndex(n => n.id === selectedNoteId.value);
    if (index !== -1) {
      notes.value[index].markdownContent = currentNoteContent.value;
      notes.value[index].updatedAt = new Date().toISOString(); // フロントで仮の更新日時
    }
  } catch (error) {
    console.error('ノートの保存に失敗しました:', error);
    errorMessage.value = 'ノートの保存に失敗しました。';
  } finally {
    isLoading.value = false;
  }
}

onMounted(() => {
  fetchNotes();
});

</script>

<template>
  <div id="app-container">
    <header class="app-header">
      <h1>Markdown ノートアプリ</h1>
    </header>

    <main class="main-content">
      <aside class="sidebar-notes">
        <h2>ノート一覧</h2>
        <div v-if="isLoading && notes.length === 0" class="loading-message">読み込み中...</div>
        <div v-if="errorMessage && notes.length === 0" class="error-message">{{ errorMessage }}</div>
        <ul>
          <li
            v-for="note in notes"
            :key="note.id"
            @click="selectNote(note)"
            class="note-item"
            :class="{ 'active': note.id === selectedNoteId }"
          >
            {{ note.title }}
            <span v-if="note.id === selectedNoteId && isNoteDirty" class="dirty-indicator">*</span>
          </li>
          <li v-if="!isLoading && notes.length === 0 && !errorMessage" class="no-notes-message">
            ノートがありません。
          </li>
        </ul>
        <button @click="addNewNote" class="add-note-button" :disabled="isLoading">
          新しいノートを追加
        </button>
      </aside>

      <section class="editor-area">
        <div class="editor-header">
          <h2>エディタ</h2>
          <button @click="saveCurrentNote" v-if="currentSelectedNote && isNoteDirty" :disabled="isLoading" class="save-button">
            {{ isLoading ? '保存中...' : '変更を保存' }}
          </button>
        </div>
        <textarea
          v-if="currentSelectedNote || currentNoteContent"
          v-model="currentNoteContent"
          placeholder="Markdownで入力してください..."
          :disabled="isLoading"
        ></textarea>
        <div v-if="!currentSelectedNote && notes.length > 0 && !currentNoteContent" class="no-content-message">
          ノートを選択してください。
        </div>
      </section>

      <section class="preview-area">
        <h2>プレビュー</h2>
        <div v-if="currentSelectedNote || currentNoteContent" v-html="currentPreviewHtml" class="markdown-preview"></div>
        <div v-if="!currentSelectedNote && !currentNoteContent" class="no-content-message">
          ノートを選択するか、新しいノートを作成してください。
        </div>
      </section>
    </main>

    <footer class="app-footer">
      <p v-if="errorMessage" class="error-message-footer">{{ errorMessage }}</p>
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

.loading-message, .error-message {
  padding: 1rem;
  text-align: center;
  color: #6c757d;
}
.error-message {
  color: red;
}
.error-message-footer {
  color: red;
  font-weight: bold;
}

.dirty-indicator {
  color: orange;
  font-weight: bold;
  margin-left: 4px;
}

.editor-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem; /* h2のデフォルトマージンと合わせるか、h2のマージンを0にする */
}
.editor-header h2 {
  margin-bottom: 0; /* editor-headerでマージンを管理 */
}

.save-button {
  padding: 0.4rem 0.8rem;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
}
.save-button:hover {
  background-color: #0056b3;
}
.save-button:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

</style>