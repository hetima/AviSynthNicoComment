
# AviSynthNicoComment

ニコ生のコメントを表示する AviSynth プラグインです。ニコ生に特化しているため ue shita big small などには対応していません。フォントはBIZ UDPゴシック/明朝です。カラー絵文字対応。AAや運営コメントもそれなりに対応。1280*720の動画に12行のコメントを流すことを想定しています。他のサイズでは検証していません。

動画やコメントをダウンロードする機能は付いていません。他のツールで取得してください。

アンケートの表示は未実装です。対応予定。

release に64bitバイナリを置いてます。


## 使い方

`NicoComment` 関数で呼び出します。単体ではコメントしか描画しないので、Overlay などを使って元動画と重ねてください。

```
LoadPlugin("AviSynthNicoComment.dll")
src = DirectShowSource("movie.mp4")
comment = NicoComment(src, file="comment.xml")
Overlay(src, comment, mask=comment.ShowAlpha("rgb"))
```

DirectShowSource で音ずれする場合は FFmpegSource や LSMASHSource を推奨します。

```
LoadPlugin("LSMASHSource.dll")
LoadPlugin("AviSynthNicoComment.dll")
videoPath = "movie.mp4"
commentPath = "comment.xml"
src = LSMASHVideoSource(videoPath)
video = AudioDub(src, LSMASHAudioSource(videoPath))
comment = NicoComment(video, file=commentPath, shift=-600)
Overlay(video, comment, mask=comment.ShowAlpha("rgb"))
```

単体のコメント付き動画を作りたい場合は、avs スクリプトを ffmpeg などで変換してください。

## パラメータ

- `clip` 動画のクリップ。サイズや長さを参照するために必要です。必須
- `file` コメントxmlファイルのパス。必須
- `row` コメントの高さをピクセル数で指定。デフォルトは60（720で12行表示）
- `shift` 表示時間(vpos)の調整。単位は秒でvposに加算されます。分解能は1/100秒。コメントを遅らせるなら正の値、早めるなら負の値。デフォルトは0。
- `shiftb` shift と同じです。両方加算されます。おおまかな開始位置と細かい調整を分けてできるように2個用意しました。

日本語を含むファイルパスが読み込めない場合はshitjisで書くと良いかもしれません。


## その他

以下のライブラリを使用しています

- [niconicoCommentLibrary](https://github.com/kurema/niconicoCommentLibrary)
- [emoji.wpf](https://github.com/samhocevar/emoji.wpf) および [Typography](https://github.com/LayoutFarm/Typography)
