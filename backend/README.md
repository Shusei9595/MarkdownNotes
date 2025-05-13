# Markdown Notes API

これは、Markdown形式でノートを保存・管理するためのバックエンドAPIです。
C# と ASP.NET Core Web API を使用して構築されています。

## プロジェクトの目的

`roadmap.sh` のプロジェクト課題「Markdown Note Taking App」のバックエンド部分を実装します。
以下の主要な機能を提供します。

*   Markdownノートの保存
*   保存されたノートの一覧表示
*   指定されたノートのMarkdownコンテンツをHTMLに変換して提供
*   Markdownテキストの簡易的な構文チェック

## 前提条件

*   [.NET SDK](https://dotnet.microsoft.com/download) (バージョン X.X.X LTS推奨、ご自身の環境に合わせてください)
    *   `dotnet --version` で確認できます。
*   (任意) APIテストツール (Postman, Thunder Client, Insomnia など)

## セットアップと実行方法

1.  **リポジトリのクローン (もしあれば)**
    ```bash
    git clone <リポジトリのURL>
    cd MarkdownNotes.Api
    ```

2.  **必要なツールのインストール (dotnet-ef)**
    もし `dotnet ef` コマンドが利用できない場合は、グローバルツールとしてインストールしてください。
    ```bash
    dotnet tool install --global dotnet-ef
    ```

3.  **データベースのマイグレーション**
    アプリケーションが使用するデータベース (SQLite) のテーブルを作成します。
    ```bash
    dotnet ef database update
    ```
    これにより、プロジェクトルート（または実行ディレクトリ）に `markdown_notes.db` ファイルが作成されます。

4.  **アプリケーションの実行**
    ```bash
    dotnet run
    ```
    デフォルトでは `http://localhost:5000` または `http://localhost:5200` (launchSettings.jsonによる) でAPIサーバーが起動します。
    コンソールの出力で正確なURLを確認してください。

## APIエンドポイント

ベースURL: `http://localhost:XXXX` (XXXXはポート番号)

| HTTPメソッド | エンドポイント             | 説明                                       | リクエストボディ (例)                                            | レスポンス (例)                                                                                               |
| :----------- | :------------------------- | :----------------------------------------- | :--------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------ |
| `POST`       | `/api/notes`               | 新しいノートを作成します。                 | `{"title": "新しいノート", "markdownContent": "# こんにちは"}` | `201 Created` と作成されたノートオブジェクト (ID含む)                                                          |
| `GET`        | `/api/notes`               | 保存されている全てのノートを一覧表示します。 | (なし)                                                           | `200 OK` とノートオブジェクトの配列 (更新日時降順)                                                              |
| `GET`        | `/api/notes/{id}`          | 指定されたIDのノートを取得します。         | (なし)                                                           | `200 OK` とノートオブジェクト、見つからない場合は `404 Not Found`                                                |
| `GET`        | `/api/notes/{id}/html`     | 指定されたIDのノートをHTML形式で取得します。 | (なし)                                                           | `200 OK` とHTML文字列、見つからない場合は `404 Not Found`                                                        |
| `POST`       | `/api/notes/lint`          | Markdownテキストの構文を簡易チェックします。 | `{"markdownText": "## テスト"}`                                  | `200 OK` と `{"isValid": true/false, "message": "..."}`、パースエラー時は `400 Bad Request` とエラー情報 |

*(ここにPUT (更新) や DELETE (削除) のエンドポイントも将来的に追加できます)*

## 使用技術

*   C#
*   ASP.NET Core Web API
*   Entity Framework Core (ORM)
*   SQLite (データベース)
*   Markdig (Markdownパーサー)

## 今後の展望 (オプション)

*   ノートの更新 (PUT) 機能
*   ノートの削除 (DELETE) 機能
*   より高度なMarkdown Lint機能
*   認証・認可機能
*   フロントエンド (Vue 3) との連携

---
