// src/services/api.js
import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'http://localhost:5200/api', // あなたのバックエンドAPIのベースURL
  headers: {
    'Content-Type': 'application/json',
  },
});

export default {
  getNotes() {
    return apiClient.get('/notes');
  },
  getNoteById(id) {
    return apiClient.get(`/notes/${id}`);
  },
  createNote(noteData) {
    return apiClient.post('/notes', noteData);
  },
  updateNote(id, noteData) {
    return apiClient.put(`/notes/${id}`, noteData);
  },
  deleteNote(id) {
    return apiClient.delete(`/notes/${id}`);
  },
  getNoteAsHtml(id) {
    return apiClient.get(`/notes/${id}/html`);
  },
  lintMarkdown(markdownText) {
    return apiClient.post('/notes/lint', { markdownText }); // { markdownText: markdownText } のショートハンド
  }
};